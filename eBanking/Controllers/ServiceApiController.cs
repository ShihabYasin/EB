/*
 * ServiceApiController handles all api services for registered user, distributor user and vendors.
 * Provides medium of intraction from mobile app / user web application to main core of eBanking
 * 
 * Only authentication and login (Login, Logout, Register) is done in AccountController for better integrity
 */

using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.Interface;
using eBanking.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace eBanking.Controllers
{
 
   [Authorize]
    public class ServiceApiController : ApiController
    {

        #region Repository,Context And Variable Declaration

        private IRatePlanRepository rateplan_repo;
        private IServiceRepository service_repo;
        private IDestinationRepository destination_repo;
        private Variable var;
        private eBankingDbContext db;
        private TransactionController _TransactionController = new TransactionController();
        public ServiceApiController()
        {
            db = new eBankingDbContext();
            rateplan_repo = new RateplanRepository();
            service_repo = new ServiceRepository();
            destination_repo = new DestinationRepository();

            var = new Variable();
        
        }
        #endregion


        #region Api actions

        #region User Operations
        /*
         * accepts the recharge pin from registered user to recharge his/her eBanking balance
         */
        [ActionName("ReCharge")]
        [HttpPost]
        public HttpResponseMessage ReCharge(Pin Pinmodel)    //string PinCode
        {
            var.UserName = null;

            var.ApiStatus = ConstMessage.STATUS_MONEY_TRANSFER_FAILED_ID;
            var.Balance = ConstMessage.STATUS_SET_USER_BALANCE;  //-1
            //var.LocalBalance = ConstMessage.STATUS_SET_USER_BALANCE;
            var.ConvertFromUsd = 1;


            if (!string.IsNullOrEmpty(Pinmodel.PinCode) && Pinmodel.PinCode != null)
            {
                Pin model = new Pin();
                model.PinCode = Pinmodel.PinCode;

                var.UserName = User.Identity.Name;

                if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
                {
                    var.UserName = RequestContext.Principal.Identity.Name;
                }

                if (!string.IsNullOrEmpty(var.UserName) && var.UserName != null)
                {
                    var Result = new UserController().Voucher(model, ConstMessage.API_FROM, var.UserName);


                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    var temp = (serializer.Serialize(Result));

                    var obj = JObject.Parse(temp);
                    var.ApiStatus = (int)obj["Data"]["ApiStatus"];
                    var.Balance = (decimal)obj["Data"]["Balance"];
                    //var.LocalBalance = (decimal)obj["Data"]["LocalBalance"];
                    var.ConvertFromUsd = (decimal)obj["Data"]["ConvertFromUsd"];
                    var.ISO = (string)obj["Data"]["ISO"];
                    
                }
                else
                    var.ApiStatus = ConstMessage.STATUS_USER_NOT_FOUND_ID;

            }
            return Request.CreateResponse(HttpStatusCode.OK, new { var.ApiStatus, var.Balance, var.ConvertFromUsd, var.ISO });//var.LocalBalance
        }

        /*
         * gets the required details (currency, destination, rateplan etc) for money transfer
         */
        [HttpGet]
        [ActionName("MoneyTransfer")]
        public HttpResponseMessage MoneyTransfer()
        {
            

            var Country = destination_repo.GetAll().Where(x=>x.Id==ConstMessage.SELECTED_BAN_DESTINATION_ID).ToList();
            var Service = service_repo.GetAll(true).Where(x => x.ParentId == ConstMessage.STATUS_MONEY_TRANSFER_SUCCESS_ID).Select(x => x).ToList(); ;
            var RatePlan = rateplan_repo.GetAll();

            var.UserName = User.Identity.Name;
            //var.UserName = "01970000833";

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }

            UserRoleRepository role_repo = new UserRoleRepository();
            eBankingUser currentUser = role_repo.FindUserByUserName(var.UserName);
            IEnumerable<ServiceCommonViewModel> CommonServiceList = null;
            //if (!string.IsNullOrEmpty(currentUser.DistributorCode))
            //{
                CommonServiceList = service_repo.GetServicesForApi(currentUser.DistributorCode).Where(s => s.ServiceParentId == ConstMessage.SERVICES_MONEYTRANSFER_ID);//currentUser.DistributorCode  "1-2-3-7"
            //}
            return Request.CreateResponse(HttpStatusCode.OK, new { Country, Service, RatePlan, CommonServiceList });
        
        }

        /*
         * accepts money transfer request from registered user
         */

        [ActionName("MoneyTransfer")]
        [HttpPost]
        public HttpResponseMessage MoneyTransfer(Payment model)
        {
            var.ApiStatus = ConstMessage.STATUS_MONEY_TRANSFER_FAILED_ID;
            var.Balance = ConstMessage.STATUS_SET_USER_BALANCE;  //-1
            //var.LocalBalance = ConstMessage.STATUS_SET_USER_BALANCE;
            var.ConvertFromUsd = 1;
            string OperationNumber = "";

            if (model != null)
            {
                var.UserName = User.Identity.Name;      
                //var.UserName = "01970000833";

                if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
                {
                    var.UserName = RequestContext.Principal.Identity.Name;
                }

                if (!string.IsNullOrEmpty(var.UserName) && var.UserName != null)
                {

                    // var.ApiStatus = ConstMessage.MONEY_TRANSFER_SUCCESS_ID;
                    Payment moneyTransfer = new Payment();

                    moneyTransfer.FromCurrencyId = model.FromCurrencyId;
                    moneyTransfer.MobileNo = model.MobileNo;
                    moneyTransfer.ServiceId = model.ServiceId;
                    moneyTransfer.Amount = model.Amount;
                    moneyTransfer.AmountInUSD = model.AmountInUSD;
                    moneyTransfer.TotalAmount = model.TotalAmount;
                    moneyTransfer.ProcessingFee = model.ProcessingFee;

                    var transfer = new UserController().MoneyTransfer(moneyTransfer, var.UserName, ConstMessage.API_FROM, var.UserName);

                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    var.ApiStatus = (int)(JObject.Parse((serializer.Serialize(transfer))))["Data"]["ApiStatus"];
                    var.Balance = (decimal)(JObject.Parse((serializer.Serialize(transfer))))["Data"]["Balance"];
                    //var.LocalBalance = (decimal)(JObject.Parse((serializer.Serialize(transfer))))["Data"]["LocalBalance"];
                    var.ConvertFromUsd = (decimal)(JObject.Parse((serializer.Serialize(transfer))))["Data"]["ConvertFromUsd"];
                    OperationNumber = (string)(JObject.Parse((serializer.Serialize(transfer))))["Data"]["OperationNumber"];
                    var.ISO = (string)(JObject.Parse((serializer.Serialize(transfer))))["Data"]["ISO"];
                }
                else
                {
                    var.ApiStatus = ConstMessage.STATUS_USER_NOT_FOUND_ID;
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { var.ApiStatus, var.Balance, var.ConvertFromUsd, var.ISO, OperationNumber = OperationNumber });//var.LocalBalance
        }
        /*
         * accepts the credit transfer request from registered user
         */

        [ActionName("CreditTransfer")]
        [HttpPost]
        public HttpResponseMessage CreditTransfer(CreditTransfer model)
        {
            var.UserName = "";
            var.ApiStatus = ConstMessage.STATUS_CREDIT_TRANSFER_FAILED_ID;  //0
            var.Balance = ConstMessage.STATUS_SET_USER_BALANCE;  //-1
            //var.LocalBalance = ConstMessage.STATUS_SET_USER_BALANCE;
            var.ConvertFromUsd = 1;

            if (model!=null)
            {
                 var.UserName =User.Identity.Name;
               
                if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
                {
                    var.UserName = RequestContext.Principal.Identity.Name;
                }

                if (!string.IsNullOrEmpty(var.UserName) && var.UserName != null)
                {

                    var transfer = new UserController().CreditTransfer(model, ConstMessage.API_FROM, var.UserName);

                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    var.ApiStatus = (int)(JObject.Parse((serializer.Serialize(transfer))))["Data"]["ApiStatus"];
                    var.Balance = (decimal)(JObject.Parse((serializer.Serialize(transfer))))["Data"]["Balance"];
                    //var.LocalBalance = (decimal)(JObject.Parse((serializer.Serialize(transfer))))["Data"]["LocalBalance"];
                    var.ConvertFromUsd = (decimal)(JObject.Parse((serializer.Serialize(transfer))))["Data"]["ConvertFromUsd"];
                    var.ISO = (string)(JObject.Parse((serializer.Serialize(transfer))))["Data"]["ISO"];

                }
                else
                {
                    var.ApiStatus = ConstMessage.STATUS_USER_NOT_FOUND_ID; //500
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { var.ApiStatus, var.Balance, var.ConvertFromUsd, var.ISO });
        }
        /*
         * unknown developer and perpous of action 
         */
        public HttpResponseMessage UserIsAuthenticate()
        {
           bool IsAutheticate= RequestContext.Principal.Identity.IsAuthenticated;
           return Request.CreateResponse(HttpStatusCode.OK, new { IsAutheticate });  
        }

        /*
         * gets the user balance of registered user
         * 
         */

        [ActionName("GetUserInfo")]
        public HttpResponseMessage GetUserInfo()
        {
            AdminRepository admin_repo = new AdminRepository();
            CurrencyRepository currency_repo = new CurrencyRepository();
            var.ApiStatus = ConstMessage.STATUS_GET_USER_BALANCE_FAILED_ID; //0
            
            //db = new eBankingDbContext();

            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) && var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }
            if (!string.IsNullOrEmpty(var.UserName) && var.UserName != null)
            {
                try
                {
                    var SelectedUser = admin_repo.GetUserByUserName(var.UserName);//db.Users.Where(x => x.UserName == var.UserName).Select(x => x).SingleOrDefault();
                    //var LocalBalance = currency_repo.ConvertToLocal(SelectedUser.CurrentBalance, SelectedUser.LocalCurrencyId);
                    var ConvertFromUsd = currency_repo.GetConversionRate(SelectedUser.LocalCurrencyId);
                    DistributorController distController = new DistributorController();
                    decimal? DistributorBalance = distController.GetDestributorBalance(var.UserName);
                    if (SelectedUser != null)
                    {
                        var.ApiStatus = ConstMessage.STATUS_GET_USER_BALANCE_SUCCESS_ID; //1
                        UserRoleRepository user_role_repo = new UserRoleRepository();
                        string userRole = user_role_repo.GetRoleByUserName(var.UserName);
                        return Request.CreateResponse(HttpStatusCode.OK, new { var.ApiStatus, Balance = SelectedUser.CurrentBalance, ConvertFromUsd, ISO = admin_repo.GetUserCurrencyISOByUserName(var.UserName), DistributorBalance = (DistributorBalance ?? 0), Role = userRole });
                    }
                    else
                        var.ApiStatus = ConstMessage.STATUS_INTERNAL_ERROR_ID; //501
                }
                catch(Exception)
                {
                    var.ApiStatus = ConstMessage.EXCEPTION_STATUS_ID; //9999
                }
                
            }
            else
                var.ApiStatus = ConstMessage.STATUS_USER_NOT_FOUND_ID; //500

            return Request.CreateResponse(HttpStatusCode.OK, new {var.ApiStatus});
        }
        /*
         * API support to get the transfer history data on registered user request
         * 
         * Implemented By: Siddique
         */
        [ActionName("GetTransferHistoryApi")]
        [HttpPost]
        public HttpResponseMessage GetTransferHistoryApi(GetTransferHistoryVM model)
        {
            AdminRepository admin_repo = new AdminRepository();
            CurrencyRepository currency_repo = new CurrencyRepository();
            var.UserName = User.Identity.Name;


            if (string.IsNullOrEmpty(var.UserName) && var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }
            if(model == null)
            {
                model = new GetTransferHistoryVM();
            }
            var selectedUser = admin_repo.GetUserByUserName(var.UserName);
            if(selectedUser != null){
                //var.LocalBalance = currency_repo.ConvertToLocal(selectedUser.CurrentBalance, selectedUser.LocalCurrencyId);
                var.ConvertFromUsd = currency_repo.GetConversionRate(selectedUser.LocalCurrencyId);
                var.Balance = selectedUser.CurrentBalance;
                var.ISO = admin_repo.GetUserCurrencyISOByUserName(var.UserName);
            }
            var history = new TransactionController().GetTransferHistory(var.UserName, model.Month, model.Year);
            //return Request.CreateResponse(HttpStatusCode.OK, new { var.ApiStatus, var.Balance });
            return Request.CreateResponse(HttpStatusCode.OK, new { history, var.Balance, var.ConvertFromUsd, var.ISO });
        }

        //[AllowAnonymous]
        [ActionName("GetCurrencyList")]
        [HttpGet]
        public HttpResponseMessage GetCurrencyList()
        {
            CurrencyRepository currency_repo = new CurrencyRepository();
            var CurrencyList = currency_repo.ActiveCurrency(null);

            return Request.CreateResponse(HttpStatusCode.OK, new { CurrencyList });
        }
        /*
         * Cancel individual transaction from user within the time limit (TimeOut)
         * 
         * Implemented By: Siddique
         */

        [ActionName("CancelTransactionApi")]
        [HttpPost]
        public HttpResponseMessage CancelTransactionApi (CancelTransaction model)
        {
            var result = new TransactionController().CancelTransaction(model.Id, ConstMessage.TransactionFromApi);
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var temp = (serializer.Serialize(result));

            var obj = JObject.Parse(temp);
            var.ApiStatus = (int)obj["Data"]["ApiStatus"];
            var.Balance = (decimal)obj["Data"]["Balance"];
            //var.LocalBalance = (decimal)obj["Data"]["LocalBalance"];
            var.ConvertFromUsd = (decimal)obj["Data"]["ConvertFromUsd"];
            var.ISO = (string)obj["Data"]["ISO"];
            //return Request.CreateResponse(HttpStatusCode.OK, result );
            return Request.CreateResponse(HttpStatusCode.OK, new { var.ApiStatus, var.Balance, var.ConvertFromUsd, var.ISO });
        }

        /*
         * sends the top up and money transfer histor to registered user and provides the option to cancel a transaction within time limit (IsTimeout)
         * 
         * Only for Top Up and Money Transfer
         */
        [HttpGet]
        [ActionName("TransactionHistory")]
        public HttpResponseMessage TransactionHistory()
        {
            IEnumerable<MoneyTopUpHistory> MoneyTopUpHistory=null;

            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }

            if (var.UserName != null && !string.IsNullOrEmpty(var.UserName))
            {
            MoneyTopUpHistory = _TransactionController.TransactionHistoryCalledFromServiceAPI(var.UserName);
            }
            return Request.CreateResponse(HttpStatusCode.OK, new {MoneyTopUpHistory });
        }

        /* sends the current top up data(avalible destinatins, services, currency etc.) to registered user upon request
         * 
         * Created by Siddique
         * 22nd July, 2015
         */
        [HttpGet]
        [ActionName("TopUpApi")]
        public HttpResponseMessage TopUpApi()
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }
            var TopUpViewData = new UserController().TopUp("", ConstMessage.API_FROM, var.UserName);
            return Request.CreateResponse(HttpStatusCode.OK, new { TopUpViewData });
        }
        /*
         * accepts the top up request from regietered user
         */
        //[AllowAnonymous]
        [HttpPost]
        [ActionName("TopUpApi")]
        public HttpResponseMessage TopUpApi(TopUp TopUpmodel)
        {
            var.ApiStatus = ConstMessage.STATUS_MODELSTATE_ISVALID_FAILED; // 60
            string OperationNumber = "";
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }

            if (ModelState.IsValid && TopUpmodel !=null)
            {
                TopUp model = TopUpmodel;
                model.UserId = var.UserName;

                var TopUpResult = new UserController().TopUp(model, ConstMessage.API_FROM, null, var.UserName);

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                var temp = (serializer.Serialize(TopUpResult));

                var obj = JObject.Parse(temp);
                var.ApiStatus = (int)obj["Data"]["result"];
                
                if(var.ApiStatus == ConstMessage.STATUS_PENDING_ID)
                    OperationNumber = (string)obj["Data"]["OperationNumber"];
                var.Balance = (decimal)obj["Data"]["Balance"];
                //var.LocalBalance = (decimal)obj["Data"]["LocalBalance"];
                var.ConvertFromUsd = (decimal)obj["Data"]["ConvertFromUsd"];
                var.ISO = (string)obj["Data"]["ISO"];
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { var.ApiStatus, var.Balance, var.ConvertFromUsd, var.ISO, OperationNumber });
         
        }

        /* Get all necessary data for Registered user and distributor user to populate dashboard
         * 
         * Created By: Siddique
         * Created On: January 10th, 2016
         * 
         */
        [HttpGet]
        [ActionName("GetRegisteredUserDashBoard")]
        public RegisteredUserStatusViewModel GetRegisteredUserDashBoard()
        {
            var ctr = new TransactionController();
            var UserModel = ctr.GetUserDashboardData(User.Identity.Name);
            UserRoleRepository user_role_repo = new UserRoleRepository();
            string userRole = user_role_repo.GetRoleByUserName(User.Identity.Name);
            if (userRole == ConstMessage.ROLE_NAME_WHOLESELLER || userRole == ConstMessage.ROLE_NAME_RETAILER)
            {
                UserModel.IsDistributor = true;
                try
                {
                    DistributorController distributorController = new DistributorController();
                    var result = distributorController.AssignPins(ConstMessage.API_FROM, User.Identity.Name);
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    UserModel.DistributorSummary = new DistributorUserSummaryViewModel();
                    UserModel.DistributorSummary.PinList = serializer.Deserialize<List<PinAssignViewModel>>(JObject.Parse((serializer.Serialize(result)))["Data"]["result"].ToString());
                    UserModel.DistributorSummary.DistributorBalance = distributorController.GetDestributorBalance(User.Identity.Name);
                    var commissionValue = distributorController.GetDistributorLastCommission(User.Identity.Name);
                    UserModel.DistributorSummary.LastCommissionReturnAmount = commissionValue.Amount;
                    UserModel.DistributorSummary.LastCommissionReturnTime = commissionValue.Time;
                }
                catch (Exception) { }
            }
            return UserModel;
        }

        #endregion

        #region Vendor Operations

        /*
         * sends the current pending money transfer requests to active vendor upon request
         */
        [AllowAnonymous]
        [HttpGet]
        [ActionName("PendingMoneyTransferApi")]
        public HttpResponseMessage PendingMoneyTransferApi()
        {
            //int VendorId = ConstMessage.VENDOR_ID;
            var result = _TransactionController.PendingMoneyTransfer(ConstMessage.VENDOR_ID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /*
         * Confirms that the vendor received the transaction list and then
         */

        [AllowAnonymous]
        [HttpPost]
        [ActionName("VendorGetConfirmationApi")]
        public HttpResponseMessage VendorGetConfirmationApi(VendorReceivingConfirmation model)
        {
            var result = _TransactionController.VendorGetConfirmation(model);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /*
         * Gets the updated list of money transfer from vendor
         */
        //[AllowAnonymous]
        [HttpPost]
        [ActionName("UpdateMoneyTransferStatusApi")]
        public HttpResponseMessage UpdateMoneyTransferStatusApi( updatePendingMoneyTransfer model) //List<PendingMoneyTransfer> models, int reqId
        {
            var ApiStatus = _TransactionController.UpdateMoneyTransferStatus(model);
            return Request.CreateResponse(HttpStatusCode.OK, new { ApiStatus });
        }
        
        

        [AllowAnonymous]
        [HttpGet]
        [ActionName("SMSTransactionReportsApi")]
        public HttpResponseMessage SMSTransactionReportsApi(int? PageNumber, int? PageSize, string FromDate, string ToDate, string PinNumber, string UserName)
        {
            SMSTransactionReportsController smsTransactionReports = new SMSTransactionReportsController();
            
            var result = smsTransactionReports.Index(PageNumber, FromDate, ToDate, PinNumber, UserName, PageSize, ConstMessage.API_FROM);
            //                       Index(int? page, DateTime? FromDate, DateTime? ToDate, string PinNumber, string UserName, int? size, int? FromAPI)
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        //[AllowAnonymous]
        [ActionName("GetBalanceChartInfo")]
        [HttpGet]
        public HttpResponseMessage GetBalanceChartInfo(int? TimeLimit)
        {
            if(TimeLimit == null)
                TimeLimit = 120;
            
            var.ApiStatus = ConstMessage.STATUS_GET_USER_BALANCE_FAILED_ID; //0
            string Message = "";

            var.UserName = User.Identity.Name; //// "8801713336512" //

            if (string.IsNullOrEmpty(var.UserName) && var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }
            if (!string.IsNullOrEmpty(var.UserName) && var.UserName != null)
            {
                try
                {
                    AdminRepository admin_repo = new AdminRepository();
                    CurrencyRepository currency_repo = new CurrencyRepository();
                    TransactionRepository transaction_repo = new TransactionRepository();
                    var SelectedUser = admin_repo.GetUserByUserName(var.UserName);
                    var ConvertFromUsd = currency_repo.GetConversionRate(SelectedUser.LocalCurrencyId);
                    if (SelectedUser != null)
                    {
                        ServiceRepository service_repo = new ServiceRepository();
                        DateTime dateEnds = DateTime.Now.Date;
                        DateTime dateStart = dateEnds;//.AddDays(-120);
                        dateEnds = dateEnds.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                        if (TimeLimit != null)
                        {
                            dateStart = dateStart.AddDays(-(int)TimeLimit);
                        }
                        var allTransactions = transaction_repo.GetAllQueryable().Where(x => x.UserId == var.UserName && (x.TransactionDate >= dateStart && x.TransactionDate <= dateEnds) && (x.Status == ConstMessage.STATUS_PENDING_ID || x.Status == ConstMessage.STATUS_COMPLETE_ID)).ToList();
                        var services = service_repo.GetAll(true);
                        var WithServiceName = (from t in allTransactions
                                               from s in services
                                               join sp in services on s.ParentId equals sp.Id into gotParents
                                               from spr in gotParents.DefaultIfEmpty()
                                               where t.ServiceId == s.Id
                                               select new
                                               {
                                                   ServiceId = t.ServiceId,
                                                   Name = (spr == null ? "" : spr.Name),
                                                   //gs.ParentId,
                                                   AmountOut = t.AmountOut
                                               }).ToList();//
                        var graphData = (from t in WithServiceName
                                    group t.AmountOut by t.Name into namedGroup
                                    select new
                                    {
                                        ServiceName = namedGroup.Key,
                                        Amount = namedGroup.Sum()
                                    });
                        Message = "\"AmarTaka\" is a money transfer medium that provides you great opportunity to send money through any mobile via bKash in Bangladesh. Nearly we will arrange another service like Mobile top-up, M-cash, U-cash etc with that platform. Hopefully our service will benefited to you.";//"This message will be shown on the homepage of the phone app.";
                        var.ApiStatus = ConstMessage.STATUS_GET_USER_BALANCE_SUCCESS_ID; //1
                        return Request.CreateResponse(HttpStatusCode.OK, new { var.ApiStatus, Balance = SelectedUser.CurrentBalance, graphData, Message, ConvertFromUsd, ISO = admin_repo.GetUserCurrencyISOByUserName(var.UserName) });
                    }
                    else
                        var.ApiStatus = ConstMessage.STATUS_INTERNAL_ERROR_ID; //501
                }
                catch (Exception)
                {
                    var.ApiStatus = ConstMessage.EXCEPTION_STATUS_ID; //9999
                }

            }
            else
                var.ApiStatus = ConstMessage.STATUS_USER_NOT_FOUND_ID; //500

            return Request.CreateResponse(HttpStatusCode.OK, new { var.ApiStatus });
        }

        
        #endregion

        #region Distributor Operations
        [ActionName("DistributorTransaction")]
        [HttpGet]
        public HttpResponseMessage DistributorTransaction(int? ServiceId, int? DistributorId, DateTime? FromDate, DateTime? ToDate, string CreatedBy, int? page, int? itemsPerPage)
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }
            DistributorController distributorController = new DistributorController();
            var result = distributorController.DistributorTransaction(ServiceId, DistributorId, FromDate, ToDate, CreatedBy, page, itemsPerPage, ConstMessage.API_FROM, var.UserName);

            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }

        [ActionName("DistributorTransactionDetailsApi")]
        [HttpGet]
        public HttpResponseMessage DistributorTransactionDetailsApi(int? DistributorTransactionId)
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }

            DistributorController distributorController = new DistributorController();
            var result = distributorController.DistributorTransactionDetails(DistributorTransactionId, ConstMessage.API_FROM, var.UserName);


            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }

        [ActionName("GetDistributors")]
        [HttpGet]
        public HttpResponseMessage GetDistributors(int? page, int? itemsPerPage)
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }
            DistributorController distributorController = new DistributorController();
            var result = distributorController.Distributors(page, itemsPerPage, ConstMessage.API_FROM, var.UserName);

            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }
        [ActionName("CreateDistributorApi")]
        [HttpGet]
        public HttpResponseMessage CreateDistributorApi(string DistributorUserName, string RoleName, bool? Active)
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }

            DistributorController distributorController = new DistributorController();
            //if(!string.IsNullOrEmpty(DistributorUserName) && !string.IsNullOrEmpty(RoleName))
            var result = distributorController.CreateDistributor(DistributorUserName, RoleName, Active, var.UserName, ConstMessage.API_FROM);
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }


        [HttpGet]
        [ActionName("eBankingCashInAPI")]
        public HttpResponseMessage eBankingCashInAPI()
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }

            DistributorController distributorController = new DistributorController();
            var result = distributorController.eBankingCashIn(ConstMessage.API_FROM, var.UserName);
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }

        [HttpPost]
        [ActionName("eBankingCashInAPI")]
        public HttpResponseMessage eBankingCashInAPI(UserCashInViewModel model)
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }

            DistributorController distributorController = new DistributorController();
            var result = distributorController.eBankingCashIn(model,"Cash In",  ConstMessage.API_FROM, var.UserName);
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }

        [HttpGet]
        [ActionName("DistributorComissionRateplan")]
        public HttpResponseMessage DistributorComissionRateplan(int? page, int? itemsPerPage, string requestedPage)
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }

            DistributorController distributorController = new DistributorController();
            var result = distributorController.DistributorComissionRateplanIndex(page, itemsPerPage, requestedPage, ConstMessage.API_FROM, var.UserName, null, null);
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }

        [HttpGet]
        [ActionName("DistributorComissionRateplanCreate")]
        public HttpResponseMessage DistributorComissionRateplanCreate()
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }

            DistributorController distributorController = new DistributorController();
            var result = distributorController.DistributorComissionRateplanCreate(ConstMessage.API_FROM, var.UserName);
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }

        [HttpPost]
        [ActionName("DistributorComissionRateplanCreate")]
        public HttpResponseMessage DistributorComissionRateplanCreate(DistributorCommissionRateplanViewModel model)
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }
            DistributorCommissionRateplan newModel = new DistributorCommissionRateplan();
            newModel.Commission = model.Commission;
            newModel.Discount = model.Discount;
            newModel.DistributorId = model.DistributorId;
            newModel.IsActive = model.IsActive;
            newModel.IsPercentage = model.IsPercentage;
            newModel.ServiceId = model.ServiceId;
            newModel.ServiceCharge = model.ServiceCharge;
            newModel.Remarks = model.Remarks;
            newModel.RateName = model.RateName;

            DistributorController distributorController = new DistributorController();
            var result = distributorController.DistributorComissionRateplanCreate(newModel, ConstMessage.API_FROM, var.UserName);
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }

        [HttpGet]
        [ActionName("AssignedRatePlansApi")]
        public HttpResponseMessage AssignedRatePlansApi(int DistributorId)
        {
            DistributorController distributorController = new DistributorController();
            var result = distributorController.AssignedRatePlans(DistributorId, null, ConstMessage.API_FROM);
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }
        [HttpGet]
        [ActionName("AssignRatePlanToDistributorApi")]
        public HttpResponseMessage AssignRatePlanToDistributorApi(int DistributorId)
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }
            DistributorController distributorController = new DistributorController();
            var result = distributorController.AssignRatePlanToDistributor(DistributorId, ConstMessage.API_FROM, var.UserName);
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }
        [HttpPost]
        [ActionName("AssignRatePlanToDistributorApi")]
        public HttpResponseMessage AssignRatePlanToDistributorApi(AssignedDCRPViewModel model)
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }
            DistributorController distributorController = new DistributorController();
            AssignDistributorRateplan Model = new AssignDistributorRateplan();
            Model.DistributorId = model.DistributorId;
            Model.IsActive = model.IsActive;
            Model.RateplanId = model.RatePlanId;
            var result = distributorController.AssignRatePlanToDistributor(Model, ConstMessage.API_FROM, var.UserName);
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }

        [HttpGet]
        [ActionName("DistributorPins")]
        public HttpResponseMessage DistributorPins(string Prefix, long? SerialNo, long? SerialNoTo, string BatchNo, string PinCode, int? Status, DateTime? FromDate, DateTime? ToDate, int? page, string Submit, string DistributorCode, bool? PinsInHand)
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }
            DistributorController distributorController = new DistributorController();
            var result = distributorController.DistributorPins(Prefix, SerialNo, SerialNoTo, BatchNo, PinCode, Status, FromDate, ToDate, page, Submit, DistributorCode, PinsInHand, ConstMessage.API_FROM, var.UserName);
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }

        [HttpGet]
        [ActionName("AssignPins")]
        public HttpResponseMessage AssignPins()
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }
            DistributorController distributorController = new DistributorController();
            var result = distributorController.AssignPins(ConstMessage.API_FROM, var.UserName);
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }
        [HttpPost]
        [ActionName("AssignPinsPOST")]
        public HttpResponseMessage AssignPinsPOST(IEnumerable<PinAssignViewModel> Models) 
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }
            DistributorController distributorController = new DistributorController();
            var result = distributorController.AssignPins(Models, ConstMessage.API_FROM, var.UserName);
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }

        #endregion

        #endregion

        [HttpGet]
        [ActionName("RetailerRegisteredUserIndex")]
        public HttpResponseMessage RetailerRegisteredUserIndex()
        {
            var.UserName = User.Identity.Name;

            if (string.IsNullOrEmpty(var.UserName) || var.UserName == null)
            {
                var.UserName = RequestContext.Principal.Identity.Name;
            }

            DistributorController distributorController = new DistributorController();
            var result = distributorController.RetailerRegisteredUserIndex(ConstMessage.API_FROM, var.UserName);
            return Request.CreateResponse(HttpStatusCode.OK, new { Data = result });
        }
    }
}