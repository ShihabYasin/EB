using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.Interface;
using eBanking.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;

using System.Text;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel.DataAnnotations;


namespace eBanking.Controllers
{


    [CustomAuth]
    public class TransactionController : Controller
    {
        private ITransactionRepository tran_repo = new TransactionRepository();
        private IPinRepository pins = new PinRepository();
        private IEnumerable<Transaction> TransactionList;

        //private IEnumerable<Transaction> TransactionList;

        #region Repository,Context And Variable Declaration
        private Variable _variable=new Variable();
        private VendorLogHelper vLogHeper = new VendorLogHelper();
        private Helper _Helper = new Helper();
        public TransactionController()
        {
            
        }
      
        private UserMenuGenarator user_menu = new UserMenuGenarator();


        #endregion

        /*---------------------------------------------------------
       *  Manage Transfer is a option for customer support user to manage Money transfer requested by a user
       *  when a user do money transfer initially its status assigned as pending
       *  then customer support user change this status from pending to Processing,complete or failed
       *  This httpget  ManageTransfer display all transaction of Money Transfer Service 
       *  by filtering money transfer data using eBankingTask.TransactionReportsGenerator()  methode
       *  this method also perform as a serch Criteria .search option are
       *  UserNumber,status,money transfer between two date(From date ,To Date) 
       *---------------------------------------------------------*/

        [ActionName("ManageTransfer")]
        [HttpGet]

        public ActionResult ManageTransfer(string UserNumber, int? Status, DateTime? FromDate, DateTime? ToDate, int? CustomerSupport, string Message, int? page, int? itemsPerPage)
        {
            ITransactionRepository transaction_repo = new TransactionRepository();
            IStatusRepository status_repo = new StatusRepository();
            //parameters for pagination
            int pageSize = (itemsPerPage ?? ConstMessage.ITEMS_PER_PAGE);
            ViewBag.ItemsPerPage = pageSize;
            ViewData["ItemsPerPageSelector"] = ConstMessage.ItemsPerPageSelector;
            int pageNumber = (page ?? 1);
            if (!string.IsNullOrEmpty(UserNumber))
                ViewBag.UserNumber = UserNumber;

            if (Status != null)
                ViewBag.SelectedStatus = Status;
            if (FromDate != null)
                ViewBag.FromDate = FromDate;
            if (ToDate != null)
                ViewBag.ToDate = ToDate;
            if (CustomerSupport != null)
                ViewBag.CustomerSupport = CustomerSupport;
            if (!string.IsNullOrEmpty(Message))
                ViewBag.Message = Message;
            try
            {
                ViewBag.Result = Message;
               
                ViewBag.QueryStatus = Status; 

                var StatusList =status_repo.GetAll().ToList(); //need filter for type of status

                //Placing a placeholder to enable selecting all status
                var placeHolder = new StatusMsg();
                placeHolder.Id = 0;
                placeHolder.Name = "All Status";
                StatusList.Add(placeHolder);
                ViewBag.Status = StatusList;

                ChangeIsTimeout();

                IPagedList<Transaction> searchResult = transaction_repo.SearchPaged(pageNumber, pageSize, UserNumber, Status, null, ConstMessage.SERVICES_MONEYTRANSFER_ID, FromDate, ToDate);

                return View(searchResult);
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }

            return View("Error");
        }


        /*---------------------------------------------------------
       * This Http Post Manage Transfer is a option for customer support user to manage Money transfer requested by a user
       *  when a user do money transfer initially its status assigned as pending
       *  then customer support user change this status from pending to Processing,complete or failed
       *  if status is Canceled or Failed then money is back to coresponding user Acount
       *---------------------------------------------------------*/
        [Authorize]
        [ActionName("ManageTransfer")]
        [HttpPost]
        public ActionResult ManageTransfer(List<Transaction> model)
        {
            ITransactionRepository transaction_repo = new TransactionRepository();
            IStatusRepository status_repo = new StatusRepository();
            IAdminRepository admin_repo = new AdminRepository();
            //Transaction userBalanceReturn;

            _variable.Flag = 0;

            try
            {
                foreach(var item in model)
                {
                    if (item.IsCheck == true)
                    {
                        //userBalanceReturn = new Transaction();
                        //to do if Status is Cancelled or Failed then amount will be back
                        //ConstMessage.STATUS_CANCELED_MSG  
                        //ConstMessage.STATUS_FAILED_MSG
                        //Amount Reback From AmountOut field in transaction table 
                        if (item.Status == ConstMessage.STATUS_CANCELED_ID || item.Status == ConstMessage.STATUS_FAILED_ID)
                        {
                            _variable.Flag = 1;  //means requested status canceled or failed

                            //string userId = HttpContext.User.Identity.Name;

                            eBankingUser user = admin_repo.GetUserByUserName(item.UserId);
                                //.ToList();
                            _variable.Balance = item.AmountOut.GetValueOrDefault();

                            if (item.AmountOut.HasValue)//_variable.Balance != null
                            {
                                user.CurrentBalance = user.CurrentBalance + _variable.Balance;
                                transaction_repo.Return_User_Balance(item);
                                _variable.Flag = 2;
                            }
                                                      
                        }

                        //_variable.Flag == 0 means status is not canceled or failed
                        if (_variable.Flag == 0 || _variable.Flag == 2)
                        {
                            
                            transaction_repo.Edit(item);
                        }
                        else
                            ModelState.AddModelError("","Update Failed");
                    }
                }

                if (_variable.Flag == 0 || _variable.Flag == 2)
                {
                  return RedirectToAction("ManageTransfer", new { Message = "Update Success" });
                }
              }
            catch (Exception ex)
            {
                string a = ex.Message;
                ModelState.AddModelError("", "Update Failed");
            }


            ViewBag.Status = status_repo.GetAll();

            return View(model);
        }

        #region All_User_Transaction_for_AdminUser 
        //[HttpGet]
        public ActionResult TransactionHistory(string User, string Recipient, DateTime? FromDate, DateTime? ToDate, int? Destination, int? Status, int? Service, int? page, int? itemsPerPage, string PinCode, int? ClientId)  
         {
            
            ITransactionRepository transaction_repo = new TransactionRepository();
            IServiceRepository service_repo = new ServiceRepository();
            IStatusRepository status_repo = new StatusRepository();
            //IAdminRepository admin_repo = new AdminRepository();
            IDestinationRepository destination_repo = new DestinationRepository();

            //TransactionList = null;

            //parameters for pagination
            int pageSize = (itemsPerPage ?? ConstMessage.ITEMS_PER_PAGE);
            ViewBag.ItemsPerPage = pageSize;
            ViewData["ItemsPerPageSelector"] = ConstMessage.ItemsPerPageSelector;
            int pageNumber = (page ?? 1);
            if (!string.IsNullOrEmpty(User))
                ViewBag.User = User;
            if (Recipient != null)
                ViewBag.Recipient = Recipient;
            if (Status != null)
                ViewBag.Status = Status;
            if (FromDate != null)
                ViewBag.FromDate = FromDate;
            if (Destination != null)
                ViewBag.Destination = Destination;
            if (ToDate != null)
                ViewBag.ToDate = ToDate;
            if (Service != null)
                ViewBag._SelectedServiceId = Service;
            if (PinCode != null)
                ViewBag._SelectPinCode = PinCode;
             //PagedList.IPagedList<Transaction> data = null;
            //test for 
            //var  tDs = admin_repo.SmsdrGetAllQueryable().Where(s=>s.REQUEST_ID != "" && s.STATUS == ConstMessage.STATUS_PENDING_ID).ToList();//(from e in db.Smsdrs where e.REQUEST_ID != "" && e.STATUS == ConstMessage.STATUS_PENDING_ID select e).ToList();

            var StatusList = status_repo.GetAllQueryable().Where(x=>x.Id != ConstMessage.STATUS_MODELSTATE_ISVALID_FAILED).ToList();
            var ServiceList = service_repo.GetAll(true);
           
            //Placing a placeholder to enable selecting all status
            var placeHolder = new StatusMsg();
            placeHolder.Id = 0;
            placeHolder.Name = "All Status";
            StatusList.Add(placeHolder);

            ViewBag.StatusList = StatusList;
            ViewBag.DestinationList = destination_repo.GetAll().ToList();
            ViewBag.Service = ServiceList;

            var rawItems = service_repo.CreateTree(service_repo.GetAll(true),0); //
            var newList = new SelectList(rawItems, "Id", "Name", "ParentName", 1);
            ViewBag.Service2 = newList;

            var ClientList = tran_repo.GetClient();
            ViewBag.ClientList = new SelectList(ClientList, "ClientId", "Name");
            //var ClientList = new SelectList()
            // User, Recipient, FromDate, ToDate, Destination, Status, Service, page, itemsPerPage, PinCode, ClientId
            //IEnumerable<Pin> pinList = pins.GetAll();
            //IEnumerable<Transaction> TransactionHistory = transaction_repo.Search(ServiceList, User, Recipient, FromDate, ToDate, Destination, Status, Service, pinList, ClientId);
            IPagedList<TransactionHistory> tran = transaction_repo.TransactionHistory(User, Recipient, FromDate, ToDate, Destination, Status, Service, PinCode, ClientId, pageNumber, pageSize);

            if (tran != null)
                return View(tran);//tran.ToPagedList(pageNumber, pageSize));
            else
                return View("Error");            
        }

        #endregion

        #region Transaction TopUp and Money Transfer History of a specific User
        //this transaction history is only for Money and Top Up Service
        [NonAction]
        public IEnumerable<MoneyTopUpHistory> TransactionHistoryCalledFromServiceAPI(string UserName)
        {
            ITransactionRepository transaction_repo = new TransactionRepository();
            IServiceRepository service_repo = new ServiceRepository();
            IStatusRepository status_repo = new StatusRepository();

            IEnumerable<MoneyTopUpHistory> MoneyTopUpHistory = eBankingTask.Money_TopUp_History(transaction_repo.GetAll(), service_repo.GetAll(true), status_repo.GetAll(), UserName);
            return MoneyTopUpHistory;
            
        }
        #endregion


       /*---------------------------------------------------------
       *  Display all Money transfer History from db
       *  search this history of a specific month and year 
       *---------------------------------------------------------*/
        public ActionResult TransferHistory(int? month, int? year, int? page, string sortOrder, string currentFilter, int? itemsPerPage)
        {
            ViewBag.CurrecntFilter = sortOrder;
            
            int pageSize = (itemsPerPage ?? ConstMessage.ITEMS_PER_PAGE);
            ViewBag.ItemsPerPage = pageSize;
            ViewData["ItemsPerPageSelector"] = ConstMessage.ItemsPerPageSelector;
            ViewData["MonthSelector"] = ConstMessage.MonthSelector;
            ViewData["YearSelector"] = ConstMessage.YearSelector;
            if(month != null)
            {
                page = 1;
            }
            
            ViewBag.CurrentFilter = month;


            ITransactionRepository transaction_repo = new TransactionRepository();
            IServiceRepository service_repo = new ServiceRepository();
            IDestinationRepository destination_repo = new DestinationRepository();
            IRatePlanRepository ratePlan_repo = new RateplanRepository();
            IStatusRepository status_repo = new StatusRepository();

            _variable.UserName=null;
            try
            {
                _variable.UserName = HttpContext.User.Identity.Name;
               
            }
            catch (Exception ) { }

            if(_variable.UserName !=null && !string.IsNullOrEmpty(_variable.UserName))
            {
                ChangeIsTimeout();
                IEnumerable<TransferHistory> transferHistory = eBankingTask.TransferHistory_Creation(transaction_repo.GetAll(), service_repo.GetAll(true), destination_repo.GetAll(), ratePlan_repo.GetAll(), status_repo.GetAll(), _variable.UserName, month, year);
                
                int pageNumber = (page ?? 1);
                //ViewBag.Menu = user_menu.GenarateMenu(HttpContext.User.Identity.Name);
                return View(transferHistory.ToPagedList(pageNumber, pageSize));
            }
            return View("Error");
        }


        /*
         * Modified by Siddique
         * Update: trigger ChangeIsTimeout() and restrict if time limit is up
         * Modified On: 23rd July, 2015
         * Update: revise api response, change location for returning customer balance
         * Modified On: 2nd August, 2015
         **/
        public ActionResult CancelTransaction (int? Id, int? fromApi)
        {
            int ApiStatus;
            string Message;
            decimal Balance;
            decimal ConvertFromUsd;
            string ISO;

            ITransactionRepository transaction_repo = new TransactionRepository();
            IAdminRepository admin_repo = new AdminRepository();
            ICurrencyRepository currency_repo = new CurrencyRepository();
            UserManager<eBankingUser> UserManager = new UserManager<eBankingUser>(new UserStore<eBankingUser>(new eBankingDbContext()));


            ChangeIsTimeout();
            var model = transaction_repo.FindById(Id);

            eBankingUser user = UserManager.FindByName(model.UserId);
            _variable.Balance = model.AmountOut.GetValueOrDefault();

            ApiStatus = ConstMessage.STATUS_CANCELED_ID;
            Message = ConstMessage.STATUS_CANCELED_MSG;
            Balance = user.CurrentBalance;

            
            if (model.Status != 40 && !model.IsTimeOut)
            {
                model.Status = 40; //status message 40 is canceled
                model.IsTimeOut = true;
                try
                {
                    transaction_repo.Edit(model);
                    if (_variable.Balance > 0)
                    {
                        transaction_repo.Return_User_Balance(model);
                    }
                }
                catch (Exception) { }
                ViewBag.Message = "Transaction Canceled";
                
            }
            else
                ViewBag.Message = "Cancellation Time Limit Expired";

            Balance = user.CurrentBalance;//currency_repo.ConvertToLocal(user.CurrentBalance, user.LocalCurrencyId);
            ConvertFromUsd = currency_repo.GetConversionRate(user.LocalCurrencyId);
            ISO = admin_repo.GetUserCurrencyISOByUserName(user.UserName);
            if (fromApi == ConstMessage.TransactionFromApi)
            {
                
                if(model.Status == 40)
                {
                    ApiStatus = ConstMessage.STATUS_COMPLETE_ID;
                    Message = ConstMessage.STATUS_COMPLETE_MSG;
                }
                return Json(new { ApiStatus, Message, Balance, ISO, ConvertFromUsd }, JsonRequestBehavior.AllowGet);
            }
            return RedirectToAction("TransferHistory");
        }



       /*---------------------------------------------------------
       *  get transfer history as previous TransferHistory action
       *  then return as Json 
       *---------------------------------------------------------*/
        public ActionResult GetTransferHistory(string user, int? month, int? year)
        {
            ITransactionRepository transaction_repo = new TransactionRepository();
            IServiceRepository service_repo = new ServiceRepository();
            IDestinationRepository destination_repo = new DestinationRepository();
            IRatePlanRepository ratePlan_repo = new RateplanRepository();
            IStatusRepository status_repo = new StatusRepository();
            _variable.UserName = user;

            if (_variable.UserName != null && !string.IsNullOrEmpty(_variable.UserName))
            {
                ChangeIsTimeout();
                IEnumerable<TransferHistory> transferHistory = eBankingTask.TransferHistory_Creation(transaction_repo.GetAll(), service_repo.GetAll(true), destination_repo.GetAll(), ratePlan_repo.GetAll(), status_repo.GetAll(), _variable.UserName, month, year);

                return Json(transferHistory,JsonRequestBehavior.AllowGet);
            }
            string error = "Error";
            return Json(new{ error },JsonRequestBehavior.AllowGet);
        }



        /*---------------------------------------------------------
       *  get a user status
       *---------------------------------------------------------*/

        /*
         * comented out by Siddique
         * 26-08-2015
         */

        //[NonAction]
        //private eBankingUser GetUserStatus(string userId)
        //{
        //    try
        //    {
        //        var user = db.Users.Where(x => x.UserName == userId).Select(x => x).SingleOrDefault();
        //        return user;
        //    }
        //    catch (Exception)
        //    {

        //    }
        //    return null;
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        [NonAction]
        public bool ChangeIsTimeout ()
        {
            ITransactionRepository transaction_repo = new TransactionRepository();
            IServiceRepository service_repo = new ServiceRepository();
            IDestinationRepository destination_repo = new DestinationRepository();
            IStatusRepository status_repo = new StatusRepository();

            List<Transaction> tran = new List<Transaction>();
            try
            {
                //Cancel option is only for money transfer service
                tran = eBankingTask.TransactionReportsGenerator(transaction_repo.GetAll().Where(t => t.IsTimeOut == false).Select(t => t).ToList(), service_repo.GetAll(true).Where(x => x.ParentId == ConstMessage.SERVICES_MONEYTRANSFER_ID).Select(x => x).ToList(), status_repo.GetAll(), destination_repo.GetAll(), null);
            }
            catch (Exception) { }
            var now = DateTime.Now;
            foreach (Transaction item in tran)
            {
                /*this portion was written to rewrite all the previous IsTimeOut to true*/
                //if (item.TimeOut == null)
                //{
                //    try
                //    {
                //        item.IsTimeOut = true;
                //        Transaction change = db.Transactions.Find(item.Id);
                //        change.IsTimeOut = true;
                //        db.Entry(change).State = EntityState.Modified;
                //        db.SaveChanges();
                //    }
                //    catch (Exception ex) { }
                //}


                if (!item.IsTimeOut && item.TimeOut < now)
                {
                    item.IsTimeOut = true;
                    Transaction change = transaction_repo.FindById(item.Id);//db.Transactions.Find(item.Id);
                    change.IsTimeOut = true;
                    try
                    {
                        transaction_repo.Edit(change);
                        //db.Entry(change).State = EntityState.Modified;
                        //db.SaveChanges();
                    }
                    catch (Exception )
                    {
                        return false;
                    }
                    return true;
                }

            }
            return false;

        }

        public RegisteredUserStatusViewModel GetUserDashboardData(string userName)
        {
            ITransactionRepository transaction_repo = new TransactionRepository();
            IServiceRepository service_repo = new ServiceRepository();
            IDestinationRepository destination_repo = new DestinationRepository();
            IStatusRepository status_repo = new StatusRepository();

            RegisteredUserStatusViewModel data = new RegisteredUserStatusViewModel();
            List<Transaction> tran = new List<Transaction>();
            List<List<Transaction>> result = new List<List<Transaction>>();
            try
            {
                tran = eBankingTask.TransactionReportsGenerator(transaction_repo.GetAll().Where(t => t.UserId.Contains(userName) && t.Status < 31).OrderByDescending(t => t.TransactionDate).ToList(), service_repo.GetAll(true).Where(x => x.ParentId == ConstMessage.SERVICES_MONEYTRANSFER_ID || x.ParentId == ConstMessage.STATUS_SERVICE_CREDIT_MANAGEMENT_ID || x.ParentId == ConstMessage.SERVICES_TOPUP_ID).ToList(), status_repo.GetAll(), destination_repo.GetAll(), null);
               
            }
            catch (Exception) { }

            data.TopUp = eBankingTask.TransactionReportsGenerator(tran, service_repo.GetAll(true).Where(x => x.ParentId == ConstMessage.SERVICES_TOPUP_ID).Select(x => x).ToList(), status_repo.GetAll(), destination_repo.GetAll(), null).Take(5).ToList();

            data.MoneyTransfer = eBankingTask.TransactionReportsGenerator(tran, service_repo.GetAll(true).Where(x => x.ParentId == ConstMessage.SERVICES_MONEYTRANSFER_ID).Select(x => x).ToList(), status_repo.GetAll(), destination_repo.GetAll(), null).Take(5).ToList();
            //var temp = eBankingTask.TransactionReportsGenerator(tran, service_repo.GetAll(true).Where(x => x.Id == ConstMessage.STATUS_SERVICE_CREDIT_TRANSFER_ID).ToList(), status_repo.GetAll(), destination_repo.GetAll(), null).Take(5).ToList();
            data.CreditTransfer = eBankingTask.TransactionReportsGenerator(tran.Where(t=>t.ToUser != null).ToList(), service_repo.GetAll(true).Where(x => x.Id == ConstMessage.STATUS_SERVICE_CREDIT_TRANSFER_ID).ToList(), status_repo.GetAll(), destination_repo.GetAll(),1).Take(5).ToList();
            
            return data;
        }

        /*
         * Developed by Siddique
         * Created on: 8th August, 2015
         * Created for: making a list of all pending moneyTransfer requests
         *
         * modified on: 27th August, 2015
         * Modification on: 1. check if vendor have a previous unsubmitted VendorRequest, if does, repopulate that data and send.
         *                  otherwise populate new transaction requests and send
         *                  2. Vendor maximum pending transaction number is implemented (VendorMaxGet)
         */
        public updatePendingMoneyTransfer PendingMoneyTransfer(int? VendorId)
        {
            ITransactionRepository transaction_repo = new TransactionRepository();
            IServiceRepository service_repo = new ServiceRepository();
            IDestinationRepository destination_repo = new DestinationRepository();
            IStatusRepository status_repo = new StatusRepository();
            IVendorRepository vendor_repo = new VendorRepository();

            bool vendorAccess = true;
            List<VendorRequestLog> checkVendorAccss = new List<VendorRequestLog>();
            VendorRequestLog previousRequest = new VendorRequestLog();
            updatePendingMoneyTransfer result = new updatePendingMoneyTransfer();
            List<Transaction> model = new List<Transaction>();

            if(VendorId != null)
            {
                checkVendorAccss = vendor_repo.GetActiveVendorReqWithDetailsByVendorId((int)VendorId).Where(v => v.Type == "GET" && v.Confirmed == true).Include(v => v.Details).ToList();
                
                /*
                 * Checks if unsubmitted request for this vendor exists
                 * if it does, repopulates the old request and sends the previous request
                 */
                foreach(var vReq in checkVendorAccss)
                {
                    var item = vendor_repo.GetVendorReqWithReqId(vReq.RequestId).Where(v => v.Type == "POST").FirstOrDefault();
                    if ( item == null)
                    {
                        vendorAccess = false;
                        previousRequest = vReq;
                        break;
                    }
                }
                result.ReqId = previousRequest.RequestId;
                List<Transaction> transactionPrev = new List<Transaction>();
                try
                {
                    foreach (var item in previousRequest.Details)
                    {
                        //result.list.Add(_Helper.convertTransactionToPMT(db.Transactions.Find(item.TransactionId)));
                        transactionPrev.Add(transaction_repo.FindById(item.TransactionId));//db.Transactions.Find(item.TransactionId)
                    }
                }
                catch (Exception) { }
                

                if(!vendorAccess)
                {
                    model = eBankingTask.TransactionReportsGenerator(transactionPrev.OrderByDescending(t => t.TransactionDate).ThenBy(t => t.TransactionDate.Value.TimeOfDay).ToList(), service_repo.GetAll(true).Where(x => x.ParentId == ConstMessage.SERVICES_MONEYTRANSFER_ID).Select(x => x).ToList(), status_repo.GetAll(), destination_repo.GetAll(), null);
                    result.list = _Helper.convertTransactionList(model);
                    return result;
                }
            }
            
            /*
             * executes only if no previous unsubmitted record was found
             * gets new data and assigns them to vendor
             */

            model = eBankingTask.TransactionReportsGenerator(transaction_repo.GetAll().Where(t => t.Status == ConstMessage.STATUS_PENDING_ID && t.VendorId == 0).OrderBy(t => t.TransactionDate).ToList(), service_repo.GetAll(true).Where(x => x.ParentId == ConstMessage.SERVICES_MONEYTRANSFER_ID).Select(x => x).ToList(), status_repo.GetAll(), destination_repo.GetAll(), null);
            result.list = _Helper.convertTransactionList(model);
            
            result.ReqId = vLogHeper.InsertVendorLog(result.list, 1, "GET",null);
            return result;
        }
        /*
         * Created By - Siddique
         * Created On - 10th August. 2015
         * Created For - Receiving a confirmation from vendor that they have received a valid list of pending money transfer and updating VendorRequestLog confirmation box
         *               finally updating the moneyTransfer statuses to "processing (status code  = 20 )"
         *               
         * 
         * Modification needed - give proper error codes and revise current status codes
         */
        [AllowAnonymous]
        public int VendorGetConfirmation(VendorReceivingConfirmation vrc)
        {
            ITransactionRepository transaction_repo = new TransactionRepository();
            IVendorRepository vendor_repo = new VendorRepository();

            int result = 0;
            try
            {
                var validVendorRequest = vendor_repo.GetVendorReqWithReqId(vrc.ReqId).Where(v => v.Type == vrc.Type && v.VendorId == vrc.VendorId).Include(v => v.Details).ToList();//db.VendorRequestLogs.Where(v => v.RequestId == vrc.ReqId && v.Type == vrc.Type && v.VendorId == vrc.VendorId).Include(v => v.Details).Select(v => v).ToList();
                if (validVendorRequest.Count() != 1)
                {
                    result = 0; //invalid data
                }
                else
                {
                    validVendorRequest[0].Confirmed = true;
                    vendor_repo.VendorRequestLog_Edit(validVendorRequest[0]);

                    foreach (var item in validVendorRequest[0].Details)
                    {
                        var MT = transaction_repo.FindById(item.TransactionId);
                        if (MT.Status == ConstMessage.STATUS_PENDING_ID)
                        {
                            MT.Status = ConstMessage.STATUS_PROCESSING_ID;
                            transaction_repo.Edit(MT);
                        }
                        else
                        {
                            result = 2;   //executed with error
                        }
                    }
                    //db.SaveChanges();
                    result = ConstMessage.STATUS_COMPLETE_ID;
                }
                
            }
            catch (Exception ex) {

                string m = ex.Message;
                result = 2;    //executed with error
                return result;
            }

            return result;
        }


        /*
         *Developed by Siddique
         *Created on: 8th August, 2015
         *Created for: updating moneyTransfer status 
         *
         *Updated on: 20th August, 2015
         *            validation by vendor log is implemented - if log request doesnt exits, it will not save
         *            
         * Updated on: 27th August, 2015
         *              added STATUS_VENDOR_PROCESSING_ID with the status changing
         *
         *Modification needed : 1. revise more cases about when to update the status and when not to - i.e - if vendor didnt confirm getting the data, it will not be saved (check current status and changed to status more accurately)
         *                      2. give proper error codes and revise current status codes
         */
        [AllowAnonymous]
        public int UpdateMoneyTransferStatus(updatePendingMoneyTransfer model) //
        {
            ITransactionRepository transaction_repo = new TransactionRepository();
            IAdminRepository admin_repo = new AdminRepository();
            IVendorRepository vendor_repo = new VendorRepository();
            UserManager<eBankingUser> UserManager = new UserManager<eBankingUser>(new UserStore<eBankingUser>(new eBankingDbContext()));

            var _validateReq = vendor_repo.GetVendorReqWithReqId(model.ReqId).ToList();//db.VendorRequestLogs.Where(v => v.RequestId == model.ReqId).ToList();
            int result = 0;
            Transaction transaction = new Transaction();
            if (_validateReq.Count() == 1 && _validateReq[0].Type.Equals("GET") && _validateReq[0].Confirmed)      //check if more than one entry exists for this reqId -> will confirm 
            {
                foreach (var item in model.list)  //this should have a db transaction
                {
                    transaction = transaction_repo.FindById(item.TransactionId);//db.Transactions.Find(item.TransactionId)
                    if (( item.Status == ConstMessage.STATUS_PENDING_ID ||item.Status == ConstMessage.STATUS_PROCESSING_ID || item.Status == ConstMessage.STATUS_VENDOR_PROCESSING_ID ||item.Status == ConstMessage.STATUS_COMPLETE_ID || item.Status == ConstMessage.STATUS_FAILED_ID) && (transaction.Status == ConstMessage.STATUS_PENDING_ID || transaction.Status == ConstMessage.STATUS_PROCESSING_ID || transaction.Status == ConstMessage.STATUS_VENDOR_PROCESSING_ID))
                    {
                        transaction.Status = item.Status;
                        transaction.ReferenceId = item.ReferenceId;
                        transaction.UpdateDate = DateTime.Now;
                        if (item.Status == ConstMessage.STATUS_FAILED_ID)
                        {
                            transaction_repo.Return_User_Balance(transaction);
                            //admin_repo.UserBalanceUpdate(item.UserId, (decimal)item.AmountOut);
                        }
                        try
                        {
                            transaction_repo.Edit(transaction);
                        }
                        catch (Exception)
                        {
                            result = 2; //2 is status_Complete_with_error
                        }
                        if (result != 2) 
                            result = ConstMessage.STATUS_COMPLETE_ID;
                    }
                }
                vLogHeper.InsertVendorLog(model.list, 1, "POST", model.ReqId);
            }
            
            return result;
        }
        
    }
    #region Vendor Log Helpers
    /*
     * Created By - Siddique
     * Created On - 19th August, 2015
     * Created For - To encapsulate the processes regarding vendor log keeping and retreving
     */
    public class VendorLogHelper
    {
        //private eBankingDbContext db = new eBankingDbContext();
        IVendorRepository vendor_repo = new VendorRepository();

        public int InsertVendorLog( List<PendingMoneyTransfer> tList, int vendorId, string type, int? requestId)
        {
            if(tList.Count() > 0 )
            {
                VendorRequestLog vLog = new VendorRequestLog();

                vLog.VendorId = vendorId;
                vLog.Type = type;

                if (requestId == null)
                {
                    try
                    {
                        int? temp = vendor_repo.VendorReqLog_GetAll().Select(v => v.RequestId).OrderByDescending(r => r).FirstOrDefault();//db.VendorRequestLogs.Select(r => r.RequestId).OrderByDescending(r=>r).FirstOrDefault();
                        if (temp == null)
                            requestId = 1;
                        else
                            requestId = (int)temp + 1;
                    }
                    catch (Exception) { }
                }
                vLog.RequestId = (int)requestId;

                
                //db.VendorRequestLogs.Add(vLog);

                foreach (var item in tList) //this should belong to one db transaction
                {
                    var entry = new VendorRequestLogDetail();
                    entry.Status = (int)item.Status;
                    entry.TransactionId = item.TransactionId;
                    vLog.Details.Add(entry);
                }
                try
                {
                    vendor_repo.VendorRequestLog_Add(vLog);
                    return vLog.RequestId;
                }
                catch (Exception) { }
            }
            return 0;
        }
    }

    public class Helper
    {
        public List<PendingMoneyTransfer> convertTransactionList(List<Transaction> model)
        {
            int i = 0;
            List<PendingMoneyTransfer> result = new List<PendingMoneyTransfer>();
            foreach (var item in model)
            {
                if (item.IsTimeOut && i < ConstMessage.VendorMaxGet)
                {
                    PendingMoneyTransfer temp = new PendingMoneyTransfer();
                    temp.TransactionId = item.Id;
                    temp.InsertedAmount = item.InsertedAmount;
                    temp.ToUser = item.ToUser;
                    temp.UserId = item.UserId;
                    temp.ServiceName = item.ServiceName;
                    temp.AmountOut = item.AmountOut;
                    temp.Status = item.Status;
                    temp.ServiceName = item.ServiceName;
                    result.Add(temp);
                    i++;
                }
            }

            return result;
        }
    }
    #endregion
}
