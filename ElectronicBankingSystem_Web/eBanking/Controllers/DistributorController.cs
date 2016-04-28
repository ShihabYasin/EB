using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.App_Start;
using eBanking.Interface;
using eBanking.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eBanking.Controllers
{
    [CustomAuth]
    public class DistributorController : Controller
    {
        #region Repository,Context And Variable Declaration

        private PinRepository pin_repo;
        private UserRoleRepository user_role_repo;
        private ICurrencyRepository currency_repo;
        //private StatusRepository status_repo;
        //private CurrencyRepository currency_repo;
        private IRatePlanRepository ratePlan_repo;
        public DistributorController()
        {
            pin_repo = new PinRepository();
            user_role_repo = new UserRoleRepository();
            currency_repo = new CurrencyRepository();
            ratePlan_repo = new RateplanRepository();
            //status_repo = new StatusRepository();
            //currency_repo = new CurrencyRepository();
        }
        #endregion



        /*---------------------------------------------------------
        *   this is Reseller Index
        *   reseller user can search their pin assigned by Admin User
        *   Search option 1. assigned Pin between two date 2. Activated PIn ,InActivated Pin ,All assigned PIn
        *   Index page by deafult show all Assigned Pin.
        *   In Pin model add a column ResellerUserID
        *---------------------------------------------------------*/
        //[HttpGet]
        //[ValidateAntiForgeryToken]
        public ActionResult DistributorTransaction(int? ServiceId, int? DistributorId, DateTime? FromDate, DateTime? ToDate, string CreatedBy, int? page, int? itemsPerPage, int? fromApi, string UserName)
        {
            int pageSize = (itemsPerPage ?? ConstMessage.ITEMS_PER_PAGE);
            ViewBag.ItemsPerPage = pageSize;
            int pageNumber = (page ?? 1);
           
            
            IServiceRepository service_repo = new ServiceRepository();
            ViewBag.Servicelist = new SelectList(service_repo.GetAllQueryable().Where(s => s.ParentId == ConstMessage.DistributorOpeartions).ToList(), "Id", "Name");
            IDistributorRepository distributor_repo = new DistributorRepository();
            if (fromApi == null || fromApi != ConstMessage.API_FROM || string.IsNullOrEmpty(UserName))
                UserName = User.Identity.Name;
            string userRole = user_role_repo.GetRoleByUserName(UserName);
            SelectList DistributorNameList = null;
            
             
            Distributor CurrentDistriubtor = null;
            IQueryable<int> distributorList = null;
            try
            {
                if (fromApi != null && fromApi == ConstMessage.API_FROM)
                {
                    CurrentDistriubtor = distributor_repo.Distributor_FindByUserName(UserName);
                }
                else
                {
                    CurrentDistriubtor = distributor_repo.Distributor_FindByUserName(User.Identity.Name);
                }
                
                distributorList = distributor_repo.Distributor_GetAllQueryable().Where(d=>d.DistributorCode.Contains(CurrentDistriubtor.DistributorCode)).Select(d=>d.DistributorId);
            }catch(Exception){ }

            // set DistributorId to current user if it found null
            if (DistributorId == null)
                DistributorId = CurrentDistriubtor.DistributorId;
            //if (FromDate == null)
            //    FromDate = DateTime.Today;
            //if (ToDate == null)
            //    ToDate = DateTime.Today;

            //ToDate.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
            //DateTime.Parse(ToDate.Value.ToShortDateString().Trim() + " 23:59:59");

            //IQueryable<DistributorTransaction> dt = distributor_repo.DistributorTransaction_Search(ServiceId, DistributorId, FromDate, new DateTime(ToDate.Value.Year, ToDate.Value.Month, ToDate.Value.Day, 23, 59, 59));//dt, 

            IQueryable<DistributorTransaction> dt = distributor_repo.DistributorTransaction_Search(ServiceId, DistributorId, FromDate, ToDate);//dt, 


            //Distributor dropdown list
            if (userRole == ConstMessage.ROLE_NAME_ADMIN)
            {
                DistributorNameList = new SelectList(distributor_repo.Distributor_GetAllQueryable().Where(d => d.IsActive == true).ToList(), "DistributorId", "UserName");
            }
            else if (CurrentDistriubtor != null) 
            {
                string subDistributorCodePrefix = CurrentDistriubtor.DistributorCode + '-';
                dt = dt.Where(d => distributorList.Contains(d.DistributorId));
                DistributorNameList = new SelectList(distributor_repo.Distributor_GetAllQueryable().Where(d => d.IsActive == true && (d.ParentId == CurrentDistriubtor.DistributorId || d.DistributorId == CurrentDistriubtor.DistributorId)).ToList(), "DistributorId", "UserName");
            }

            ViewBag.DistributorNameList = DistributorNameList;
            
            IPagedList<DistributorTransactionVM> Model = distributor_repo.DistributorTransactionVM(dt, pageNumber, pageSize);

            ViewBag.ServiceId = ServiceId;
            ViewBag.DistributorId = DistributorId;
            ViewBag.FromDate = FromDate;
            ViewBag.ToDate = ToDate;
            ViewBag.CreatedBy = CreatedBy;
            if (fromApi != null && fromApi == ConstMessage.API_FROM)
            {
                return Json(new { Model, Model.HasNextPage, Model.HasPreviousPage, Model.IsFirstPage, Model.IsLastPage, Model.PageCount, Model.PageNumber, Model.PageSize, Servicelist = ViewBag.Servicelist, DistributorNameList = ViewBag.DistributorNameList }, JsonRequestBehavior.AllowGet);
            }
            return View(Model);
        }

        [HttpGet]
        public ActionResult DistributorTransactionDetails(int? DistributorTransactionId, int? fromApi, string UserName)
        {
            List<DistributorTransactionDetailVM> vmodels = new List<DistributorTransactionDetailVM>();
            List<DistributorTransactionDetail> models = new List<DistributorTransactionDetail>();
            List<Transaction> mainTransaction=new List<Transaction>();
            Distributor CurrentDistriubtor = null;
            IDistributorRepository distributor_repo = new DistributorRepository();
            ITransactionRepository transaction_repo = new TransactionRepository();
            IServiceRepository service_repo=new ServiceRepository(); 
            try
            {
                if (fromApi != null && fromApi == ConstMessage.API_FROM)
                {
                    CurrentDistriubtor = distributor_repo.Distributor_FindByUserName(UserName);
                }
                else
                {
                    UserName = User.Identity.Name;
                    CurrentDistriubtor = distributor_repo.Distributor_FindByUserName(User.Identity.Name);
                }
            }
            catch (Exception) { }
            string userRole = user_role_repo.GetRoleByUserName(UserName);

            //var mainTransaction = distributor_repo.DistributorTransaction_GetAllQueryable().Where(dt=>dt.DTId == (int)DistributorTransactionId).FirstOrDefault();

            //if (userRole == ConstMessage.ROLE_NAME_ADMIN || mainTransaction.DistributorId == CurrentDistriubtor.DistributorId)
            //{
            //    models = distributor_repo.DistributorTransactionDetails_GetAllQueryable().Where(dtd => dtd.DTId == (int)DistributorTransactionId).ToList();
            //}
            models = distributor_repo.DistributorTransactionDetails_GetAllQueryable().Where(dtd => dtd.DTId == (int)DistributorTransactionId).ToList();
           
            var disTransaction=distributor_repo.DistributorTransactionDetails_GetAllQueryable().Where(dtd => dtd.DTId == (int)DistributorTransactionId).ToList();

           
                vmodels= (from dt in disTransaction
                         join mt in transaction_repo.GetAllQueryable() on  dt.UserTransactionNo equals mt.Id
                         join s in service_repo.GetAllQueryable() on mt.ServiceId equals s.Id 
                         select new {
                             dt.UserName,
                             dt.DistributorCommission,                          
                             mt.TransactionDate,
                             mt.ToUser,
                             mt.AmountOut,
                             mt.OperationNumber,
                             s.Name
                         }).Select(DisTransactionvm=>new DistributorTransactionDetailVM
                         {
                             Date=DisTransactionvm.TransactionDate,
                             UserName=DisTransactionvm.UserName,
                             RecipentNo=DisTransactionvm.ToUser,
                             Amount=DisTransactionvm.AmountOut,
                             UserTransactionNo=DisTransactionvm.OperationNumber,
                             DistributorCommission=DisTransactionvm.DistributorCommission,
                             ServiceName=DisTransactionvm.Name
                         }).ToList();
          

            if (fromApi != null && fromApi == ConstMessage.API_FROM)
            {
                return Json(new { vmodels }, JsonRequestBehavior.AllowGet);
            }
            return View(vmodels);
        }

        [HttpGet]
        public ActionResult DistributorPins(string Prefix, long? SerialNo, long? SerialNoTo, string BatchNo, string PinCode, int? Status, DateTime? FromDate, DateTime? ToDate, int? page, string Submit, string DistributorCode, bool? PinsInHand, int? fromApi, string ParentDistributor)
        {
            int pageSize = ConstMessage.ITEMS_PER_PAGE;
            int pageNumber = (page ?? 1);
            eBankingUser user = null;
            if(fromApi != null && fromApi == ConstMessage.API_FROM && !string.IsNullOrEmpty(ParentDistributor))
            {
                user = user_role_repo.FindUserByUserName(ParentDistributor);
            }
            else
            {
                ParentDistributor = HttpContext.User.Identity.Name;
                user = user_role_repo.FindUserByUserName(ParentDistributor);
            }
            
            IDistributorRepository distributor_repo = new DistributorRepository();
            PinViewModelTotal searchResult = new PinViewModelTotal();
            ViewBag.Prefixes = new SelectList(PinManagement.GetPinPrefixes(), "Prefix", "Prefix");
            ViewBag.Statuses = ConstMessage.PinStatuses;
            int totalPinCount = 0;
            decimal totalPinValue = 0;

            //IPagedList<PinViewModel> pinViewModel = null;
            if(user !=null)
            {
                try
                {
                    var currentDistributor = distributor_repo.Distributor_FindByUserName(ParentDistributor);
                    string userRole = user_role_repo.GetRoleByUserName(ParentDistributor);
                    if(currentDistributor != null)
                    {
                        if (!string.IsNullOrEmpty(DistributorCode))
                            searchResult = pin_repo.Search(pageNumber, pageSize, Prefix, SerialNo, SerialNoTo, PinCode, BatchNo, Status, FromDate, ToDate, DistributorCode, PinsInHand);
                        else
                            searchResult = pin_repo.Search(pageNumber, pageSize, Prefix, SerialNo, SerialNoTo, PinCode, BatchNo, Status, FromDate, ToDate, currentDistributor.DistributorCode, PinsInHand);
                        //searchResult = pin_repo.Search(pageNumber, pageSize, Prefix, SerialNo, SerialNoTo, BatchNo, Status, FromDate, ToDate, currentDistributor.DistributorCode);
                        ViewBag.SubDistributors = new SelectList(distributor_repo.Distributor_GetAllQueryable().Where(d => d.ParentId == currentDistributor.DistributorId).ToList(), "DistributorCode", "UserName");
                        ViewBag.CurrentDistributorCode = currentDistributor.DistributorCode;
                    }
                    else if (userRole == ConstMessage.ROLE_NAME_ADMIN)
                    {
                        ViewBag.SubDistributors = new SelectList(distributor_repo.Distributor_GetAllQueryable().Where(d => d.ParentId == 0).ToList(), "DistributorCode", "UserName");
                        searchResult = pin_repo.Search(pageNumber, pageSize, Prefix, SerialNo, SerialNoTo, PinCode, BatchNo, Status, FromDate, ToDate, "", PinsInHand);
                    }
                    
                    PinController pinController = new PinController();

                    DistributorTransaction dTransaction = new DistributorTransaction();
                    dTransaction.ConvertToUsd = 1;
                    dTransaction.CreatedBy = ParentDistributor;
                    dTransaction.CreatedOn = DateTime.Now;
                    dTransaction.CurrencyId = ConstMessage.SELECTED_USD_DESTINATION_ID;

                    if (Submit == "Assign" && !string.IsNullOrEmpty(DistributorCode))
                    {
                        totalPinCount = searchResult.TotalPins;
                        totalPinValue = searchResult.TotalPinValue;
                        var toDistributor = distributor_repo.Distributor_FindById(distributor_repo.GetDistributorIdFromDistributorCode(DistributorCode));
                        if(toDistributor.DistributorBalance < totalPinValue)
                        {
                            ViewBag.Message = "Requested distributor doesn't have sufficient balance. Current balance of the distributor is $" + toDistributor.DistributorBalance;

                            if (fromApi != null && fromApi == ConstMessage.API_FROM)
                            {
                                return Json(new
                                {
                                    #region json data
                                    searchResult,
                                    HasNextPage = searchResult.PinViewModel.HasNextPage,
                                    HasPreviousPage = searchResult.PinViewModel.HasPreviousPage,
                                    IsFirstPage = searchResult.PinViewModel.IsFirstPage,
                                    IsLastPage = searchResult.PinViewModel.IsLastPage,
                                    PageCount = searchResult.PinViewModel.PageCount,
                                    PageNumber = searchResult.PinViewModel.PageNumber,
                                    PageSize = searchResult.PinViewModel.PageSize,
                                    Servicelist = ViewBag.Servicelist,
                                    DistributorNameList = ViewBag.DistributorNameList,
                                    Prefixes = ViewBag.Prefixes,
                                    Message = ViewBag.Message,
                                    Statuses = ViewBag.Statuses,
                                    SubDistributors = ViewBag.SubDistributors,
                                    totalPinCount,
                                    totalPinValue,
                                    CurrentDistributorCode = ViewBag.CurrentDistributorCode
                                    #endregion
                                }, JsonRequestBehavior.AllowGet);
                            }
                            
                            return View(searchResult);
                        }

                        dTransaction.DistributorId = toDistributor.DistributorId;
                        dTransaction.AmountOutLocal = dTransaction.AmountOut = totalPinValue;
                        dTransaction.ServiceId = ConstMessage.STATUS_SERVICE_PIN_ASSIGN;
                        int? dtid = distributor_repo.DistributorTransaction_Add(dTransaction);

                        pinController.PinHandover(searchResult.PinIdList, DistributorCode, ConstMessage.STATUS_SERVICE_PIN_ASSIGN, dtid);
                        searchResult = pin_repo.Search(pageNumber, pageSize, Prefix, SerialNo, SerialNoTo, PinCode, BatchNo, Status, FromDate, ToDate, currentDistributor.DistributorCode, PinsInHand);
                        //var toDistributor = distributor_repo.Distributor_GetAllQueryable().Where(d=>d.DistributorCode == DistributorCode).FirstOrDefault().UserName;
                        ViewBag.Message = "Total " + totalPinCount + " pins of worth " + totalPinValue + " (USD) have been assigned to \"" + toDistributor.UserName + "\".";
                    }
                    else if (Submit == "Return" && currentDistributor.ParentId > 0)
                    {
                        totalPinCount = searchResult.TotalPins;
                        totalPinValue = searchResult.TotalPinValue;
                        var parentDistibutor = distributor_repo.Distributor_FindById(currentDistributor.ParentId);

                        dTransaction.DistributorId = currentDistributor.DistributorId;
                        dTransaction.AmountInLocal = dTransaction.AmountIn = totalPinValue;
                        dTransaction.ServiceId = ConstMessage.STATUS_SERVICE_PIN_RETURN;
                        int? dtid = distributor_repo.DistributorTransaction_Add(dTransaction);

                        pinController.PinHandover(searchResult.PinIdList, parentDistibutor.DistributorCode, ConstMessage.STATUS_SERVICE_PIN_RETURN, dtid);
                        ViewBag.Message = "Total " + totalPinCount + " pins of worth " + totalPinValue + " (USD) have been returned to \"" + parentDistibutor.UserName + "\".";
                        searchResult = pin_repo.Search(pageNumber, pageSize, Prefix, SerialNo, SerialNoTo, PinCode, BatchNo, Status, FromDate, ToDate, currentDistributor.DistributorCode, PinsInHand);
                    }
                    if (fromApi != null && fromApi == ConstMessage.API_FROM)
                    {
                        return Json(new
                        {
                            #region json data
                            searchResult,
                            HasNextPage = searchResult.PinViewModel.HasNextPage,
                            HasPreviousPage = searchResult.PinViewModel.HasPreviousPage,
                            IsFirstPage = searchResult.PinViewModel.IsFirstPage,
                            IsLastPage = searchResult.PinViewModel.IsLastPage,
                            PageCount = searchResult.PinViewModel.PageCount,
                            PageNumber = searchResult.PinViewModel.PageNumber,
                            PageSize = searchResult.PinViewModel.PageSize,
                            Servicelist = ViewBag.Servicelist,
                            DistributorNameList = ViewBag.DistributorNameList,
                            Prefixes = ViewBag.Prefixes,
                            Message = ViewBag.Message,
                            Statuses = ViewBag.Statuses,
                            SubDistributors = ViewBag.SubDistributors,
                            totalPinCount,
                            totalPinValue,
                            CurrentDistributorCode = ViewBag.CurrentDistributorCode
                            #endregion
                        }, JsonRequestBehavior.AllowGet);
                    }
                    return View(searchResult);       
                }
                catch (Exception) { }
            }
            if (fromApi != null && fromApi == ConstMessage.API_FROM)
            {
                return Json(new { Message = "Error" }, JsonRequestBehavior.AllowGet);
            }
            return View("Error");
        }

        [HttpGet]
        public JsonResult AssignPins(int? fromApi, string UserName)
        {
            //eBankingUser user = null;
            if (fromApi != null && fromApi == ConstMessage.API_FROM && !string.IsNullOrEmpty(UserName))
            {
                //user = user_role_repo.FindUserByUserName(UserName);
            }
            else
            {
                UserName = HttpContext.User.Identity.Name;
                //user = user_role_repo.FindUserByUserName(UserName);
            }
            DistributorRepository distributor_repo = new DistributorRepository();
            Distributor currentDistributor = distributor_repo.Distributor_FindByUserName(UserName);
            PinRepository pin_repo = new PinRepository();
            var result = pin_repo.AssignPinSummery(currentDistributor.DistributorCode);
            var DistributorNameList = new SelectList(distributor_repo.Distributor_GetAllQueryable().Where(d => d.IsActive == true && d.ParentId == currentDistributor.DistributorId).ToList(), "DistributorId", "UserName");
            //ViewBag.DistributorNameList = DistributorNameList;
            return Json(new { result, DistributorNameList }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AssignPins(IEnumerable<PinAssignViewModel> model, int? fromApi, string UserName)
        {
            DistributorRepository distributor_repo = new DistributorRepository();
            PinRepository pin_repo = new PinRepository();
            PinController pinController = new PinController();
            var currentDistributor = distributor_repo.Distributor_FindByUserName(UserName);
            List<int> pinIdList = new List<int>();
            Distributor toDistributor = new Distributor();
            decimal totalPinValue = 0;
            foreach (var item in model)
            {
                try
                {
                    var currentPins = pin_repo.GetAllQueryable().Where(p => p.DistributorCode.Equals(currentDistributor.DistributorCode) && p.IsActive == true && p.Status == ConstMessage.PIN_UN_USED_ID && p.Value == item.UnitValue).Take(item.AssignedQuantity).Select(p => new {p.Id, p.Value }).ToList();
                    pinIdList.AddRange(currentPins.Select(p=>p.Id).ToList());
                    totalPinValue += currentPins.Sum(p => p.Value);
                }
                catch (Exception) { }
                toDistributor = distributor_repo.Distributor_FindById(item.DistributorId);
            }
            if (toDistributor.DistributorBalance >= totalPinValue)
            {
                DistributorTransaction dTransaction = new DistributorTransaction();
                dTransaction.ConvertToUsd = 1;
                dTransaction.CreatedBy = currentDistributor.UserName;
                dTransaction.CreatedOn = DateTime.Now;
                dTransaction.CurrencyId = ConstMessage.SELECTED_USD_DESTINATION_ID;

                {
                    dTransaction.DistributorId = toDistributor.DistributorId;
                    dTransaction.AmountOutLocal = dTransaction.AmountOut = totalPinValue;
                    dTransaction.ServiceId = ConstMessage.STATUS_SERVICE_PIN_ASSIGN;
                    int? dtid = distributor_repo.DistributorTransaction_Add(dTransaction);

                    pinController.PinHandover(pinIdList, toDistributor.DistributorCode, ConstMessage.STATUS_SERVICE_PIN_ASSIGN, dtid);

                }
            }
            else
            {
                return Json(new { Message = "Requested distributor doesn't have sufficient balance. Current balance of the distributor is $" + toDistributor.DistributorBalance,
                                  ApiStatus = ConstMessage.STATUS_FAILED_ID},JsonRequestBehavior.AllowGet);
            }



            return Json(new { Message = "Total " + pinIdList.Count + " pins of worth " + totalPinValue + " (USD) have been assigned to \"" + toDistributor.UserName + "\".",
                              ApiStatus = ConstMessage.STATUS_COMPLETE_ID}, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult eBankingCashIn(int? fromApi, string userName)
        {
            IDistributorRepository distributor_repo = new DistributorRepository();
            AdminController admin = new AdminController();
            IdentityConfig ObjIdentity = new IdentityConfig();
            eBankingUser currentUser;
            if (fromApi != null && fromApi == ConstMessage.API_FROM && !string.IsNullOrEmpty(userName))
                currentUser = ObjIdentity.UserManager.FindByNameAsync(userName).Result;
            else
            {
                userName = User.Identity.Name;
                currentUser = ObjIdentity.UserManager.FindByNameAsync(userName).Result;
            }
            //eBankingUser currentUser = ObjIdentity.UserManager.FindByNameAsync(User.Identity.Name).Result;
            string userRole = admin.FindCurrentUserRole(currentUser);
            if(userRole == ConstMessage.ROLE_NAME_ADMIN)
                ViewBag.UserName = new SelectList(distributor_repo.Distributor_GetAllQueryable().Where(d => d.IsActive == true && d.ParentId == 0).ToList(), "DistributorId", "UserName");
            else if( userRole == ConstMessage.ROLE_NAME_WHOLESELLER)
            {
                int currentDistributorId = distributor_repo.Distributor_FindByUserName(userName).DistributorId;
                ViewBag.UserName = new SelectList(distributor_repo.Distributor_GetAllQueryable().Where(d => d.IsActive == true && d.ParentId == currentDistributorId).ToList(), "DistributorId", "UserName");
            }
            ViewBag.Currency = new SelectList(currency_repo.GetAll(), "Id", "ISO"); //todo - get only currency for active destinations
            
            if (fromApi != null && fromApi == ConstMessage.API_FROM)
                return Json(new { UserNameList = ViewBag.UserName, CurrencyList = ViewBag.Currency }, JsonRequestBehavior.AllowGet);
            else
                return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult eBankingCashIn(UserCashInViewModel model, string Submit, int? fromApi, string userName)
        {
            IDistributorRepository distributor_repo = new DistributorRepository();
            AdminController admin = new AdminController();
            IdentityConfig ObjIdentity = new IdentityConfig();
            eBankingUser currentUser = null;
            if(fromApi != null && fromApi == ConstMessage.API_FROM && !string.IsNullOrEmpty(userName))
                currentUser = ObjIdentity.UserManager.FindByNameAsync(userName).Result;
            else
            {
                userName = User.Identity.Name;
                currentUser = ObjIdentity.UserManager.FindByNameAsync(userName).Result;
            }
            string userRole = admin.FindCurrentUserRole(currentUser);
            if (userRole == ConstMessage.ROLE_NAME_ADMIN)
                ViewBag.UserName = new SelectList(distributor_repo.Distributor_GetAllQueryable().Where(d => d.IsActive == true && d.ParentId == 0).ToList(), "DistributorId", "UserName");
            else if (userRole == ConstMessage.ROLE_NAME_WHOLESELLER)
            {
                int currentDistributorId = distributor_repo.Distributor_FindByUserName(userName).DistributorId;
                ViewBag.UserName = new SelectList(distributor_repo.Distributor_GetAllQueryable().Where(d => d.IsActive == true && d.ParentId == currentDistributorId).ToList(), "DistributorId", "UserName");
            }
            ViewBag.Currency = new SelectList(currency_repo.GetAll(), "Id", "ISO"); //todo - get only currency for active destinations

            if (ModelState.IsValid)
            {
                ITransactionRepository transaction_repo = new TransactionRepository();
                IAdminRepository admin_repo = new AdminRepository();
                DistributorTransaction transaction = new DistributorTransaction();
                transaction.DistributorId = model.DistributorId;
                transaction.CreatedBy = (userName != null ? userName : "system");
                transaction.CreatedOn = DateTime.Now;

                if (Submit == "Cash In")
                {
                    //transaction.ServiceId = ConstMessage.STATUS_SERVICE_EBANKING_CASH_IN_ID;  //20 in service table
                    transaction.ServiceId = ConstMessage.SERVICE_DISTRIBUTOR_CASHIN_ID;
                    transaction.AmountIn = model.eBankingCredit;
                    transaction.AmountInLocal = model.Price;
                }
                else if (Submit == "Cash Out")
                {
                    decimal distributorCurrentBalance = (decimal)distributor_repo.Distributor_FindById(model.DistributorId).DistributorBalance;
                    if (distributorCurrentBalance < model.eBankingCredit)
                    {
                        ViewBag.Status = ConstMessage.STATUS_FAILED_ID;
                        ViewBag.Message = "Distributor doesnt have sufficient balance. Current balance of selected distributor is $" + distributorCurrentBalance;

                        if (fromApi != null && fromApi == ConstMessage.API_FROM)
                            return Json(new { Message = ViewBag.Message, ApiStatus = ViewBag.Status }, JsonRequestBehavior.AllowGet);
                        else
                            return View(model);
                    }
                    transaction.ServiceId = ConstMessage.STATUS_SERVICE_EBANKING_CASH_OUT_ID;  //31 in service table
                    transaction.AmountOut = model.eBankingCredit;
                    transaction.AmountOutLocal = model.Price;
                }
                transaction.CurrencyId = model.LocalCurrencyId;
                transaction.ConvertToUsd = model.ConversionRate;

                if (distributor_repo.DistributorTransaction_Add(transaction) != null)
                {
                    //transaction_repo.Add(newTransaction);
                    ViewBag.Status = ConstMessage.STATUS_COMPLETE_ID;
                    ViewBag.Message = "Transaction Successful.";
                    ModelState.Clear();

                    if (fromApi != null && fromApi == ConstMessage.API_FROM)
                        return Json(new { Message = ViewBag.Message, ApiStatus = ViewBag.Status }, JsonRequestBehavior.AllowGet);
                    else
                        return View();
                }
                else
                {
                    ViewBag.Status = ConstMessage.STATUS_FAILED_ID;
                    ViewBag.Message = "Transaction encountered an error. Please contact Support.";
                    if (fromApi != null && fromApi == ConstMessage.API_FROM)
                        return Json(new { Message = ViewBag.Message, ApiStatus = ViewBag.Status }, JsonRequestBehavior.AllowGet);
                    else
                        return View(model);
                }
            }
            else
                if (fromApi != null && fromApi == ConstMessage.API_FROM)
                    return Json(new { Message = ViewBag.Message, ApiStatus = ViewBag.Status }, JsonRequestBehavior.AllowGet);
                else
                    return View(model);
        }

        [HttpGet]
        public ActionResult DistributorComissionRateplanIndex(int? page, int? itemsPerPage, string requestedPage, int? fromApi, string UserName, int? ApiStatus, string Message)
        {
            int pageSize = (itemsPerPage ?? ConstMessage.ITEMS_PER_PAGE);
            ViewBag.ItemsPerPage = pageSize;
            ViewData["ItemsPerPageSelector"] = ConstMessage.ItemsPerPageSelector;
            int pageNumber = (page ?? 1);
            if (string.IsNullOrEmpty(requestedPage))
                requestedPage = "CreatedBy";

            if(ApiStatus != null)
            {
                ViewBag.ApiStatus = ApiStatus;
            }
            if (!string.IsNullOrEmpty(Message))
            {
                ViewBag.Message = Message;
            }

            eBankingDbContext db = new eBankingDbContext();
            IDistributorRepository distributor_repo = new DistributorRepository(db);
            IServiceRepository service_repo = new ServiceRepository(db);
            Distributor currentDisributor = null;
            if(fromApi != null && fromApi == ConstMessage.API_FROM && !string.IsNullOrEmpty(UserName))
                currentDisributor = distributor_repo.Distributor_FindByUserName(UserName);
            else
            {
                UserName = User.Identity.Name;
                currentDisributor = distributor_repo.Distributor_FindByUserName(UserName);
            }

            string userRole = user_role_repo.GetRoleByUserName(UserName);
    

            IQueryable<DistributorCommissionRateplanViewModel> DCRPList = null;
            if (currentDisributor != null)
            {
                if (requestedPage == "CreatedBy")
                    DCRPList = distributor_repo.DCRP_GetAllVMByDistributorId(currentDisributor.DistributorId).OrderBy(d => d.ServiceId);
                else
                {
                    DCRPList = distributor_repo.DCRP_GetAssignedToDCRP(currentDisributor.DistributorId).OrderBy(d => d.ServiceId);
                }
            }
            else
            {
                
                if (userRole == ConstMessage.ROLE_NAME_ADMIN)
                {
                    DCRPList = distributor_repo.DCRP_GetAllVMByDistributorId(null).OrderBy(d => d.ServiceId);
                }
            }

            if(DCRPList != null)
            {
                IPagedList<DistributorCommissionRateplanViewModel> Model = null;
                try
                {
                    Model = DCRPList.ToPagedList(pageNumber, pageSize);
                }
                catch (Exception) { }
                if (fromApi != null && fromApi == ConstMessage.API_FROM)
                {
                    if (Model != null)
                        return Json(new { Model, Model.HasNextPage, Model.HasPreviousPage, Model.IsFirstPage, Model.IsLastPage, Model.PageCount, Model.PageNumber, Model.PageSize }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { ModelStatus = "No Data" }, JsonRequestBehavior.AllowGet);
                }
                ViewBag.requestedPage = requestedPage;

                ViewBag.UserRole = userRole;
                return View(Model);
            }
            else
                return View("Error");
        }

        [HttpGet]
        public ActionResult DistributorComissionRateplanCreate(int? fromApi, string UserName)
        {
            eBankingDbContext db = new eBankingDbContext();
            IServiceRepository service_repo = new ServiceRepository(db);
            IDistributorRepository distributor_repo = new DistributorRepository(db);
            Distributor currentDistributor = null;
          
          
            if (fromApi != null && fromApi == ConstMessage.API_FROM && !string.IsNullOrEmpty(UserName))
                currentDistributor = distributor_repo.Distributor_FindByUserName(UserName);
            else
            {
                UserName = User.Identity.Name;
                currentDistributor = distributor_repo.Distributor_FindByUserName(UserName);

              
            }
            
            try
            {
                var serviceListFull = service_repo.GetAllQueryable().Where(s => s.IsActive == true)
                    .Where(s => s.ParentId == ConstMessage.SERVICES_MONEYTRANSFER_ID || s.ParentId == ConstMessage.SERVICES_TOPUP_ID)
                    .OrderBy(s=>s.ParentId).ThenBy(s=>s.DestinationId);
                if (currentDistributor != null)
                {
                    var adsList = distributor_repo.ADR_GetAllQueryable().Where(a=>a.IsActive == true && a.DistributorId == currentDistributor.DistributorId);
                    var dcrpList = distributor_repo.DCRP_GetAllQueryable();
                    //var serviceListFull = service_repo.GetAllQueryable().Where(s => s.ParentId == ConstMessage.SERVICES_MONEYTRANSFER_ID || s.ParentId == ConstMessage.SERVICES_TOPUP_ID);
                    var serviceList = ( from ads in adsList
                                        join rps in dcrpList on ads.RateplanId equals rps.DCRP_ID
                                        join svs in serviceListFull on rps.ServiceId equals svs.Id
                                        select new 
                                        {
                                          Id = svs.Id,
                                          Name = svs.Name
                                        }
                                      ).ToList();
                    ViewData["ServiceList"] = new SelectList(serviceList, "Id", "Name");
                    ViewData["DCRPList"] = (from ads in adsList
                                            join dcrp in dcrpList.Where(d=>d.IsActive) on ads.RateplanId equals dcrp.DCRP_ID
                                            select new VerifyDistributorCommission 
                                            {
                                                ServiceId = dcrp.ServiceId,
                                                IsPercentage = dcrp.IsPercentage,
                                                AllowedCommission = dcrp.Commission
                                        
                                            }).ToList();//dcrpList.ToList()
                }
                else
                {
                    
                    IRatePlanRepository rateplan_repo = new RateplanRepository(db);
                    var ratePlanList = rateplan_repo.GetAllQueryable().Where(r=>r.IsActive == true);
                    ViewData["ServiceList"] = new SelectList(serviceListFull.ToList(), "Id", "Name");
                    ViewData["DCRPList"] = (from svc in serviceListFull
                                            join rpl in ratePlanList on svc.Id equals rpl.ServiceId into ratePlanJoin
                                            from rpj in ratePlanJoin.DefaultIfEmpty()
                                            select new VerifyDistributorCommission
                                            {
                                                ServiceId = svc.Id,
                                                IsPercentage = rpj.MRPisPercentage,
                                                MRP = rpj.MRP,
                                                AllowedCommission = (rpj.MRP - (decimal)svc.value / (decimal)rpj.ConvertionRate)
                                            }).ToList();

                    string userRole = user_role_repo.GetRoleByUserName(UserName);
                    ViewBag.UserRole = userRole;
                }
            }
            catch (Exception) { }
            if (fromApi != null && fromApi == ConstMessage.API_FROM)
                return Json(new { ServiceList = ViewData["ServiceList"], DCRPList = ViewData["DCRPList"] }, JsonRequestBehavior.AllowGet);
            return View();
        }

        [HttpPost]
        public ActionResult DistributorComissionRateplanCreate(DistributorCommissionRateplan DCRP_model, int? fromApi, string UserName)
        {
            IDistributorRepository distributor_repo = new DistributorRepository();
            IRatePlanRepository rateplan_repo = new RateplanRepository();
            IServiceRepository service_repo = new ServiceRepository();
            SelectList DistributorNameList = null;

            Distributor currentDistributor = null;
            if (fromApi != null && fromApi == ConstMessage.API_FROM && !string.IsNullOrEmpty(UserName))
                currentDistributor = distributor_repo.Distributor_FindByUserName(UserName);
            else
            {
                UserName = User.Identity.Name;
                currentDistributor = distributor_repo.Distributor_FindByUserName(UserName);
               
                //if (currentDistributor != null)
                //{
                //    string subDistributorCodePrefix = currentDistributor.DistributorCode + '-';
                //    //dt = dt.Where(d => distributorList.Contains(d.DistributorId));
                //      DistributorNameList = new SelectList(distributor_repo.Distributor_GetAllQueryable().Where(d => d.IsActive == true && (d.ParentId == currentDistributor.DistributorId || d.DistributorId == currentDistributor.DistributorId)).ToList(), "DistributorId", "UserName");
                //}
            }

            string userRole = user_role_repo.GetRoleByUserName(UserName);
            if (userRole != "Admin")
            {
                DCRP_model.DistributorId = currentDistributor.DistributorId;
            }
            else
            {
                //RatePlan ratePlan = rateplan_repo.FindByService(DCRP_model.ServiceId);

                //decimal costWithCommission = DCRP_model.Commission + ratePlan.Cost;
                //hack: Validation is off due to MRP calculation need to be change in service rate plan creation
                //if (!(costWithCommission >= ratePlan.Cost && costWithCommission <= ratePlan.MRP))
                //{
                //    ViewData["ServiceList"] = new SelectList(service_repo.GetAll(true).ToList(), "Id", "Name");
                //    ViewBag.Message = "Commission Rate can not be greater than MRP";
                //    ViewBag.ApiStatus = ConstMessage.STATUS_FAILED_ID;
                //    if (fromApi != null && fromApi == ConstMessage.API_FROM)
                //        return Json(new { ApiStatus = ConstMessage.STATUS_FAILED_ID, Message = ViewBag.Message }, JsonRequestBehavior.AllowGet);
                //    return View(DCRP_model);
                //}
                //DCRP_model.DistributorId = 0;
                
            }
            if (ModelState.IsValid)
            {
                DCRP_model.CreatedBy = UserName;
                DCRP_model.CreatedOn = DateTime.Now;
                distributor_repo.DCRP_Create(DCRP_model);

                //var childDisList = DistributorNameList.Where(dt => dt.Value != currentDistributor.DistributorId.ToString()).ToList();
                //foreach (var id in DistributorNameList)
                //{
                //    int childDisId = Convert.ToInt16(id.Value);
                //    distributor_repo.DCRP_DeactivatePrevious(childDisId, DCRP_model.ServiceId);
                //}
                

                ViewBag.Message = "Rate Plan Creation Successful";
                ViewBag.ApiStatus = ConstMessage.STATUS_COMPLETE_ID;

                if (fromApi != null && fromApi == ConstMessage.API_FROM)
                    return Json(new { ApiStatus = ConstMessage.STATUS_COMPLETE_ID, Message = "Rate Plan Creation Successful" }, JsonRequestBehavior.AllowGet);

                return RedirectToAction("DistributorComissionRateplanIndex", new { ApiStatus = ViewBag.ApiStatus, Message = ViewBag.Message });
            }
         
            
            ViewData["ServiceList"] = new SelectList(service_repo.GetAll(true).ToList(), "Id", "Name");

            ViewBag.Message = "Commission Rate can not be greater than MRP";
            ViewBag.ApiStatus = ConstMessage.STATUS_FAILED_ID;

            if (fromApi != null && fromApi == ConstMessage.API_FROM)
                return Json(new { ApiStatus = ConstMessage.STATUS_FAILED_ID, Message = ViewBag.Message }, JsonRequestBehavior.AllowGet);
            return View(DCRP_model);
        }

        [HttpGet]
        public ActionResult CreateDistributor(string Message, int? ApiStatus)
        {
            ViewBag.Distributors = ConstMessage.Distributors;
            if(!string.IsNullOrEmpty(Message))
                ViewBag.Message = Message;
            if(ApiStatus != null)
                ViewBag.ApiStatus = ApiStatus;
            return View();
        }
        [HttpPost]
        public ActionResult CreateDistributor(string UserName, string Role, bool? IsActive, string parentUserName, int? fromApi)
        {
            IDistributorRepository distributor_repo = new DistributorRepository();
            AdminController adminController = new AdminController();
            ViewBag.Distributors = ConstMessage.Distributors;

            Distributor parentDistributor = new Distributor();
            if(!string.IsNullOrEmpty(parentUserName) && fromApi != null && fromApi == ConstMessage.API_FROM)
                parentDistributor = distributor_repo.Distributor_FindByUserName(parentUserName);
            else
                parentDistributor = distributor_repo.Distributor_FindByUserName(User.Identity.Name);//"60164434164"

            Distributor newDistributor = new Distributor();
            newDistributor.UserName = UserName;
            newDistributor.IsActive = (IsActive ?? false);
            newDistributor.ParentId = (parentDistributor != null? parentDistributor.DistributorId : 0);
            newDistributor.DistributorCode = (parentDistributor != null ? parentDistributor.DistributorCode : "");

            var user = user_role_repo.FindUserByUserName(UserName);
            if (user != null)
            {
                var distributorExists = distributor_repo.Distributor_FindByUserName(UserName);
                if (distributorExists != null && (fromApi == null || fromApi != ConstMessage.API_FROM))
                    return RedirectToAction("CreateDistributor",new {Message = "Distributor Exists", ApiStatus = ConstMessage.STATUS_FAILED_ID});
                else if (distributorExists != null && fromApi != null && fromApi == ConstMessage.API_FROM)
                    return Json(new { Message = "Distributor Exists", ApiStatus = ConstMessage.STATUS_FAILED_ID }, JsonRequestBehavior.AllowGet);

                adminController.UpdateUserRole(Role, UserName);

                if (distributor_repo.Distributor_Add(newDistributor))
                {
                    if(fromApi == null && fromApi != ConstMessage.API_FROM)
                        return Json(new { Message = "Distributor Created", ApiStatus = ConstMessage.STATUS_COMPLETE_ID }, JsonRequestBehavior.AllowGet);
                    else
                        return RedirectToAction("CreateDistributor", new { Message = "Distributor Created", ApiStatus = ConstMessage.STATUS_COMPLETE_ID });
                }
                    
            }
            else
            {
                AccountController accountController = new AccountController();
                accountController.ApiRegister(UserName,"","0");
                //if (!adminController.UpdateUserRole(Role, UserName))
                //    return View("Error occured while updating User Role");
                adminController.UpdateUserRole(Role, UserName);

                if (distributor_repo.Distributor_Add(newDistributor))
                {
                    if (fromApi == null && fromApi != ConstMessage.API_FROM)
                        return Json(new { Message = "Distributor Created", ApiStatus = ConstMessage.STATUS_COMPLETE_ID }, JsonRequestBehavior.AllowGet);
                    else
                        return RedirectToAction("CreateDistributor", new { Message = "Distributor Created", ApiStatus = ConstMessage.STATUS_COMPLETE_ID });
                }
                //if (fromApi == null && fromApi != ConstMessage.API_FROM)
                //    return Json(new { Message = "Distributor Created", ApiStatus = ConstMessage.STATUS_COMPLETE_ID }, JsonRequestBehavior.AllowGet);
                //else
                //    return RedirectToAction("CreateDistributor", new { Message = "Distributor Created", ApiStatus = ConstMessage.STATUS_COMPLETE_ID });
            }
            if (fromApi == null && fromApi != ConstMessage.API_FROM)
                return Json(new { Message = "Something went wrong, please contact admin.", ApiStatus = ConstMessage.STATUS_FAILED_ID }, JsonRequestBehavior.AllowGet);
            else
                return RedirectToAction("CreateDistributor", new { Message = "Something went wrong, please contact admin.", ApiStatus = ConstMessage.STATUS_FAILED_ID });
            //return View("Something went wrong, please contact admin.");
        }

        [HttpGet]
        public ActionResult Distributors(int? page, int? itemsPerPage, int? fromApi, string userName)
        {
            IDistributorRepository distributor_repo = new DistributorRepository();
            IAdminRepository admin_repo = new AdminRepository();
            
            int pageSize = (itemsPerPage ?? ConstMessage.ITEMS_PER_PAGE);
            ViewBag.ItemsPerPage = pageSize;
            ViewData["ItemsPerPageSelector"] = ConstMessage.ItemsPerPageSelector;
            int pageNumber = (page ?? 1);
            Distributor parent = null;
            string userRole = "";
            IEnumerable<Distributor> allChildDistributors = null;
            if(fromApi != null && fromApi == ConstMessage.API_FROM && !string.IsNullOrEmpty(userName))
                userRole = user_role_repo.GetRoleByUserName(userName);
            else
            {
                userName = User.Identity.Name;
                userRole = user_role_repo.GetRoleByUserName(userName);
            }

            if (userRole != "Admin")
            {
                parent = distributor_repo.Distributor_FindByUserName(userName);
                allChildDistributors = distributor_repo.Distributor_GetAllQueryable().Where(d => d.ParentId == parent.DistributorId).ToList();//d.DistributorCode.Contains(parentCode + "-")
            }
            else
            {
                allChildDistributors = distributor_repo.Distributor_GetAllQueryable().ToList();
            }
                
            var List  = admin_repo.SearchByUserAndRole("", "", null);
            List<DistributorViewModel> model = (from cDist in allChildDistributors
                                                join user in admin_repo.SearchByUserAndRole("", "", null) on cDist.UserName equals user.UserName
                                                select new DistributorViewModel
                                                {
                                                    DistributorId = cDist.DistributorId,
                                                    UserName = cDist.UserName,
                                                    DistributorCode = cDist.DistributorCode,
                                                    DistributorBalance = cDist.DistributorBalance,
                                                    DistributorType = user.RoleName,
                                                    IsActive = cDist.IsActive
                                                }).OrderByDescending(d=>d.DistributorType).ThenBy(d=>d.DistributorCode).ToList();
            
            IPagedList<DistributorViewModel> Model = model.ToPagedList(pageNumber,pageSize);
            if(fromApi != null && fromApi == ConstMessage.API_FROM)
                return Json(new { Model, Model.HasNextPage, Model.HasPreviousPage, Model.IsFirstPage, Model.IsLastPage, Model.PageCount, Model.PageNumber, Model.PageSize }, JsonRequestBehavior.AllowGet);
            return View(Model);
   
        }

        [HttpGet]
        public ActionResult AssignedRatePlans(int DistributorId, string Message, int?fromApi)
        {
            eBankingDbContext db = new eBankingDbContext();
            IServiceRepository service_repo = new ServiceRepository(db);
            IDistributorRepository distributor_repo = new DistributorRepository(db);

            var adsList = distributor_repo.ADR_GetAllQueryable().Where(a => a.IsActive == true && a.DistributorId == DistributorId);
            var dcrpList = distributor_repo.DCRP_GetAllQueryable();
            var serviceListFull = service_repo.GetAllQueryable();
            var assignedDCRPViewModels = (from ads in adsList
                               join rps in dcrpList on ads.RateplanId equals rps.DCRP_ID
                               join svs in serviceListFull on rps.ServiceId equals svs.Id
                               select new AssignedDCRPViewModel
                               {
                                   Commission = rps.Commission,
                                   DistributorId = rps.DistributorId,
                                   IsPercentage = rps.IsPercentage,
                                   RatePlanId = rps.DCRP_ID,
                                   ServiceId = svs.Id,
                                   ServiceName = svs.Name,
                                   ServiceCharge = rps.ServiceCharge,
                                   RateName = rps.RateName
                               }
                              ).ToList();
            ViewData["ServiceList"] = new SelectList(assignedDCRPViewModels, "ServiceId", "ServiceName");
            ViewData["DCRPList"] = (from ads in adsList
                                    join dcrp in dcrpList.Where(d => d.IsActive) on ads.RateplanId equals dcrp.DCRP_ID
                                    select new VerifyDistributorCommission
                                    {
                                        ServiceId = dcrp.ServiceId,
                                        IsPercentage = dcrp.IsPercentage,
                                        AllowedCommission = dcrp.Commission

                                    }).ToList();//dcrpList.ToList()
            ViewBag.DistributorId = DistributorId;
            ViewBag.DistributorName = distributor_repo.Distributor_FindById(DistributorId).UserName;
            ViewBag.Message = Message;

            if (fromApi != null && fromApi == ConstMessage.API_FROM)
                return Json(new { Model = assignedDCRPViewModels, ServiceList = ViewData["ServiceList"], DCRPList = ViewData["DCRPList"], DistributorId = ViewBag.DistributorId, DistributorName = ViewBag.DistributorName, Message = ViewBag.Message }, JsonRequestBehavior.AllowGet);

            return View(assignedDCRPViewModels);
        }
        [HttpGet]
        public ActionResult AssignRatePlanToDistributor(int DistributorId, int? fromApi, string UserName)
        {
            eBankingDbContext db = new eBankingDbContext();
            IServiceRepository service_repo = new ServiceRepository(db);
            IDistributorRepository distributor_repo = new DistributorRepository(db);
            try
            {
                Distributor currentDistributor = null;
                string userRole = "";
                if (fromApi != null && fromApi == ConstMessage.API_FROM && !string.IsNullOrEmpty(UserName))
                    currentDistributor = distributor_repo.Distributor_FindByUserName(UserName);
                else
                {
                    UserName = User.Identity.Name;
                    currentDistributor = distributor_repo.Distributor_FindByUserName(UserName);
                }

                IQueryable<DistributorCommissionRateplan> rateplansOfDistributor = null;
                userRole = user_role_repo.GetRoleByUserName(UserName);
                if (userRole != "Admin")
                {
                    int currentDistributorId = distributor_repo.Distributor_FindByUserName(UserName).DistributorId;
                    rateplansOfDistributor = distributor_repo.DCRP_GetAllQueryable().Where(d => d.IsActive == true && d.DistributorId == currentDistributorId);//ADR_GetAllQueryable().Where(a => a.IsActive == true && a.DistributorId == currentDistributorId);
                }
                else
                {
                    rateplansOfDistributor = distributor_repo.DCRP_GetAllQueryable().Where(d => d.IsActive == true && d.DistributorId == 0);
                }
                
                IQueryable<Service> serviceListFull = service_repo.GetAllQueryable();
                var serviceList = (from rod in rateplansOfDistributor
                                   join svs in serviceListFull on rod.ServiceId equals svs.Id
                                   select new AssignedDCRPViewModel
                                   {
                                       Commission = rod.Commission,
                                       DistributorId = rod.DistributorId,
                                       IsPercentage = rod.IsPercentage,
                                       RatePlanId = rod.DCRP_ID,
                                       ServiceId = svs.Id,
                                       ServiceCharge = rod.ServiceCharge,
                                       ServiceName = ((rod.RateName != null) ? (rod.RateName + " - ") : "") 
                                            + svs.Name + " - Commission " + rod.Commission 
                                            + (rod.IsPercentage == true ? " %" : " USD" )
                                            + ((rod.ServiceCharge != null && rod.ServiceCharge>0) ? (" - Service Charge " + rod.ServiceCharge.ToString() + " USD") : "")
                                   }
                                  ).ToList();
                ViewBag.DCRPSelectList = new SelectList(serviceList, "RatePlanId", "ServiceName");
                ViewBag.DCRPListViewModel = serviceList;
                ViewBag.DistributorId = DistributorId;
                ViewBag.DistributorUserName = distributor_repo.Distributor_FindById(DistributorId).UserName;
            }
            catch (Exception) { }
            if (fromApi != null && fromApi == ConstMessage.API_FROM)
                return Json(new { DCRPSelectList = ViewBag.DCRPSelectList, DCRPListViewModel = ViewBag.DCRPListViewModel, DistributorId = ViewBag.DistributorId, DistributorUserName = ViewBag.DistributorUserName }, JsonRequestBehavior.AllowGet);

            return View();
        }
        [HttpPost]
        public ActionResult AssignRatePlanToDistributor(AssignDistributorRateplan model, int? fromApi, string UserName)
        {
            string Message = "Assignment Failed";
            IDistributorRepository distributor_repo = new DistributorRepository();
            SelectList DistributorNameList = null;
            if (ModelState.IsValid)
            {
                try 
                {
                    int ratePlanOwnerId = distributor_repo.DCRP_GetById(model.RateplanId).DistributorId;
                    
                    Distributor currentDistributor = null;
                    string userRole = "";
                    if (fromApi != null && fromApi == ConstMessage.API_FROM && !string.IsNullOrEmpty(UserName))
                        currentDistributor = distributor_repo.Distributor_FindByUserName(UserName);
                    else
                    {
                        UserName = User.Identity.Name;
                        currentDistributor = distributor_repo.Distributor_FindByUserName(UserName);
                    }
                    userRole = user_role_repo.GetRoleByUserName(UserName);


                    /*
                     * Only the owner of the rateplan can assign that rateplan to another distributor
                     * Admin will also be able to assign rateplans that are created by any Admin user
                     */
                    if ((currentDistributor != null && ratePlanOwnerId == currentDistributor.DistributorId) || userRole == ConstMessage.ROLE_NAME_ADMIN)
                    {
                        IEnumerable<AssignDistributorRateplan> oldAssignments = distributor_repo.ADR_GetAllQueryable().Where(a => a.DistributorId == model.DistributorId && a.IsActive == true).ToList();
                        int currentServiceId = distributor_repo.DCRP_GetById(model.RateplanId).ServiceId;
                        IEnumerable<DistributorCommissionRateplan> AssignedRateplans = distributor_repo.DCRP_GetAllQueryable().Where(d => d.ServiceId == currentServiceId).ToList();

                        DistributorNameList = new SelectList(distributor_repo.Distributor_GetAllQueryable().Where(d => d.IsActive == true && (d.ParentId == model.DistributorId || d.DistributorId == model.DistributorId)).ToList(), "DistributorId", "UserName");

                        IEnumerable<AssignDistributorRateplan> changeUp = (from assignments in oldAssignments
                                                                           join rateplans in AssignedRateplans on assignments.RateplanId equals rateplans.DCRP_ID
                                                                           select assignments).ToList();



                        var childDisList = DistributorNameList.Where(dt => dt.Value != model.DistributorId.ToString()).ToList();
                        foreach (var id in DistributorNameList)
                        {
                            int childDisId = Convert.ToInt16(id.Value);
                            distributor_repo.DCRP_DeactivatePrevious(childDisId, currentServiceId);
                        }

                        bool previousUpdateSuccess = true;
                        foreach (var item in changeUp)
                        {
                            item.IsActive = false;
                            item.UpdatedBy = UserName;
                            item.UpdatedOn = DateTime.Now;
                            if (!distributor_repo.ADR_Edit(item))
                                previousUpdateSuccess = false;
                        }
                        if (previousUpdateSuccess)
                        {
                            model.IsActive = true;
                            model.CreatedBy = UserName;
                            model.CreatedOn = DateTime.Now;
                            if (distributor_repo.ADR_Create(model))
                                Message = "Assignment Complete";
                            else
                                Message = "Assignment Failed";
                        }
                    }
                    else
                    {
                        if (fromApi != null && fromApi == ConstMessage.API_FROM)
                            return Json(new { DistributorId = model.DistributorId, Message = Message }, JsonRequestBehavior.AllowGet);

                        return RedirectToAction("AssignedRatePlans", new { DistributorId = model.DistributorId, Message = Message });
                    }
                }
                catch (Exception) { }
                
            }
            if (fromApi != null && fromApi == ConstMessage.API_FROM)
                return Json(new { DistributorId = model.DistributorId, Message = Message }, JsonRequestBehavior.AllowGet);

            return RedirectToAction("AssignedRatePlans", new { DistributorId = model.DistributorId, Message = Message });
        }

        #region Helpers
        public decimal? GetDestributorBalance(string username)
        {
            try
            {
                //var username = User.Identity.Name;
                DistributorRepository dist_repo = new DistributorRepository();
                return dist_repo.Distributor_FindByUserName(username).DistributorBalance;
            }
            catch (Exception) { }
            return null;
        }
        public DistributorLastCommissionReturn GetDistributorLastCommission(string username)
        {
            try
            {
                DistributorRepository dist_repo = new DistributorRepository();
                var distId = dist_repo.Distributor_FindByUserName(username).DistributorId;
                var temp = dist_repo.DistributorTransaction_Search(ConstMessage.DistributorCommissionReturn, distId, null, null).OrderByDescending(dt => dt.CreatedOn).FirstOrDefault();//.Select(dt => new DistributorLastCommissionReturn { Amount = dt.AmountIn, Time = dt.CreatedOn })
                DistributorLastCommissionReturn result = new DistributorLastCommissionReturn();
                result.Amount = temp.AmountIn;
                result.Time = temp.CreatedOn;
                return result;
            }
            catch (Exception) { }
            return null;
        }

        
        #endregion

        [HttpGet]
        public ActionResult RetailerRegisteredUserIndex(int? fromApi, string UserName)
        {
            Distributor CurrentDistriubtor = null;
            string userRole = "";
            IEnumerable<RetailerRegisteredUserVM> userList=new List<RetailerRegisteredUserVM>();
            IDistributorRepository distributor_repo = new DistributorRepository();

            if (fromApi != null && fromApi == ConstMessage.API_FROM && !string.IsNullOrEmpty(UserName))
                CurrentDistriubtor = distributor_repo.Distributor_FindByUserName(UserName);
            else
            {
                UserName = User.Identity.Name;
                CurrentDistriubtor = distributor_repo.Distributor_FindByUserName(UserName);
            }

            userRole = user_role_repo.GetRoleByUserName(UserName);

            if (userRole==ConstMessage.ROLE_NAME_RETAILER)
            {
                CurrentDistriubtor = distributor_repo.Distributor_FindByUserName(UserName);

                userList = (from usr in user_role_repo.GetAllUser()
                            where usr.DistributorCode == CurrentDistriubtor.DistributorCode
                            select new RetailerRegisteredUserVM { UserNme = usr.UserName, Balance = usr.CurrentBalance }).ToList();

                if (fromApi != null && fromApi == ConstMessage.API_FROM)
                    return Json(new {Data=userList }, JsonRequestBehavior.AllowGet);
                else
                    return View(userList);
            }
           

            return View();
        }


    }
}
