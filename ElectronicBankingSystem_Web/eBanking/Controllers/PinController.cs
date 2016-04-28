using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.Interface;
using eBanking.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;



namespace eBanking.Controllers
{
    [CustomAuth]
    public class PinController : Controller
    {

        #region Repository, Context And Variable Declaration

        //private IPinRepository pin_repo;
        private ICurrencyRepository currency_repo;
        private IStatusRepository status_repo;
        private IUserRoleRepository user_role_repo;
        private IServiceRepository service_repo;

        private Variable _variable = new Variable();
        private List<Pin> CreatedPin=new List<Pin>();
        private UserMenuGenarator user_menu;
        IDistributorRepository distributor_repo;
        
        public PinController()
        {
            //pin_repo = new PinRepository();
            currency_repo = new CurrencyRepository();
            user_role_repo = new UserRoleRepository();
            status_repo = new StatusRepository();
            service_repo = new ServiceRepository();
            distributor_repo = new DistributorRepository();

            user_menu = new UserMenuGenarator();

        }
        #endregion


         /*---------------------------------------------------------
         *  It is Pin Manage GET method including search option by BatchNumber and PinCode
         *  first get all pin list and then all user list of Rseller Role to show in dropdownlist
         *  if pinlist is not null or exists then filter the pinlist whose status is NotUsed
         *  then filter the pinlist again if BatchNumber or PinCode or both are given for search
         *  next return the pinlist
         *  any error or exception return to Error page 
         *---------------------------------------------------------*/
        /*
         * Modified on - 8th September, 2015
         * Modified by - Siddique
         * Description - Pagination is implemented
         */

        [HttpGet]
        public ActionResult PinManage(string BatchNumber, string PinCode, decimal? Value, int? page, int? itemsPerPage)
       {
            IPinRepository pin_repo = new PinRepository();
            //parameters for pagination
            //int pageSize = ConstMessage.ITEMS_PER_PAGE;
            int pageSize = (itemsPerPage ?? ConstMessage.ITEMS_PER_PAGE);
            ViewBag.ItemsPerPage = pageSize;
            ViewData["ItemsPerPageSelector"] = ConstMessage.ItemsPerPageSelector;
            int pageNumber = (page ?? 1);
            ViewBag.BatchNumber = BatchNumber;
            ViewBag.PinCode = PinCode;
            ViewData["Distributor"] = distributor_repo.Distributor_GetAll();

            try
            {
                var ResId = pin_repo.GetAllUserOfaRole(ConstMessage.RESELLER_USER_ROLE_NAME).ToList();

                ViewBag.ResellerId = ResId;
                //user_role_repo.GetAllUserOfaRole(ConstMessage.RESELLER_USER_ROLE_NAME).ToList();
                
                IQueryable<Pin> PinList = pin_repo.GetAllQueryable().OrderBy(p=>p.Id);
                if (PinList != null)
                {
                    PinList = PinList.Where(x => x.Status == ConstMessage.PIN_UN_USED_ID);
                    
                    if (!string.IsNullOrEmpty(BatchNumber))
                    {
                        PinList = PinList.Where(x => x.BatchNumber.Contains(BatchNumber));
                    }

                    if (!string.IsNullOrEmpty(PinCode))
                    {
                        PinList = PinList.Where(x => x.PinCode.Contains(PinCode));
                    }
                    if(Value > 0)
                    {
                        ViewBag.Value = Value;
                        PinList = PinList.Where(x=>x.Value == Value);
                    }
                    var list = PinList.ToPagedList(pageNumber, pageSize);
                    return View(list);
                }
            }
            catch (Exception) { }
            return View("Error");
        }


         /*---------------------------------------------------------
         *   this method is used to active or deactive  a pin and 
         *    assign this pin to a reseller user
         *---------------------------------------------------------*/

        [HttpPost]
        public ActionResult PinManage(List<Pin> model, string DestributorCode)
        {
            int activatedPins = ActivatePins(model);//,DestributorCode
            return RedirectToAction("Index", new { message = "Total " + activatedPins + " activated." });
        }
        /*
         * Created on  - 26th october, 2015
         * Created by  - Siddique
         * Description - this function is created to separate the pin activation statements for multiple use. it will return
         *               the total number of pins that is activated on this process
         */
        private int ActivatePins(List<Pin> models)//, string DestributorCode
        {
            int numberOfPins = 0;
            IPinRepository pin_repo = new PinRepository();
            AdminRepository admin_repo = new AdminRepository();
            IDistributorRepository distributor_repo = new DistributorRepository();
            string destributorCode = "";
            decimal totalPinValue = 0;
            string pinNumbers = "";
            string pinPrefix = "";
            //long pinSerial = 0;
            PinHistory pinHistory = null;
            string submittedBy = User.Identity.Name;

            ViewData["Distributor"] = new SelectList(distributor_repo.Distributor_GetAll(), "DistributorCode", "UserName");

            try
            {
                models = models.OrderByDescending(m => m.DistributorCode).ThenBy(m=>m.PinPrefix).ThenBy(m=>m.SerialNo).ToList();
                foreach (var item in models)
                {
                    var pin = pin_repo.FindById(item.Id);

                    

                    //if item.IsActive will changed or item.ResellerUserID wil changed then edit a pin

                    if (item.IsActive != pin.IsActive) // || item.ResellerUserID!=pin.ResellerUserID
                    {
                        pin.IsActive = item.IsActive;
                        numberOfPins++;
                        pin.DistributorCode = item.DistributorCode;

                        //detect continuous pin serial number and insert pinHistory entry

                        if (pinPrefix != pin.PinPrefix || pinHistory.PinSerialTo != (pin.SerialNo - 1))
                        {
                            if (pinHistory != null)
                            {
                                pin_repo.PinHistory_Add(pinHistory);
                                pinHistory = null;
                            }
                            pinPrefix = pin.PinPrefix;
                            pinHistory = new PinHistory();
                            pinHistory.PinPrefix = pin.PinPrefix;
                            pinHistory.PinSerialFrom = pin.SerialNo;
                            pinHistory.PinSerialTo = pin.SerialNo;
                            pinHistory.AssignedBy = (distributor_repo.Distributor_FindByUserName(submittedBy) != null ? distributor_repo.Distributor_FindByUserName(submittedBy).DistributorCode : submittedBy);
                            pinHistory.AssignedOn = DateTime.Now;
                            pinHistory.AssignedTo = pin.DistributorCode;
                            pinHistory.EntryType = ConstMessage.PIN_HISTORY_ACTIVATE;
                        }
                        else
                        {
                            pinHistory.PinSerialTo = pin.SerialNo;
                        }

                        
                        if (destributorCode != pin.DistributorCode)
                        {
                            
                            if (!string.IsNullOrEmpty(destributorCode))
                            {
                                AcivatePinTransactionInsert(destributorCode, totalPinValue, pinNumbers);
                            }
                            
                            destributorCode = pin.DistributorCode;
                            pinNumbers = pin.Id.ToString();
                            if (pin.IsActive)
                                totalPinValue = pin.Value;
                            else
                                totalPinValue = -pin.Value;

                            if ((distributor_repo.GetDistributorBalanceByDistCode(pin.DistributorCode) ?? 0) >= totalPinValue)
                            {
                                bool result = pin_repo.Edit(pin);
                            }
                            else
                                continue;
                            
                        }
                        else
                        {
                            pinNumbers += ", " + pin.Id.ToString();
                            if (pin.IsActive)
                                totalPinValue += pin.Value;
                            else
                                totalPinValue -= pin.Value;
                            if ((distributor_repo.GetDistributorBalanceByDistCode(pin.DistributorCode) ?? 0) >= totalPinValue)
                            {
                                bool result = pin_repo.Edit(pin);
                            }
                            else
                                continue;
                        }
                    }

                }
                if(pinHistory != null)
                    pin_repo.PinHistory_Add(pinHistory);
                if (!string.IsNullOrEmpty(destributorCode) && (distributor_repo.GetDistributorBalanceByDistCode(destributorCode) ?? 0) >= totalPinValue)
                {
                    AcivatePinTransactionInsert(destributorCode, totalPinValue, pinNumbers);
                }
                //return true;
            }
            catch (Exception) { }
            return numberOfPins;
        }

        /*---------------------------------------------------------
        * pin index is resposible for two task 
        * first line is used to get created pin list while redirect to Index from pin Create 
        * the created pin are in TempData["CretedPinList"] 
        * when Index is not called for Create post method 
        * then it get all pin from pin_repo
        * to display pin with currency and assigned user we filter pin in eBankingTask.PinIndex method 
        * then display pinViewModel
        * exception return Error page 
        *---------------------------------------------------------*/

        // GET: /Pin/

        public ActionResult Index(string Prefix, int? SerialNoFrom, int? SerialNoTo, string BatchNumber, string PinCode, decimal? Value, string AssignedTo, string UsedBy, int? Status, bool? IsActive, DateTime? FromDate, DateTime? ToDate, int? page, int? itemsPerPage, string export, string message)
        {
            //SMSDR_Helper smsdr = new SMSDR_Helper();
            //smsdr.ReturnDistributorCommission();
            IPinRepository pin_repo = new PinRepository();
            IStatusRepository status_repo = new StatusRepository();
            
            ViewData["Distributor"] = new SelectList(distributor_repo.Distributor_GetAll(), "DistributorCode", "UserName");//user_role_repo.GetAllUserOfaRole(ConstMessage.RESELLER_USER_ROLE_NAME).ToList()
            ViewBag.Value = Value;
            ViewBag.UsedBy = UsedBy;
            int pageSize = (itemsPerPage ?? ConstMessage.ITEMS_PER_PAGE);
            ViewBag.ItemsPerPage = pageSize;
            ViewBag.AssignedTo = AssignedTo;
            if (Status != null)
                ViewBag.SelectedStatus = Status;
            int pageNumber = (page ?? 1);
            ViewData["PinPrefixes"] = new SelectList(PinManagement.GetPinPrefixes(), "Prefix", "Prefix");
            ViewBag.IsActiveList = new SelectList(new[] {   new { Value= true, Name = "True" },
                                                            new { Value = false, Name = "False" }
                                                        },"Value", "Name");
            ViewData["ItemsPerPageSelector"] = ConstMessage.ItemsPerPageSelector;
            ViewBag.Prefix = Prefix;
            ViewBag.SerialNoFrom = SerialNoFrom;
            ViewBag.SerialNoTo = SerialNoTo;
            ViewBag.IsActive = IsActive;
            ViewBag.Status = Status;
            ViewBag.StatusMsg = new SelectList(status_repo.GetAllQueryable().Where(s=>s.Id > 99 && s.Id < 103).ToList(),"Id","Name");
            ViewBag.Message = message;

            ViewBag.UserRole = user_role_repo.GetRoleByUserName(User.Identity.Name);

            try
            {
                string tempData = (string)TempData["CretedPinBatchNo"];
                string batchNumber = null;
                if (!string.IsNullOrEmpty(tempData))
                {
                    batchNumber = tempData;
                }
                else
                    batchNumber = BatchNumber;


                IPagedList<PinViewModel> pinViewModel = null;
                if (export == "Export Excel")
                {
                    IEnumerable<PinViewModel> exportPins = pin_repo.ExportPins(Prefix, SerialNoFrom, SerialNoTo, batchNumber, PinCode, Value, AssignedTo, UsedBy, Status, IsActive, FromDate, ToDate, pageNumber, pageSize);
                    PinRecords(exportPins.ToList());
                }
                else
                {
                    ViewBag.BatchNumber = batchNumber;
                    pinViewModel = pin_repo.PinSearch(Prefix, SerialNoFrom, SerialNoTo, batchNumber, PinCode, Value, AssignedTo, UsedBy, Status, IsActive, FromDate, ToDate, pageNumber, pageSize);
                }

                if (pinViewModel != null)
                    return View(pinViewModel);
                else
                    return View("Error");
            }
            catch (Exception ex)
            {
                string a = ex.Message.ToString();
            }
            return View("Error");

        }
        public void PinRecords(IEnumerable<PinViewModel> pinRecords) //string Prefix, int? SerialNoFrom, int? SerialNoTo, string BatchNumber, string PinCode, decimal? Value, string AssignedTo, string UsedBy, int? Status, bool? IsActive, DateTime? FromDate, DateTime? ToDate, int? page
        {
            //IPinRepository pin_repo = new PinRepository();
            GridView gv = new GridView();
           // var searchResult = Index(Prefix, SerialNoFrom, SerialNoTo, BatchNumber, PinCode, Value, AssignedTo, UsedBy, Status, IsActive, FromDate, ToDate, page);
           // var pinRecords = pin_repo.GetAllQueryable().ToList(); //.GetAllPinAsPinVMByBatchNumber(null, null, null, null, null, 3, 20);
            gv.DataSource = pinRecords;
            gv.DataBind();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename = PinRecords.xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            gv.RenderControl(htw);
            
            //Excel.Application excel = new Excel.Application();
            //excel.Visible = true;
            //Excel.Workbook wb = excel.Workbooks.Add(1);
            //Excel.Worksheet ws = (Excel.Worksheet)wb.Sheets[1];
            //ws.Cells[1, 1].EntireColumn.NumberFormat = "00";
            //wb.SaveAs(@"D:\Temp\aaexcel.xlsx");

            Response.Output.Write(sw.ToString());
            Response.Flush();
            sw.Close();
            Response.End();            
        }
        [HttpGet]
        public ActionResult BulkPinActivation(string PinPrefix, int? serialStart, int? serialEnd, string Submit)//, int? Confirmed
        {
            IPinRepository pin_repo = new PinRepository();
            ViewBag.serialStart = serialStart;
            ViewBag.serialEnd = serialEnd;

            ViewData["PinPrefixes"] = new SelectList(PinManagement.GetPinPrefixes(), "Prefix", "Prefix");
            if(!string.IsNullOrEmpty(PinPrefix) && serialStart > 0 && serialEnd > 0)
            {
                var pinsInRange = pin_repo.GetAllQueryable().Where(p => p.PinPrefix.Contains(PinPrefix) && p.SerialNo >= serialStart && p.SerialNo <= serialEnd && p.DistributorCode != null).ToList();
                if (Submit == "Activate Pins") //Confirmed != null && Confirmed == 2
                {
                    pinsInRange = pinsInRange.Where(p=>!p.IsActive).ToList();
                    foreach (var item in pinsInRange)
                    {
                        item.IsActive = true;
                    }
                    ViewBag.ActivatedPins = ActivatePins(pinsInRange);
                }
                else
                {
                    ViewBag.PinQuantity = pinsInRange.Count();
                    ViewBag.ActivePinQuantity = pinsInRange.Where(p => p.IsActive).Count();
                    ViewBag.InactivePinQuantity = pinsInRange.Where(p => !p.IsActive).Count();
                }
                
            }
            return View();
        }

        /*---------------------------------------------------------
        * get pin id ann collect all data of this id by filtering in eBankingTask.SinglePin and return
          
        *---------------------------------------------------------*/
        // GET: /Pin/Details/5
        public ActionResult Details(int? id)
        {
            IPinRepository pin_repo = new PinRepository();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pin pin =pin_repo.FindById(id);
            
            if (pin != null)
            {
                try
                {
                    var detailsPin = pin_repo.GetPinVMById(id);

                    if (detailsPin != null)
                    {
                        return View(detailsPin);
                    }
                }
                catch (Exception)
                {

                }
            }
           
            return HttpNotFound();
        }



        /*---------------------------------------------------------
        *  pin create page with two dropdownlist currency and ResellerId
        *---------------------------------------------------------*/
        // GET: /Pin/Create
        public ActionResult Create()
        {
            ViewData["Currency"] = new SelectList(currency_repo.GetAll().Where(x => x.DestinationId == ConstMessage.SELECTED_USD_DESTINATION_ID).ToList(), "Id", "CurrencyName", ConstMessage.SELECTED_USD_DESTINATION_ID);

            ViewData["Distributor"] = new SelectList(distributor_repo.Distributor_GetAll(), "DistributorCode", "UserName");//ViewData["ResellerId"] = new SelectList(user_role_repo.GetAllUserOfaRole(ConstMessage.RESELLER_USER_ROLE_NAME).ToList(), "UserName", "UserName");
            var pinPrefixSelectList = new SelectList(PinManagement.GetPinPrefixes(), "Prefix", "Prefix");
            ViewData["PinPrefixes"] = new SelectList(PinManagement.GetPinPrefixes(), "Prefix", "Prefix");
            ViewData["Services"] = new SelectList(service_repo.GetAll(true).Where(s=>s.IsGroup == false), "Id", "Name",ConstMessage.Service_Voucher_Recharge);
            return View();
        }



        /*---------------------------------------------------------
        *  Pin create option Number parameter contains how many pin we want to create
        *  ResellerId means Is the created pin assigned to any Reseller User 
        *  create pin serial number using PinManagement.GenerateSerialNumber() method
        *  create a pin code number using PinManagement.GeneratePinCode() method
        *  _variable.AllStringVar create pin BatchNumber and specifies how many pin of a specific value 
        *  are created in a date date formate is day-Month-Year-hours-minute
        *  CreatedPin.Add() method add all created pin to a list and assign in TempData["CretedPinList"]
        *  then redirect to index method and display all created pin in Index view 
        *  ViewData["Currency"] and  ViewData["ResellerId"] used to display Currency and reseller user in dropdownlist
        *---------------------------------------------------------*/
        // POST: /Pin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Pin pin, string Number, string DistributorCode, string PinPrefix)
        {
            IPinRepository pin_repo = new PinRepository();
            pin.IsActive = false;
            PinHistory pinHistory = new PinHistory();

            if (Number != null && !string.IsNullOrEmpty(Number))
            {
                if(!string.IsNullOrEmpty(DistributorCode))
                {
                    if (ModelState.IsValid)
                    {
                        _variable.Flag = Convert.ToInt32(Number);

                        _variable.AllStringVar = ConstMessage.PIN_BATCH_PREFIX + pin.Value + ":" + DateTime.Now.ToString("ddMMyyyyhhmm");

                        //Loop 1 t0 n times 

                        for (int i = 0; i < _variable.Flag; i++)
                        {
                            pin.DistributorCode = DistributorCode;
                            pin.IsActive = false;
                            pin.CreationDate = DateTime.Now;
                            pin.CreatedBy = User.Identity.Name;

                            //Auto Pin Code genaration 5 times if dupliacte pin 
                            _variable.count = 0;

                            pin.Status = ConstMessage.PIN_UN_USED_ID;


                            pin.PinCode = PinManagement.GeneratePinCode();

                            pin.BatchNumber = _variable.AllStringVar;

                            pin_repo.Add(pin);

                            //know to do the pin code unique replace first charecter of the pin by pinId
                            //pin.SerialNo = PinManagement.GenerateSerialNumber(pin.Id);
                            pin.PinPrefix = PinPrefix;
                            pin.SerialNo = PinManagement.GenerateSerialNumber(PinPrefix);
                            pin.PinCode = PinManagement.DoUniqueThePinCode(pin.PinCode, pin.Id);

                            //edit this PinCode so no duplicate pin genrate
                            pin_repo.Edit(pin);
                            TempData["CretedPinBatchNo"] = pin.BatchNumber;

                            if (pinHistory.PinSerialFrom < 1)
                            {
                                pinHistory.AssignedBy = User.Identity.Name;
                                pinHistory.AssignedOn = DateTime.Now;
                                pinHistory.AssignedTo = pin.DistributorCode;
                                pinHistory.EntryType = ConstMessage.PIN_HISTORY_CREATE;
                                pinHistory.PinPrefix = pin.PinPrefix;
                                pinHistory.PinSerialFrom = pin.SerialNo;
                                pinHistory.PinSerialTo = pin.SerialNo;
                            }
                            else
                            {
                                pinHistory.PinSerialTo = pin.SerialNo;
                            }
                            //CreatedPin.Add(new Pin { Id = pin.Id, SerialNo=pin.SerialNo, PinCode = pin.PinCode, BatchNumber = pin.BatchNumber, Value = pin.Value, CurrencyID = pin.CurrencyID, Status = pin.Status, CreationDate = pin.CreationDate });

                        }
                        pin_repo.PinHistory_Add(pinHistory);


                        return RedirectToAction("Index");
                    }
                    else
                        ModelState.AddModelError("Number", "The Distributor field is required!!");
                }
            }
            else
            {
                ModelState.AddModelError("Number", "The Number field is required!!");
            }

            ViewData["Currency"] = new SelectList(currency_repo.GetAll().Where(x => x.DestinationId == ConstMessage.SELECTED_USD_DESTINATION_ID).ToList(), "Id", "CurrencyName", ConstMessage.SELECTED_USD_DESTINATION_ID);
            ViewData["Distributor"] = new SelectList(distributor_repo.Distributor_GetAll(), "DistributorCode", "UserName");//ViewData["ResellerId"] = new SelectList(user_role_repo.GetAllUserOfaRole(ConstMessage.RESELLER_USER_ROLE_NAME).ToList(), "UserName", "UserName");
            ViewData["PinPrefixes"] = new SelectList(PinManagement.GetPinPrefixes(), "Prefix", "Prefix");
            ViewData["Services"] = new SelectList(service_repo.GetAll(true).Where(s => s.IsGroup == false), "Id", "Name");
            return View(pin);
        }


        /*---------------------------------------------------------
        *  Here is a block comment that draws attention
        *  to itself.
        *---------------------------------------------------------*/
        //Uri GET: /Pin/Edit/5
        public ActionResult Edit(int? id)
        {
            IPinRepository pin_repo = new PinRepository();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pin pin = pin_repo.FindById(id);
            ViewData["Currency"] = new SelectList(currency_repo.GetAll(), "Id", "CurrencyName", pin.CurrencyID);

            if (pin == null)
            {
                return HttpNotFound();
            }
            return View(pin);

        }



        /*---------------------------------------------------------
        *  Here is a block comment that draws attention
        *  to itself.
        *---------------------------------------------------------*/
        // POST: /Pin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Pin pin)
        {
            IPinRepository pin_repo = new PinRepository();
            if (ModelState.IsValid)
            {
                bool edit = pin_repo.Edit(pin);

                if (edit == true)
                    return RedirectToAction("Index");
                else
                    ModelState.AddModelError("", "Edit Failed.Please Try again.");

            }
            ViewData["Currency"] = new SelectList(currency_repo.GetAll(), "Id", "CurrencyName", pin.CurrencyID);

            return View(pin);
        }

        public List<PinViewModel> PinHandover(List<int> pinIds, string DistributorCode, int Type, int?DTId)
        {
            List<PinViewModel> assigned = new List<PinViewModel>();
            IPinRepository pin_repo = new PinRepository();
            IDistributorRepository distributor_repo = new DistributorRepository();
            PinHistory pinHistory = null;
            string pinPrefix = "";
            
            foreach (var id in pinIds)
            {
                Pin selectedPin = pin_repo.FindById(id);
                string prevDistributorCode = selectedPin.DistributorCode;
                selectedPin.DistributorCode = DistributorCode;
                pin_repo.Edit(selectedPin);
                assigned.Add(pin_repo.GetPinVMById(id));

                if (pinPrefix != selectedPin.PinPrefix || pinHistory.PinSerialTo != (selectedPin.SerialNo - 1))
                {
                    if (pinHistory != null)
                    {
                        pin_repo.PinHistory_Add(pinHistory);
                        pinHistory = null;
                    }
                    pinPrefix = selectedPin.PinPrefix;
                    pinHistory = new PinHistory();
                    pinHistory.PinPrefix = selectedPin.PinPrefix;
                    pinHistory.PinSerialFrom = selectedPin.SerialNo;
                    pinHistory.PinSerialTo = selectedPin.SerialNo;
                    pinHistory.AssignedBy = prevDistributorCode;
                    pinHistory.AssignedOn = DateTime.Now;
                    pinHistory.AssignedTo = selectedPin.DistributorCode;
                    pinHistory.EntryType = Type;
                    pinHistory.DTId = DTId;
                }
                else
                {
                    pinHistory.PinSerialTo = selectedPin.SerialNo;
                }
            }
            if (pinHistory != null)
                pin_repo.PinHistory_Add(pinHistory);
            return assigned;
        }




        /*---------------------------------------------------------
        *  Here is a block comment that draws attention
        *  to itself.
        *---------------------------------------------------------*/
        // GET: /Pin/Delete/5
        public ActionResult Delete(int? id)
        {
            IPinRepository pin_repo = new PinRepository();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Pin pin =pin_repo.FindById(id);

            if (pin != null)
            {

                try
                {
                    var detailsPin = pin_repo.GetPinVMById(id);//eBankingTask.SinglePin(pin_repo.GetAll(), currency_repo.GetAll(), status_repo.GetAll(), pin.Id);

                    if (detailsPin != null)
                        return View(detailsPin);

                }
                catch (Exception)
                {

                }
               
            }
            return HttpNotFound();
        }



        /*---------------------------------------------------------
        * get the deleted pin id from DeleteConfirmed(int id) parameter and through this parameter to 
        * pin repository Delete method this method return deleted pin entity when delete Successful  
        *---------------------------------------------------------*/
        //Uri POST: /Pin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            IPinRepository pin_repo = new PinRepository();
            Pin Pindelete = pin_repo.Delete(id);

            if (Pindelete != null)
                return RedirectToAction("Index");
            else
                ModelState.AddModelError("", "Delete Failed.Please Try again.");

            var detailsPin = pin_repo.GetPinVMById(id);//eBankingTask.SinglePin(pin_repo.GetAll(), currency_repo.GetAll(), status_repo.GetAll(), id);

            if (detailsPin == null)
            {
                return HttpNotFound();
            }
                return View(detailsPin);
           
        }

        private bool AcivatePinTransactionInsert(string distributorCode, decimal? totalPinValue, string pinNumbers)
        {

            ITransactionRepository transaction_repo = new TransactionRepository();
            //Transaction destributorTransaction = new Transaction();
            DistributorTransaction distributorTransaction = new DistributorTransaction();
            IDistributorRepository distributor_repo = new DistributorRepository();
            IAdminRepository admin_repo = new AdminRepository();

            distributorTransaction.DistributorId = distributor_repo.GetDistributorIdFromDistributorCode(distributorCode);
            distributorTransaction.AmountOut = totalPinValue;
            distributorTransaction.CreatedBy = User.Identity.Name;

            distributorTransaction.ServiceId = ConstMessage.STATUS_SERVICE_EBANKING_PIN_ACTIVATION;
            distributorTransaction.CurrencyId = ConstMessage.SELECTED_USD_DESTINATION_ID;
            distributorTransaction.ConvertToUsd = 1;

            
            try
            {
                if(distributor_repo.DistributorTransaction_Add(distributorTransaction) != null)
                    return true;
            }
            catch (Exception) { }
            return false;
        }

        /*---------------------------------------------------------
        *  if db context is open then Dispose the context hear  
        *---------------------------------------------------------*/
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
