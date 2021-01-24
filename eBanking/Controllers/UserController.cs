using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.App_Start;
using eBanking.Interface;
using eBanking.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eBanking.Controllers
{
    [CustomAuth]
    public class UserController : Controller
    {
        #region Repository,Context And Variable Declaration

        private Variable _variable;
        //private eBankingDbContext db;
        Transaction newTransaction;
        private IdentityConfig ObjIdentity;
        //private Currency currency;
        private RatePlan ratePlan;
        private UserMenuGenarator user_menu = new UserMenuGenarator();
        private ITransactionRepository transaction_repo;
        private IServiceRepository service_repo;
        private IRatePlanRepository ratePlan_repo;
        private IDestinationRepository destination_repo;
        private IPinRepository pin_repo;
        private ICurrencyRepository currency_repo;
        private IAdminRepository admin_repo;
        private IDistributorRepository distributor_repo;
        private SMSDR_Helper smsHelper = new SMSDR_Helper();
        private UserManager<eBankingUser> UserManager{get; set;}
        private IVendorRepository vendor_repo;
            
        public UserController()
        {
            _variable = new Variable();
            //db = new eBankingDbContext();
            newTransaction = new Transaction();
            ObjIdentity = new IdentityConfig();
            transaction_repo = new TransactionRepository();
            service_repo = new ServiceRepository();
            ratePlan_repo = new RateplanRepository();
            destination_repo = new DestinationRepository();
            pin_repo = new PinRepository();
            currency_repo = new CurrencyRepository();
            admin_repo = new AdminRepository();
            UserManager = new UserManager<eBankingUser>(new UserStore<eBankingUser>(new eBankingDbContext()));
            vendor_repo = new VendorRepository();
            distributor_repo = new DistributorRepository();

        }

        #endregion

        //Recharge
        //user can create a voucher by using a pinCode
        //one pin is used one time as like phone recharge card
        //after success an Status Used is replace beside this PinCode

        public ActionResult Voucher()
        {            
            return View();
        }

        /*---------------------------------------------------------
        *  User can Recharge money to own account via Voucher HttpPost method
        *  Initially status(ApiStatus) and User Balance assigned to 0 and 1
        *  check rechered Pin Code 
        *  get your User Name check it
        *  check Pincode validity is it in live and Active and NotUsed status
        *  If Pin Status is NotUsed then recharge
        *  while rechathe genarate a operation number for transaction and add others necessary data to transaction
        *  Change status of Pin table NotUsed to Used and Edit Pin
        *  update user CurrentBalance as he rechareed in line  user.CurrentBalance += pin.Value;
        *  then save all data and return view 
        *  if request comes from mobile then return Json with status and user balance 
        *---------------------------------------------------------*/
        //post data for Recharge

        //HACK - 1st october, 2015, Siddique, turned of all the "ModelState.AddModelError" because of simultanious call to recharge and topUp

        [HttpPost]
        public ActionResult Voucher(Pin voucher, int? fromApi,string UserNo)
        {
            //inatially all status is failed and balance is -1

            _variable.ApiStatus = ConstMessage.PIN_STATUS_ERROR_ID; //0
            _variable.Balance = ConstMessage.STATUS_SET_USER_BALANCE; //-1
            //_variable.LocalBalance = ConstMessage.STATUS_SET_USER_BALANCE;
            _variable.ConvertFromUsd = 1;
            Pin pin = new Pin();

            if (voucher.PinCode == null && string.IsNullOrEmpty(voucher.PinCode))
            {
                _variable.ApiStatus = ConstMessage.PIN_STATUS_INVALID_PIN_ID;

                //ModelState.AddModelError("", "You should Insert a Pin");
            }
            else
            {
                //from Api request
                //get user name

                if (!string.IsNullOrEmpty(UserNo)) 
                    _variable.UserName = UserNo;
                else         

                _variable.UserName = HttpContext.User.Identity.Name;


                if (_variable.UserName == null || string.IsNullOrEmpty(_variable.UserName))
                {
                     _variable.ApiStatus = ConstMessage.STATUS_USER_NOT_FOUND_ID;
                     //ModelState.AddModelError("",ConstMessage.STATUS_USER_NOT_FOUND_MSG);
                     return View();
                }

                //get ammount from Pin by using pin code if pin Status==NotUsed and IsActive true
                pin = pin_repo.GetActivePinByPinCode(voucher.PinCode);

                //if now pin exists
                if (pin != null)
                {
                    //check is the pin already used or not

                    if (pin.Status == ConstMessage.PIN_UN_USED_ID && pin.ServiceId == ConstMessage.Service_Voucher_Recharge)
                    {
                        UserRoleRepository user_role_repo = new UserRoleRepository();
                        int distributorId = distributor_repo.GetDistributorIdFromDistributorCode(pin.DistributorCode);
                        string distributorRole = user_role_repo.GetRoleByUserName(distributor_repo.Distributor_FindById(distributorId).UserName);
                        if (distributorRole == ConstMessage.ROLE_NAME_RETAILER)
                        {
                            newTransaction = new Transaction();
                            newTransaction.OperationNumber = transaction_repo.OperationNumberGenarator(ConstMessage.PREFIX_RECHARGE, DateTime.Now.ToString("ddMMyyyy"));

                            newTransaction.UserId = _variable.UserName;
                            newTransaction.PinId = pin.Id;
                            newTransaction.Status = ConstMessage.PIN_USED_STATUS_ID;
                            newTransaction.ServiceId = ConstMessage.STATUS_SERVICE_VOUCHER_RECHARGED_ID;
                            newTransaction.FromCurrencyId = ConstMessage.SELECTED_USD_DESTINATION_ID;
                            newTransaction.AmountIN = pin.Value;
                            newTransaction.TransactionDate = DateTime.Now;
                            newTransaction.VendorId = 0;
                            newTransaction.ToUser = "";
                            newTransaction.DistributorCommission = 0;
                            try
                            {
                                pin.Status = ConstMessage.PIN_USED_STATUS_ID;
                                pin.ExecutionDate = DateTime.Now;
                                pin.UsedBy = _variable.UserName;
                                //transaction_repo.Add(newTransaction);

                                //eBankingUser user = UserManager.FindByName(_variable.UserName);
                                //user.CurrentBalance += pin.Value;
               /*
                * todo: transfer admin_repo.VoucherDbEntry to transaction repository
                */
                                // saves db entry for voucher and adds balance to user
                                if (admin_repo.VoucherDbEntry(newTransaction, pin, _variable.UserName, pin.Value)) 
                                    ViewBag.Message = ConstMessage.STATUS_RECHARGE_SUCCESS_MSG;

                                eBankingUser user = UserManager.FindByName(_variable.UserName);
                                _variable.Balance = user.CurrentBalance;
                                if (string.IsNullOrEmpty(user.DistributorCode))
                                {
                                    user.DistributorCode = pin.DistributorCode;
                                    UserManager.Update(user);
                                }
                                //_variable.LocalBalance = currency_repo.ConvertToLocal(user.CurrentBalance, user.LocalCurrencyId);
                                _variable.ConvertFromUsd = currency_repo.GetConversionRate(user.LocalCurrencyId);
                                _variable.ISO = admin_repo.GetUserCurrencyISOByUserName(_variable.UserName);
                                //_variable.Balance = user.CurrentBalance;
                                _variable.ApiStatus = ConstMessage.STATUS_COMPLETE_ID;
                                ViewBag.OperationNumber = newTransaction.OperationNumber;
                                ViewBag.PinValue = pin.Value;
                           
                            }
                            catch (Exception ex)
                            {
                                string a = ex.Message;
                                _variable.ApiStatus = ConstMessage.STATUS_FAILED_ID;
                           
                            }
                        }
                        else
                        {
                            _variable.ApiStatus = ConstMessage.PIN_STATUS_INVALID_PIN_ID;
                            ViewBag.Message = "Pin is not assigned.";
                        }

                    }
                    else
                    {
                        //ModelState.AddModelError("", ConstMessage.PIN_USED_STATUS_ERROR_MSG);
                        _variable.ApiStatus = ConstMessage.PIN_USED_STATUS_ID;
                        ViewBag.PinValue = pin.Value;
                    }
                }
                else
                {
                    //Invalid pinCode
                    //ModelState.AddModelError("", ConstMessage.PIN_STATUS_INVALID_PIN_ERROR_MSG);
                    _variable.ApiStatus = ConstMessage.PIN_STATUS_INVALID_PIN_ID;
                    ViewBag.PinValue = 0;
                }

            }

            if (fromApi == ConstMessage.API_FROM)
            {
                return Json(new { _variable.ApiStatus, _variable.Balance, _variable.ConvertFromUsd, _variable.ISO, message = ViewBag.Message, OperationNumber = ViewBag.OperationNumber, PinValue = ViewBag.PinValue }, JsonRequestBehavior.AllowGet); //_variable.LocalBalance
            }

            //ViewBag.Menu = user_menu.GenarateMenu(HttpContext.User.Identity.Name);
            return View();
        }

        /*
         *Modified by Siddique
         *22nd July, 2015
         *Modified to support TopUp for api
         */

        /*---------------------------------------------------------
        *   this is TopUp HttpGet view page both for browser and mobile 
        *   FromApi has a value when request comes from  mobile or device
        *   Type display Prepaid and PostPaid option value 1 and 2
        *   ViewData["Destination"],ViewData["Service"] ---all for dropdown list
        *   if request comes from device then return Json with necessary service   
        *---------------------------------------------------------*/


        [HttpGet]
        public ActionResult TopUp(string TopUpResult, int? FromApi, string UserName)
        {
            IEnumerable<ServiceCommonViewModel> CommonServiceList = null;
            List<Service> Type = new List<Service>
            {
             new Service{ Id=ConstMessage.TOP_UP_PRE_PAID_VALUE, Name=ConstMessage.TOP_UP_PRE_PAID_MSG},     
             new Service{ Id=ConstMessage.TOP_UP_POST_PAID_VALUE, Name=ConstMessage.TOP_UP_POST_PAID_MSG}, 
            };
         

            try
            {
                UserRoleRepository role_repo = new UserRoleRepository();
                eBankingUser currentUser;
                if(FromApi != null && FromApi == ConstMessage.API_FROM && !string.IsNullOrEmpty(UserName))
                {
                    currentUser = role_repo.FindUserByUserName(UserName);
                }
                else
                {
                    UserName = User.Identity.Name;
                    currentUser = role_repo.FindUserByUserName(UserName);
                }
                //if (!string.IsNullOrEmpty(currentUser.DistributorCode))
                //{
                CommonServiceList = service_repo.GetServicesForApi(currentUser.DistributorCode).Where(s=>s.ServiceParentId == ConstMessage.SERVICES_TOPUP_ID);//currentUser.DistributorCode  "1-2-3-7"
                //}
                var Service = service_repo.GetAllQueryable().Where(x => x.IsActive == true && x.ParentId == ConstMessage.SERVICES_TOPUP_ID && x.value > 0).OrderBy(x=>x.value);
                var RatePlan = ratePlan_repo.GetAll();
               // var Type = service_repo.GetAll().Where(x => x.Id == 6 || x.Id == 7);

                var Destination = eBankingTask.GetDestinationForTopUp(destination_repo.GetAll(),service_repo.GetAll(true));//destination_repo.GetDestinationForTopUp();

                ViewData["Destination"] = new SelectList(Destination, "Id", "DestinationName");
                ViewData["Service"] = new SelectList(Service, "Id", "Name");
                
                ViewData["Type"] = new SelectList(Type, "Id", "Name");
                
                ViewData["Data"] = new { Service, RatePlan };
                ViewBag.TopUpResult = TopUpResult;

                if (FromApi == ConstMessage.API_FROM)
                {
                    return Json(new { Service, RatePlan, Type, Destination, CommonServiceList = CommonServiceList }, JsonRequestBehavior.AllowGet);
                }
                return View();
            }
            catch (Exception)
            {
                
            }
            return View("Error");
        }


        /*---------------------------------------------------------
         *   This is TopUp HttpPost method ,FromAPI has value when request comes from mobile or device
         *   Initially TopUp status is assigned Failed and Balance=0
         *   check UserId(UserName) validity from user table then do TopUp transaction 
         *   with Status Pending and TopUp Operation Number
         *   also add relative data into SMSDR table then send Top Up request to the TopUp vendor
         *   get vendor resmonse save it and filter it to get Status
         *   if TopUp Success then decrease User money in line  user.CurrentBalance -= TopUpmodel.TotalAmount;
         *   and save also in smsdr table with status Complete if vendor Error or exception then changed status Failed
         *---------------------------------------------------------*/

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult TopUp(TopUp TopUpmodel, int? ClientId, bool? FromSms, string UserName)
        {          


            int result = ConstMessage.STATUS_FAILED_ID;
            
            decimal Balance = 0;
            //decimal LocalBalance = 0;
            decimal ConvertFromUsd = 0;
            string ISO = "";
            bool topUpEntry = false;
            string ResponseMessage = "";
            SMSDR errorLog;
            RequestResponse requestResponse;
            string ErrorMessage = "";

            _variable.Flag = ConstMessage.STATUS_PENDING_ID;

            if (ModelState.IsValid)
            {
                    
                if (TopUpmodel.UserId == null && string.IsNullOrEmpty(TopUpmodel.UserId))
                    TopUpmodel.UserId = HttpContext.User.Identity.Name;

                if (TopUpmodel.UserId != null && !string.IsNullOrEmpty(TopUpmodel.UserId))
                {
                    //get value from TopUpmodel.ValueId 

                    var user = UserManager.FindByName(TopUpmodel.UserId);

                    Service selectedTopup = new Service();
                    RatePlan selectedRateplan = new RatePlan();

                    try
                    {
                        selectedTopup = service_repo.FindById(TopUpmodel.ServiceId);
                        selectedRateplan = ratePlan_repo.FindByService(selectedTopup.Id);
                    }
                    catch (Exception)
                    {

                    }

                    #region ReCalculate topup

                    if (FromSms == null || FromSms == false)
                    {
                        var verifiedUser = ValidateToUser( TopUpmodel.ToUser, TopUpmodel.ServiceId);
                        TopUpmodel.OperatorPrefix = verifiedUser.OperatorPrefix;
                        TopUpmodel.ToUser = verifiedUser.PhoneNumber;
                    }

                    TopUpmodel.AmountInUSD = selectedRateplan.MRP;
                    TopUpmodel.FromCurrencyId = selectedTopup.DestinationId;
                    if (selectedRateplan.ServiceChargeIsPercentage)
                    {
                        TopUpmodel.ProcessingFee = selectedRateplan.MRP * (selectedRateplan.ServiceCharge / 100);
                    }
                    TopUpmodel.ProcessingFee = selectedRateplan.ServiceCharge;
                    TopUpmodel.TotalAmount = TopUpmodel.AmountInUSD + TopUpmodel.ProcessingFee;
                    TopUpmodel.Value = selectedTopup.value;
                    //TopUpmodel.StatusId = 
                    //TopUpmodel.ValueId = 

                    IEnumerable<DistributorServiceChargeVM> distributorCharges = new List<DistributorServiceChargeVM>();
                    List<DistributorTransaction> dTranList = new List<DistributorTransaction>();

                    UserRoleRepository role_repo = new UserRoleRepository();
                    eBankingUser currentUser;

                    //dont calculate distributor commission if service is by pin
                    if (TopUpmodel.PinId == null)
                    {
                        if (ClientId != null && ClientId == ConstMessage.API_FROM && !string.IsNullOrEmpty(UserName))
                        {
                            currentUser = role_repo.FindUserByUserName(UserName);
                        }
                        else
                        {
                            UserName = User.Identity.Name;
                            currentUser = role_repo.FindUserByUserName(UserName);
                        }
                        if (!string.IsNullOrEmpty(currentUser.DistributorCode))
                        {
                            distributorCharges = distributor_repo.DCRP_GetDSCforEachDistributor(TopUpmodel.ServiceId, currentUser.DistributorCode);//currentUser.DistributorCode  "1-2-3-7"
                            foreach (var distributor in distributorCharges)
                            {
                                DistributorTransaction dTran = new DistributorTransaction();
                                dTran.AmountIn = distributor.ServiceCharge;
                                dTran.ConvertToUsd = 1;
                                dTran.CreatedBy = UserName;
                                dTran.CreatedOn = DateTime.Now;
                                dTran.CurrencyId = ConstMessage.SELECTED_USD_DESTINATION_ID;
                                dTran.DistributorId = distributor.DistributorId;

                                dTran.Remarks = "CommissionOfService:"+TopUpmodel.ServiceId+", Username:"+TopUpmodel.UserId;

                                TopUpmodel.TotalAmount += (dTran.AmountIn ?? 0);

                                dTranList.Add(dTran);

                            }
                        }
                    }

                    #endregion

                    //TopUpmodel.AmountInUSD = TopUpmodel.Value;

                    if (TopUpmodel.Value != null && user != null && (user.CurrentBalance >= TopUpmodel.TotalAmount || (FromSms ?? false)))
                    {
                        //admin_repo.UserBalanceUpdate(user.UserName, -(decimal)TopUpmodel.TotalAmount);

                        if ((!FromSms ?? true))
                            Balance = user.CurrentBalance - (decimal)(TopUpmodel.TotalAmount);

                        //todo - calculate all submitted data based on service and value... top up
                        #region assignment to transaction and smsdr model
                        newTransaction = new Transaction();
                        newTransaction.UserId = TopUpmodel.UserId;
                        newTransaction.ToUser = TopUpmodel.ToUser;
                        newTransaction.TransactionDate = DateTime.Now;
                        newTransaction.ServiceId = TopUpmodel.ServiceId;
                        newTransaction.RatePlanId = TopUpmodel.RatePlanId;
                        newTransaction.FromCurrencyId = TopUpmodel.FromCurrencyId;
                        newTransaction.InsertedAmount = TopUpmodel.Value;
                        newTransaction.AmountOut = TopUpmodel.TotalAmount;
                        newTransaction.Status = ConstMessage.STATUS_PENDING_ID;
                        newTransaction.OperationNumber = transaction_repo.OperationNumberGenarator(ConstMessage.PREFIX_TOP_UP, DateTime.Now.ToString("ddMMyyyy"));
                        newTransaction.PinId = TopUpmodel.PinId;
                        if(TopUpmodel.PinId != null && TopUpmodel.PinId > 0)
                            newTransaction.DistributorCommission = 0;
                        else
                            newTransaction.DistributorCommission = ConstMessage.DistributorCommissionNotReturned;
                        if ((FromSms ?? false))
                        {
                            newTransaction.AmountIN = TopUpmodel.TotalAmount;

                        }

                        //now add data into SMSDR
                        SMSDR smsdr = new SMSDR();
                        smsdr.PHN_NO = TopUpmodel.ToUser;
                        smsdr.PHN_NO_TYPE = "";
                        smsdr.TRANS_AMOUNT = TopUpmodel.Value;
                        smsdr.COST_AMOUNT = TopUpmodel.TotalAmount;
                        smsdr.RATE_PLANE_ID = TopUpmodel.RatePlanId;
                        smsdr.REQUEST_ID = "";
                        smsdr.REQUEST_TIME = DateTime.Now;
                        smsdr.REFILL_SUCCESS_TIME = null;
                        smsdr.STATUS = ConstMessage.STATUS_PENDING_ID; //1
                        smsdr.RESPONSE_MSG = "";
                        smsdr.GW_ID = "";
                        smsdr.CREATED_BY = TopUpmodel.UserId;
                        //admin_repo.SmsdrAdd(smsdr);
                        #endregion

                        IVendorRepository vendor_repo = new VendorRepository();
                        IEnumerable<VendorRoute> availableVendors = vendor_repo.VendorRoute_GetAllByService(TopUpmodel.ServiceId);
                        //int selectedVendorId = //ConstMessage.abrar_VendorId;
                        foreach (var route in availableVendors)
                        {
                            //try
                            //{
                                if (vendor_repo.FindVendorById(route.VendorId).CurrentBalance < newTransaction.InsertedAmount)
                                    continue;

                                #region abrar
                                if (route.VendorId == ConstMessage.abrar_VendorId)
                                {
                                    
                                    smsdr.VendorId = ConstMessage.abrar_VendorId;
                                    //old function
                                    //string ResponseMessage = smsHelper.SendHttpRequest(ConstMessage.SMSGatewayRequestPage, ConstMessage.SMSGatewayReq, ConstMessage.SMSGatewayCountryCode, TopUpmodel.ToUser, (int)TopUpmodel.Value, TopUpmodel.Type);//"01713336506" TopUpmodel.UserId 
                                    //new function
                                    ResponseMessage = smsHelper.SendVendorHttpRequest(route.VendorId, ConstMessage.Request_Type_Request, newTransaction.ServiceId, TopUpmodel.ToUser, (int)TopUpmodel.Value, null, TopUpmodel.Type);
                                    //ResponseMessage = "\n\n\nRequestID=1447737588064:01820000833";

                                    ViewBag.TopUpResult = ConstMessage.TOP_UP_SUCCESS_MSG;

                                    //spilt the response for Request_Id
                                    RequestResponse Response;
                                    string TagName = "";
                                    string TagCode = "";
                                    try
                                    {
                                        Response = smsHelper.SpiltResponse(ResponseMessage,ConstMessage.abrar_VendorId,ConstMessage.Request_Type_Request);
                                        TagName = Response.RESPONSE_MSG;
                                        TagCode = Response.TransactionId;
                                        smsdr.REQUEST_ID = TagCode;
                                    }
                                    catch (Exception ex)
                                    {
                                        string a = ex.Message;
                                        ViewBag.TopUpResult = ConstMessage.TOP_UP_FAILED_MSG;
                                        result = ConstMessage.STATUS_FAILED_ID;
                                    }

                                    if (TagName.Contains(ConstMessage.SEND_AIRTIME_REQUEST_SUCCESSFUL_RESPONSE_MSG))
                                    {
                                        newTransaction.VendorId = ConstMessage.abrar_VendorId;
                                        newTransaction.ReferenceId = smsdr.REQUEST_ID;
                                        if (ClientId != null)
                                            newTransaction.ClientId = (int)ClientId;
                                        else
                                            newTransaction.ClientId = 0;
                                        //returns true if all db entry is successful
                                        topUpEntry = admin_repo.TopUpDbEntry(newTransaction, smsdr, FromSms);
                                        ReturnDistributorTransactionCharges(dTranList,newTransaction.OperationNumber);


                                        if (topUpEntry)
                                            result = ConstMessage.STATUS_PENDING_ID;

                                        //Vendor Transaction
                                        vendor_repo.VendorTransaction_Add(newTransaction, ConstMessage.Balance_Deduct);
                                        vendor_repo.VendorRoute_UpdatePendingQty(route.Id,true);

                                        break;
                                    }
                                    else if (TagName == ConstMessage.SEND_AIRTIME_REQUEST_ERROR_RESPONSE_MSG)
                                    {
                                        errorLog = new SMSDR();
                                        errorLog = smsdr;
                                        errorLog.STATUS = ConstMessage.STATUS_FAILED_ID;
                                        errorLog.RESPONSE_MSG = ResponseMessage;
                                        admin_repo.SmsdrAdd(errorLog);
                                        continue;
                                    }
                                }
                                #endregion

                                #region ezze
                                else if (route.VendorId == ConstMessage.ezze_VendorId || route.VendorId == ConstMessage.ezze_topup_VendorId)
                                {
                                    newTransaction.ReferenceId = transaction_repo.VendorUniqueIdGenerator(ConstMessage.ezze_vendor_prefix);//smsHelper.GenerateReferenceId(newTransaction.OperationNumber, newTransaction.ToUser);
                                    smsdr.REQUEST_ID = newTransaction.ReferenceId;
                                    smsdr.VendorId = route.VendorId;
                                    if (newTransaction.ToUser.Length == 13)
                                        newTransaction.ToUser = newTransaction.ToUser.Substring(2, 11);
                                    ResponseMessage = smsHelper.SendVendorHttpRequest(route.VendorId, ConstMessage.Request_Type_Request, newTransaction.ServiceId, TopUpmodel.ToUser, (int)TopUpmodel.Value, newTransaction.ReferenceId, TopUpmodel.Type); //"SUCCESS: Request processed successfully.";//
                                    
                                    newTransaction.VendorId = route.VendorId;

                                    if (ResponseMessage.Contains("SUCCESS"))
                                    {
                                        ViewBag.TopUpResult = ConstMessage.TOP_UP_SUCCESS_MSG;

                                        newTransaction.VendorId = route.VendorId;

                                        if (ClientId != null)
                                            newTransaction.ClientId = (int)ClientId;
                                        else
                                            newTransaction.ClientId = 0;
                                        //returns true if all db entry is successful
                                        topUpEntry = admin_repo.TopUpDbEntry(newTransaction, smsdr, FromSms);
                                        ReturnDistributorTransactionCharges(dTranList, newTransaction.OperationNumber);


                                        if (topUpEntry)
                                            result = ConstMessage.STATUS_PENDING_ID;

                                        //Vendor Transaction
                                        vendor_repo.VendorTransaction_Add(newTransaction, ConstMessage.Balance_Deduct);
                                        vendor_repo.VendorRoute_UpdatePendingQty(route.Id, true);

                                        break;
                                    }
                                    else
                                    {
                                        errorLog = new SMSDR();
                                        errorLog = smsdr;
                                        errorLog.STATUS = ConstMessage.STATUS_FAILED_ID;
                                        errorLog.RESPONSE_MSG = ResponseMessage;
                                        admin_repo.SmsdrAdd(errorLog);
                                    }
                                    continue;
                                }

                                #endregion

                                #region swift
                                else if (route.VendorId == ConstMessage.Swift_VendorId)
                                {
                                    SoapManager soapManager = new SoapManager();
                                    newTransaction.ReferenceId = transaction_repo.VendorUniqueIdGenerator(ConstMessage.Swift_vendor_prefix);
                                    smsdr.REQUEST_ID = newTransaction.ReferenceId;
                                    smsdr.VendorId = route.VendorId;
                                    if (newTransaction.ToUser.Length == 13)
                                        newTransaction.ToUser = newTransaction.ToUser.Substring(2, 11);


                                    requestResponse = soapManager.SwiftTopUP(newTransaction.ReferenceId, TopUpmodel.OperatorPrefix, (long)TopUpmodel.Value, TopUpmodel.ToUser);//smsHelper.SendVendorHttpRequest(ConstMessage.ezze_VendorId, ConstMessage.Request_Type_Request, newTransaction.ServiceId, TopUpmodel.ToUser, (int)TopUpmodel.Value, newTransaction.ReferenceId, TopUpmodel.Type); //"SUCCESS: Request processed successfully.";//
                                    /////////////////////////////////////////////////////////////////////
                                    //static values for test purpose
                                    //requestResponse = new RequestResponse();
                                    //requestResponse.RefilSuccessDate = DateTime.Now;
                                    //requestResponse.RESPONSE_MSG = "response message";
                                    //requestResponse.Status = ConstMessage.NEPAL_SWIFT_SUCCESS;
                                    //requestResponse.TransactionId = "transaction-id-123";
                                    ////////////////////////////////////////////////////////////////////


                                    if (requestResponse.Status == ConstMessage.NEPAL_SWIFT_SUCCESS)
                                    {
                                        newTransaction.Status = ConstMessage.STATUS_COMPLETE_ID;
                                        newTransaction.ReferenceId = requestResponse.TransactionId;
                                        newTransaction.VendorId = route.VendorId;
                                        newTransaction.ClientId = 0;
                                        newTransaction.ToUser = TopUpmodel.ToUser;
                                        smsdr.STATUS = ConstMessage.STATUS_COMPLETE_ID;
                                        smsdr.REQUEST_ID = requestResponse.TransactionId;
                                        smsdr.REFILL_SUCCESS_TIME = requestResponse.RefilSuccessDate;
                                        smsdr.RESPONSE_MSG = requestResponse.RESPONSE_MSG;
                                        
                                        topUpEntry = admin_repo.TopUpDbEntry(newTransaction, smsdr, FromSms);
                                        ReturnDistributorTransactionCharges(dTranList, newTransaction.OperationNumber);

                                        if (topUpEntry)
                                            result = ConstMessage.STATUS_COMPLETE_ID;

                                    }
                                    else
                                    {
                                        errorLog = new SMSDR();
                                        errorLog = smsdr;
                                        errorLog.STATUS = ConstMessage.STATUS_FAILED_ID;
                                        errorLog.RESPONSE_MSG = ResponseMessage;
                                        admin_repo.SmsdrAdd(errorLog);
                                    }
                                    continue;

                                }
                                #endregion

                            #region Terminator
                                    //Developed By Md. Mojahidul Islam
                                    //12/3/2016 
                                else if(route.VendorId==ConstMessage.terminator_VendorId)
                                {
                                    newTransaction.ReferenceId = transaction_repo.VendorUniqueIdGenerator(ConstMessage.terminator_vendor_prefix);
                                    smsdr.REQUEST_ID = newTransaction.ReferenceId;
                                    smsdr.VendorId = route.VendorId;
                                    if (newTransaction.ToUser.Length == 13)
                                        newTransaction.ToUser = newTransaction.ToUser.Substring(2, 11);
                                    ResponseMessage = smsHelper.SendVendorHttpRequest(route.VendorId, ConstMessage.Request_Type_Request, newTransaction.ServiceId, TopUpmodel.ToUser, (int)TopUpmodel.Value, newTransaction.OperationNumber, TopUpmodel.Type); //"SUCCESS: Request processed successfully.";//

                                    newTransaction.VendorId = route.VendorId;

                                    if (ResponseMessage.Contains("SUCCESS"))
                                    {
                                        ViewBag.TopUpResult = ConstMessage.TOP_UP_SUCCESS_MSG;

                                        newTransaction.VendorId = route.VendorId;

                                        if (ClientId != null)
                                            newTransaction.ClientId = (int)ClientId;
                                        else
                                            newTransaction.ClientId = 0;
                                        //returns true if all db entry is successful
                                        topUpEntry = admin_repo.TopUpDbEntry(newTransaction, smsdr, FromSms);
                                        ReturnDistributorTransactionCharges(dTranList, newTransaction.OperationNumber);


                                        if (topUpEntry)
                                            result = ConstMessage.STATUS_PENDING_ID;

                                        //Vendor Transaction
                                        vendor_repo.VendorTransaction_Add(newTransaction, ConstMessage.Balance_Deduct);
                                        vendor_repo.VendorRoute_UpdatePendingQty(route.Id, true);

                                        break;
                                    }
                                    else
                                    {
                                        errorLog = new SMSDR();
                                        errorLog = smsdr;
                                        errorLog.STATUS = ConstMessage.STATUS_FAILED_ID;
                                        errorLog.RESPONSE_MSG = ResponseMessage;
                                        admin_repo.SmsdrAdd(errorLog);
                                    }
                                    continue;
                                }


                            #endregion

                            //}
                            //catch (Exception ex)
                            //{
                            //    string a = ex.Message;
                            //    ViewBag.TopUpResult = ConstMessage.TOP_UP_FAILED_MSG;
                            //    result = ConstMessage.STATUS_FAILED_ID;
                            //}
                        }
                        if(!topUpEntry)
                        {
                            ViewBag.TopUpResult = ConstMessage.TOP_UP_FAILED_MSG;
                            result = ConstMessage.STATUS_FAILED_ID;
                            //admin_repo.UserBalanceUpdate(user.UserName, (decimal)TopUpmodel.TotalAmount);
                            Balance += (decimal)TopUpmodel.TotalAmount;
                            _variable.ApiStatus = ConstMessage.STATUS_MONEY_TRANSFER_FAILED_ID;

                        }
                        //else
                        //{
                        //    Balance = user.CurrentBalance - TopUpmodel.TotalAmount;
                        //}
                        //Balance = user.CurrentBalance;
                        ConvertFromUsd = currency_repo.GetConversionRate(user.LocalCurrencyId);
                        ISO = admin_repo.GetUserCurrencyISOByUserName(user.UserName);
                    }
                    else if (user.CurrentBalance < TopUpmodel.TotalAmount)
                    {
                        ErrorMessage = "You dont have sufficient balance.";
                        Balance = user.CurrentBalance;
                    }
                    
                }
                else
                    result = ConstMessage.STATUS_USER_NOT_FOUND_ID; //user not found 500
            }
            if (ClientId != null && ClientId != ConstMessage.FROM_SERVER)
            {
                //if (ClientId > 500 && ClientId < 1001)
                //{
                if (result == ConstMessage.STATUS_PENDING_ID)
                {
                    string OperationNumber = newTransaction.OperationNumber;
                    return Json(new { result, Balance, ConvertFromUsd, ISO, OperationNumber }, JsonRequestBehavior.AllowGet);//LocalBalance
                }
                else if(result == ConstMessage.STATUS_COMPLETE_ID)
                {
                    string OperationNumber = newTransaction.OperationNumber;
                    return Json(new { result, Balance, ConvertFromUsd, ISO, OperationNumber = newTransaction.ReferenceId, TransactionId = newTransaction.ReferenceId }, JsonRequestBehavior.AllowGet);//LocalBalance
                }
                else
                    return Json(new { result, Balance, ConvertFromUsd, ISO, ErrorMessage }, JsonRequestBehavior.AllowGet);//LocalBalance
                //}
                //else
                    //return Json(new { result, Balance, ConvertFromUsd, ISO }, JsonRequestBehavior.AllowGet);//LocalBalance
            }
            

            return RedirectToAction("TopUp", new { ViewBag.TopUpResult });
        }


        /*---------------------------------------------------------
       *   This is MoneyTransfer HttpGet method ViewData is for drop down list
       *---------------------------------------------------------*/
        [HttpGet]
        public ActionResult MoneyTransfer(string Message)
        {
            ViewBag.Message = Message;
          
            try
            {
                var dest = new SelectList(destination_repo.GetAll(), "Id", "DestinationName", ConstMessage.SELECTED_BAN_DESTINATION_ID);
                ViewData["Dest"] = dest;
                var service = new SelectList(service_repo.Search(null, null, 1, null, true), "Id", "Name", ConstMessage.SELECTED_SERVICE_ID);
                ViewData["Service"] = service;
                var rtplan = ratePlan_repo.GetAll();
                ViewBag.RatePlan = rtplan;

                return View();
            }
            catch (Exception)
            {
                
            }
            return View("Error");
        }


        /*---------------------------------------------------------
         *   This is MoneyTransfer HttpPost method ,FromAPI has value when request comes from mobile or device
         *   Initially MoneyTransfer status is assigned Failed and Balance=-1
         *   check model validity of payment and prefix.prefix is user mobile number country code hear only bangladesh is given 880
         *   with Status Pending and MoneyTransfer Operation Number        
         *   if MOney Transfer Success then decrease User money in line   user.CurrentBalance -= payment.TotalAmount;
         *   if  Error or exception then changed status Failed
         *---------------------------------------------------------*/
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult MoneyTransfer(Payment payment, string prefix, int? fromApi, string UserName)
        {

            _variable.ApiStatus = ConstMessage.STATUS_MONEY_TRANSFER_FAILED_ID;
            _variable.Balance = ConstMessage.STATUS_SET_USER_BALANCE;  //-1
            //_variable.LocalBalance = ConstMessage.STATUS_SET_USER_BALANCE;
            _variable.ConvertFromUsd = 1;
            bool moneyTransferEntry = false;
            Service selectedService;
            RatePlan selectedRateplan;

            if (ModelState.IsValid && !string.IsNullOrEmpty(prefix))
            {
                if (fromApi == ConstMessage.API_FROM)
                {
                    newTransaction.UserId = prefix;
                    _variable.UserName = payment.MobileNo;
                }
                else
                {
                    prefix = "0";
                    newTransaction.UserId = HttpContext.User.Identity.Name;
                    _variable.UserName = prefix + payment.MobileNo;
                }

                var user = UserManager.FindByName(newTransaction.UserId);

                if (user == null)
                {
                    if (fromApi == ConstMessage.API_FROM)
                        return Json(new { _variable.ApiStatus, _variable.Balance }, JsonRequestBehavior.AllowGet);

                    ModelState.AddModelError("", "Invalid User Name.Please Log In");


                    ViewData["Dest"] = new SelectList(destination_repo.GetAll(), "Id", "DestinationName", ConstMessage.SELECTED_BAN_DESTINATION_ID);
                    ViewData["Service"] = new SelectList(service_repo.Search(null, null, 1, null, true), "Id", "Name", ConstMessage.SELECTED_SERVICE_ID);
                    ViewBag.RatePlan = ratePlan_repo.GetAll();

                    return View(payment);
                }
                else
                {
                    //todo - calculate all submitted data based on service and value... money transfer
                    #region Recalculate moneyTransfer
                    try
                    {
                        selectedService = service_repo.FindById(payment.ServiceId);
                        selectedRateplan = ratePlan_repo.FindByService(payment.ServiceId);
                        payment.AmountInUSD = payment.Amount / selectedRateplan.ConvertionRate;
                        //payment.FromCurrencyId = ;

                        if (selectedRateplan.ServiceChargeIsPercentage != null && (bool)selectedRateplan.ServiceChargeIsPercentage)
                        {
                            if (selectedRateplan.MRPisPercentage)
                            {
                                payment.ProcessingFee = (payment.AmountInUSD * (selectedRateplan.MRP / 100)) + (payment.AmountInUSD * (selectedRateplan.ServiceCharge / 100));
                            }
                            else
                                payment.ProcessingFee = (payment.AmountInUSD * (selectedRateplan.ServiceCharge / 100));
                        }
                        else
                            payment.ProcessingFee = selectedRateplan.ServiceCharge;

                        payment.TotalAmount = payment.AmountInUSD + payment.ProcessingFee;
                    }
                    catch (Exception)
                    {
                        if (fromApi == ConstMessage.API_FROM)
                            return Json(new { _variable.ApiStatus, _variable.Balance }, JsonRequestBehavior.AllowGet);

                        ViewData["Dest"] = new SelectList(destination_repo.GetAll(), "Id", "DestinationName", ConstMessage.SELECTED_BAN_DESTINATION_ID);
                        ViewData["Service"] = new SelectList(service_repo.Search(null, null, 1, null, null), "Id", "Name");
                        ViewBag.RatePlan = ratePlan_repo.GetAll();

                        return View("MoneyTransfer", payment);
                    }
                    
                    #endregion

                    #region transaction and smsdr assignments
                    //get ammount from Pin by using pin code
                    newTransaction.ToUser = _variable.UserName;
                    newTransaction.ServiceId = payment.ServiceId;
                    newTransaction.OperationNumber = transaction_repo.OperationNumberGenarator(ConstMessage.PREFIX_MONEY_TRANSFER, DateTime.Now.ToString("ddMMyyyy"));

                    //get from currencyId
                    newTransaction.FromCurrencyId = payment.FromCurrencyId;
                    //GetCurrencyId(payment.FromCurrencyId);


                    //local currency amount 
                    newTransaction.InsertedAmount = payment.Amount;

                    //Converted amount including Service Charge
                    newTransaction.AmountOut = payment.TotalAmount;
                    newTransaction.TransactionDate = DateTime.Now;
                    newTransaction.Status = ConstMessage.STATUS_PENDING_ID;
                    newTransaction.VendorId = 0;
                    if(payment.PinId != null && payment.PinId > 0)
                        newTransaction.DistributorCommission = 0;
                    else
                        newTransaction.DistributorCommission = ConstMessage.DistributorCommissionNotReturned;


                    //now add data into SMSDR
                    SMSDR smsdr = new SMSDR();
                    smsdr.PHN_NO = _variable.UserName;
                    smsdr.PHN_NO_TYPE = "";
                    smsdr.TRANS_AMOUNT = payment.AmountInUSD;
                    smsdr.COST_AMOUNT = payment.TotalAmount;
                    smsdr.REQUEST_ID = "";
                    smsdr.REQUEST_TIME = DateTime.Now;
                    smsdr.REFILL_SUCCESS_TIME = null;
                    smsdr.STATUS = ConstMessage.STATUS_PENDING_ID; //1
                    smsdr.RESPONSE_MSG = "";
                    smsdr.GW_ID = "";
                    smsdr.CREATED_BY = newTransaction.UserId;

                    IEnumerable<DistributorServiceChargeVM> distributorCharges = new List<DistributorServiceChargeVM>();
                    List<DistributorTransaction> dTranList = new List<DistributorTransaction>();

                    UserRoleRepository role_repo = new UserRoleRepository();
                    eBankingUser currentUser;
                    if (fromApi != null && fromApi == ConstMessage.API_FROM && !string.IsNullOrEmpty(UserName))
                    {
                        currentUser = role_repo.FindUserByUserName(UserName);
                    }
                    else
                    {
                        UserName = User.Identity.Name;
                        currentUser = role_repo.FindUserByUserName(UserName);
                    }
                    if (!string.IsNullOrEmpty(currentUser.DistributorCode))
                    {
                        distributorCharges = distributor_repo.DCRP_GetDSCforEachDistributor(newTransaction.ServiceId, currentUser.DistributorCode);//currentUser.DistributorCode  "1-2-3-7"
                        foreach (var distributor in distributorCharges)
                        {
                            DistributorTransaction dTran = new DistributorTransaction();
                            dTran.AmountIn = distributor.ServiceCharge;
                            dTran.ConvertToUsd = 1;
                            dTran.CreatedBy = UserName;
                            dTran.CreatedOn = DateTime.Now;
                            dTran.CurrencyId = ConstMessage.SELECTED_USD_DESTINATION_ID;
                            dTran.DistributorId = distributor.DistributorId;
                            dTran.Remarks = "CommissionOfService:" + newTransaction.ServiceId + ", Username:" + newTransaction.UserId;

                            newTransaction.AmountOut += (dTran.AmountIn ?? 0);

                            dTranList.Add(dTran);

                        }
                    }

                    #endregion
                    if (newTransaction.FromCurrencyId > 0)
                    {
                        newTransaction.RatePlanId = getRatePlanId(newTransaction.FromCurrencyId, newTransaction.ServiceId);
                        var tempPendingTime = ratePlan_repo.FindById(newTransaction.RatePlanId).PendingTime;
                        double pendingTime = (double)(tempPendingTime ?? 0);

                        if (ConstMessage.MONEY_TRANSFER_TIMEOUT_CONFIG) //pendingTime != null && 
                        {
                            newTransaction.TimeOut = newTransaction.TransactionDate.Value.AddMinutes(pendingTime);
                            newTransaction.IsTimeOut = false;
                        }
                        else
                        {
                            newTransaction.TimeOut = newTransaction.TransactionDate;
                            newTransaction.IsTimeOut = true;
                        }
                    }


                    if (newTransaction.FromCurrencyId < 0 || newTransaction.RatePlanId < 0 || (newTransaction.OperationNumber == null && string.IsNullOrEmpty(newTransaction.OperationNumber)))
                    {
                        if (fromApi == ConstMessage.API_FROM)
                            return Json(new { _variable.ApiStatus, _variable.Balance }, JsonRequestBehavior.AllowGet);

                        return View("Error");
                    }
                    else if (user.CurrentBalance < newTransaction.AmountOut)
                    {
                        _variable.ApiStatus = ConstMessage.STATUS_CREDIT_TRANSFER_EXCEED_BALANCE_ID; //3

                        if (fromApi == ConstMessage.API_FROM)
                            return Json(new { _variable.ApiStatus, _variable.Balance }, JsonRequestBehavior.AllowGet);

                        ViewBag.Error = "You don't have sufficient amount to transfer. Please recharge first.";

                        ViewData["Dest"] = new SelectList(destination_repo.GetAll(), "Id", "DestinationName", ConstMessage.SELECTED_BAN_DESTINATION_ID);
                        ViewData["Service"] = new SelectList(service_repo.Search(null, null, 1, null, true), "Id", "Name", ConstMessage.SELECTED_SERVICE_ID);
                        ViewBag.RatePlan = ratePlan_repo.GetAll();
                        return View(payment);
                    }
                    else
                    {
                        #region try catch
                        try
                        {
                            smsdr.RATE_PLANE_ID = (int)newTransaction.RatePlanId;


                            //admin_repo.UserBalanceUpdate(newTransaction.UserId, -(decimal)newTransaction.AmountOut);
                            _variable.Balance = user.CurrentBalance - (decimal)(newTransaction.AmountOut);
                            //todo change hard-coded bkash to accept service
                            //int selectedVendorId = ConstMessage.ezze_VendorId;//ConstMessage.SELECTED_VENDOR_FOR_MONEY_TRANSFER;
                            IVendorRepository vendor_repo = new VendorRepository();
                            IEnumerable<VendorRoute> availableVendors = vendor_repo.VendorRoute_GetAllByService(payment.ServiceId);
                            string ResponseMessage = "";
                            foreach (var route in availableVendors)
                            {
                                if (vendor_repo.FindVendorById(route.VendorId).CurrentBalance < newTransaction.InsertedAmount)
                                    continue;
                                //HACK - the following code is completely hard-coded due to insufficient similarities of vendor request formats, new codes need to be added to support more vendors
                                if (route.VendorId == ConstMessage.ezze_VendorId || route.VendorId == ConstMessage.ezze_bkash_VendorId)
                                {
                                    newTransaction.ReferenceId = transaction_repo.VendorUniqueIdGenerator(ConstMessage.ezze_vendor_prefix);//smsHelper.GenerateReferenceId(newTransaction.OperationNumber, newTransaction.ToUser);
                                    smsdr.REQUEST_ID = newTransaction.ReferenceId;
                                    if (newTransaction.ToUser.Length == 13)
                                        newTransaction.ToUser = newTransaction.ToUser.Substring(2, 11);
                                    ResponseMessage = smsHelper.SendVendorHttpRequest(route.VendorId, ConstMessage.Request_Type_Request, payment.ServiceId, newTransaction.ToUser, (int)newTransaction.InsertedAmount, newTransaction.ReferenceId, null); //"SUCCESS: Request processed successfully.";//
                                    //ResponseMessage = "SUCCESS";
                                    smsdr.RESPONSE_MSG = ResponseMessage;
                                    smsdr.VendorId = route.VendorId;
                                    newTransaction.VendorId = route.VendorId;

                                    if (ResponseMessage.Contains("SUCCESS"))
                                    {
                                        //moneyTransferEntry = transaction_repo.Add(newTransaction);
                                        
                                        
                                        //admin_repo.SmsdrAdd(smsdr);                            //smsdr is saved
                                        moneyTransferEntry = admin_repo.MoneyTransferDbEntry(newTransaction, smsdr);
                                        ReturnDistributorTransactionCharges(dTranList, newTransaction.OperationNumber);  //distributor transaction charges are returned

                                        //Vendor Transaction
                                        vendor_repo.VendorTransaction_Add(newTransaction,ConstMessage.Balance_Deduct);
                                        vendor_repo.VendorRoute_UpdatePendingQty(route.Id, true);
                                    }
                                    break;
                                }
                                else
                                {
                                    smsdr.STATUS = ConstMessage.STATUS_FAILED_ID;
                                    //ResponseMessage = "SUCCESS"; //to keep the entry in transaction table so that vendor can pull from there
                                    //moneyTransferEntry = transaction_repo.Add(newTransaction);
                                }
                            }

                            //moneyTransferEntry = transaction_repo.Add(newTransaction);
                            

                            //todo user balance and transaction table are not maintaining db transaction
                            //moneyTransferEntry = admin_repo.MoneyTransferDbEntry(newTransaction, newTransaction.UserId, 0);

                            ViewBag.OperationNumber = "";
                            if (moneyTransferEntry)
                            {
                                //smsdr.TRANSACTION_ID = newTransaction.Id;
                                //admin_repo.SmsdrAdd(smsdr);                            //smsdr is saved
                                _variable.Message = "Transaction Successful.Transaction Number " + newTransaction.OperationNumber + ".";
                                ViewBag.OperationNumber = newTransaction.OperationNumber;
                                _variable.ApiStatus = ConstMessage.STATUS_MONEY_TRANSFER_SUCCESS_ID;
                            }
                            else
                            {
                                if (ResponseMessage.Contains("Number format invalid"))
                                    _variable.Message = "Invalid number. Please try again.";
                                else
                                    _variable.Message = "Transaction Failed. Please try again later.";

                                admin_repo.SmsdrAdd(smsdr);
                                //admin_repo.UserBalanceUpdate(newTransaction.UserId, (decimal)newTransaction.AmountOut);
                                _variable.Balance += (decimal)newTransaction.AmountOut;
                                _variable.ApiStatus = ConstMessage.STATUS_MONEY_TRANSFER_FAILED_ID;
                            }

                            

                        }
                        catch (Exception)
                        {
                            //to do log file if failed 
                            //ModelState.AddModelError("", _variable.Message);
                            _variable.ApiStatus = ConstMessage.STATUS_FAILED_ID;
                        }
                        //_variable.LocalBalance = currency_repo.ConvertToLocal(_variable.Balance, user.LocalCurrencyId);
                        _variable.ConvertFromUsd = currency_repo.GetConversionRate(user.LocalCurrencyId);
                        _variable.ISO = admin_repo.GetUserCurrencyISOByUserName(user.UserName);
                        if (fromApi == ConstMessage.API_FROM)
                            return Json(new { _variable.ApiStatus, _variable.Balance, _variable.ConvertFromUsd, _variable.ISO, OperationNumber = ViewBag.OperationNumber }, JsonRequestBehavior.AllowGet); //_variable.LocalBalance

                        return RedirectToAction("MoneyTransfer", new { Message = _variable.Message });
                        #endregion
                    }
                }   //eles close 
            }

            if (fromApi == ConstMessage.API_FROM)
                return Json(new { _variable.ApiStatus, _variable.Balance }, JsonRequestBehavior.AllowGet);

            //db.Destinations.ToList()
            ViewData["Dest"] = new SelectList(destination_repo.GetAll(), "Id", "DestinationName", ConstMessage.SELECTED_BAN_DESTINATION_ID);
            ViewData["Service"] = new SelectList(service_repo.Search(null, null, 1, null, null), "Id", "Name");
            ViewBag.RatePlan = ratePlan_repo.GetAll();

            return View("MoneyTransfer", payment);
        }
        

       /*---------------------------------------------------------
       * Credit Transfer.
       * This option Is used when a Registered user transfer money from Own Account
       * to another Registered User Account.
       * This is HttpGet Credit Transfer Page View Model 
       *---------------------------------------------------------*/
        [HttpGet]
       public ActionResult CreditTransfer()
        {            
            return View();
        }


       /*---------------------------------------------------------
       *   transfer Money from your Own account to another  registered user account
       *   In transaction table it has two entry 
        *  one is for dicrease transfered money (Amount Out) from your account
        *  another entry is Increase transfer money (Amount In) 
        *  and update both user current money in user table
       *---------------------------------------------------------*/
        [HttpPost]
        public ActionResult CreditTransfer(CreditTransfer model, int? fromApi,string UserNo)
        {
            _variable.ApiStatus = ConstMessage.STATUS_CREDIT_TRANSFER_FAILED_ID; //0
            _variable.Balance = ConstMessage.STATUS_SET_USER_BALANCE;  //-1
            //_variable.LocalBalance = ConstMessage.STATUS_SET_USER_BALANCE;
            _variable.ConvertFromUsd = 1;

            if (fromApi == ConstMessage.API_FROM)
            {
                model.UserName = UserNo;
            }
            else
            {
                model.UserName = HttpContext.User.Identity.Name;
            }

            if (model.UserName == null && string.IsNullOrEmpty(model.UserName))
            {

                _variable.ApiStatus = ConstMessage.STATUS_USER_NOT_FOUND_ID;  //500
                ModelState.AddModelError("", "User not found.Please Log In !!");
            }
            else
            {
                if (model.UserName == model.ToUser)
                {
                    _variable.ApiStatus = ConstMessage.STATUS_CREDIT_TRANSFER_USER_TOUSER_SAME_ID;  //2
                    ModelState.AddModelError("",ConstMessage.STATUS_CREDIT_TRANSFER_USER_TOUSER_SAME_MSG);
                }
                else
                {
                    try
                    {
                        var validuser = UserManager.FindByName(model.ToUser);

                        var FromUser = UserManager.FindByName(model.UserName);

                        if (validuser != null && FromUser !=null)
                        {
                            if (FromUser.CurrentBalance >= model.Balance)
                            {
                                //string clientId = System.Web.HttpContext.Current.Session["clientId"] as string;
                                //Transaction for Sender User
                                newTransaction = new Transaction();
                                newTransaction.UserId = model.ToUser;
                                newTransaction.FromUser = model.UserName;
                                newTransaction.ToUser = model.ToUser;
                                newTransaction.FromCurrencyId = ConstMessage.SELECTED_USD_DESTINATION_ID;
                                newTransaction.AmountIN = model.Balance;
                                newTransaction.TransactionDate = DateTime.Now;
                                newTransaction.Status = ConstMessage.STATUS_COMPLETE_ID;
                                newTransaction.ServiceId = ConstMessage.STATUS_SERVICE_CREDIT_TRANSFER_ID;  //12 in service table
                                newTransaction.OperationNumber=_variable.AllStringVar = transaction_repo.OperationNumberGenarator(ConstMessage.PREFIX_CREDIT_TRANSFER, DateTime.Now.ToString("ddMMyyyy"));
                                newTransaction.VendorId = 0;
                                newTransaction.DistributorCommission = 0;
                                //newTransaction.ClientId = (string.IsNullOrEmpty(clientId)? 0 : Convert.ToInt32(clientId)); 

                                

                                //Transaction for Recipient User
                                Transaction outTransaction = new Transaction();
                                outTransaction.FromUser = model.UserName;
                                outTransaction.UserId = model.UserName;
                                outTransaction.ToUser = model.ToUser;
                                outTransaction.FromCurrencyId = ConstMessage.SELECTED_USD_DESTINATION_ID;
                                outTransaction.AmountOut = model.Balance;
                                outTransaction.TransactionDate = DateTime.Now;
                                outTransaction.Status = ConstMessage.STATUS_COMPLETE_ID;
                                outTransaction.ServiceId = ConstMessage.STATUS_SERVICE_CREDIT_TRANSFER_ID;  //12 in service table
                                outTransaction.OperationNumber = _variable.AllStringVar;
                                outTransaction.VendorId = 0;
                                outTransaction.DistributorCommission = 0;
                                

                                //credit transfer has two entry in transaction table
                                //beacuse money is interchanged between two registered user  

                                // save entries to db and assigns success messages
                                if (admin_repo.CreditTransferDbEntry(newTransaction, outTransaction, model.UserName, model.ToUser))
                                {
                                    _variable.Balance = FromUser.CurrentBalance - (model.Balance);
                                    //_variable.LocalBalance = currency_repo.ConvertToLocal(_variable.Balance, FromUser.LocalCurrencyId);
                                    _variable.ConvertFromUsd = currency_repo.GetConversionRate(FromUser.LocalCurrencyId);
                                    _variable.ISO = admin_repo.GetUserCurrencyISOByUserName(FromUser.UserName);
                                    _variable.ApiStatus = ConstMessage.STATUS_CREDIT_TRANSFER_SUCCESS_ID;
                                    ViewBag.Message = ConstMessage.STATUS_CREDIT_TRANSFER_SUCCESS_MSG;
                                    ViewBag.Success = ConstMessage.STATUS_CREDIT_TRANSFER_SUCCESS_MSG;
                                }
                                else
                                {
                                    _variable.ApiStatus = ConstMessage.EXCEPTION_STATUS_ID;  //9999
                                    ModelState.AddModelError("", ConstMessage.STATUS_INTERNAL_ERROR_MSG);
                                }
                            }
                            else
                            {
                                _variable.ApiStatus = ConstMessage.STATUS_CREDIT_TRANSFER_EXCEED_BALANCE_ID; //3
                                ModelState.AddModelError("",ConstMessage.STATUS_CREDIT_TRANSFER_EXCEED_BALANCE_MSG);
                            }
                        }
                        else
                        {
                            _variable.ApiStatus = ConstMessage.STATUS_INVALID_USER_ID;  //503
                            ModelState.AddModelError("", ConstMessage.STATUS_INVALID_USER_MSG);
                        }
                    }
                    catch (Exception)
                    {
                        _variable.ApiStatus = ConstMessage.EXCEPTION_STATUS_ID;  //9999
                        ModelState.AddModelError("",ConstMessage.STATUS_INTERNAL_ERROR_MSG);
                    }
                
                }
            
            }

            if (fromApi == 1)
            {
                return Json(new { _variable.ApiStatus, _variable.Balance, _variable.ConvertFromUsd, _variable.ISO }, JsonRequestBehavior.AllowGet);//_variable.LocalBalance
            }

            if (ViewBag.Message == null)
                return View(model);
            else
                return View();
        }

        ActionResult SoapTest()
        {
            var soap = new SoapManager();
            soap.SwiftGetCurrentBalance("");
            return Json(new { _variable.ApiStatus, _variable.Balance, _variable.ConvertFromUsd, _variable.ISO }, JsonRequestBehavior.AllowGet);//_variable.LocalBalance
        }

        #region Helper

        [NonAction]
        public int getRatePlanId(int fromCurrencyId, int serviceId)
        {
            try
            {
                //exception
                ratePlan = ratePlan_repo.GetAllQueryable().Where(x => x.FromCurrencyId == fromCurrencyId && x.ServiceId == serviceId && x.IsActive == true && x.ToCurrencyId == ConstMessage.SELECTED_USD_DESTINATION_ID).SingleOrDefault(); ;
                return ratePlan.Id;
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }

            return -1;
        }

        [NonAction]
        public bool VerifyTopUpService(int ServiceId, string PhoneNumber)
        {
            try
            {
                if (service_repo.FindById(ServiceId).ParentId == ConstMessage.Services_topUp_Parent)
                    return true;
            }
            catch (Exception) { }
            return false;
        }

        [NonAction]
        public SMSToUser ValidateToUser(string toUser, int? serviceId)
        {
            //int[] result = {0,0,0};
            SMSToUser result = new SMSToUser();
            Destination serviceCountry;

            if (toUser[0] == '+')
                toUser = toUser.Substring(1, (toUser.Length - 1));
            #region added on 29th october to remove "00" and add 88 if it is presumabaly bd number
            //TODO - check the requirement to change accordingly
            if (toUser.Substring(0, 2) == "00")
                toUser = toUser.Substring(2, (toUser.Length - 2));

            #region added on 14th December to match phone number with destination

            if (serviceId.HasValue)
            {
                serviceCountry = destination_repo.FindById(service_repo.FindById(serviceId).DestinationId);
                result.CountryCode = serviceCountry.CountryCode;
                
                if (toUser.Substring(0, serviceCountry.CountryCode.Length).Equals(serviceCountry.CountryCode))
                {
                    try
                    {
                        int countryCodeLength = serviceCountry.CountryCode.Length;
                        if (serviceCountry.CountryCode == "880")
                            result.PhoneNumber = toUser.Substring(toUser.Length - 11, 11);
                        else
                            result.PhoneNumber = toUser.Substring(countryCodeLength, (toUser.Length - countryCodeLength));
                    }
                    catch (Exception) { }
                }
                else
                {
                    if (serviceCountry.Id == ConstMessage.SELECTED_BAN_DESTINATION_ID)
                    {
                        if (toUser.Length >= ConstMessage.TOPUP_BD_MIN_LENGTH)
                        {
                            result.PhoneNumber = toUser.Substring(toUser.Length - 11, 11);
                            result.Valid = true;
                        }
                    }

                    else if (serviceCountry.Id == ConstMessage.SELECTED_NEP_DESTINATION_ID)
                    {
                        if(toUser.Length >= ConstMessage.TOPUP_NP_MIN_LENGTH)
                        {
                            result.PhoneNumber = toUser.Substring(toUser.Length - ConstMessage.TOPUP_NP_MIN_LENGTH, ConstMessage.TOPUP_NP_MIN_LENGTH);
                            //result.Valid = true;
                        }
                    }
                }

                if (serviceCountry.Id == ConstMessage.SELECTED_NEP_DESTINATION_ID)
                {
                    string operatorPrefix = result.PhoneNumber.Substring(0, 3);
                    if (operatorPrefix.Equals("980") || operatorPrefix.Equals("981") || operatorPrefix.Equals("982"))
                    {
                        result.OperatorPrefix = ConstMessage.NEPAL_SWIFT_NCELL;
                        result.Valid = true;
                    }
                    else if (operatorPrefix.Equals("984") || operatorPrefix.Equals("986"))
                    {
                        result.OperatorPrefix = ConstMessage.NEPAL_SWIFT_PREPAID;
                        result.Valid = true;
                    }
                    else if (operatorPrefix.Equals("985"))
                    {
                        result.OperatorPrefix = ConstMessage.NEPAL_SWIFT_POSTPAID;
                        result.IsPostPaid = true;
                        result.Valid = true;
                    }
                }
            }

            #endregion

            //if (toUser.Length == 11 && (toUser.Substring(0, 3) == "016" || toUser.Substring(0, 3) == "017" || toUser.Substring(0, 3) == "018" || toUser.Substring(0, 3) == "019"))
            //    toUser = "88" + toUser;
            #endregion

            return result;
        }
        [NonAction]
        public bool ReturnDistributorTransactionCharges(IEnumerable<DistributorTransaction> dTran, string OperationNumber)
        {
            
            try
            {
                foreach (var item in dTran)
                {
                    if (item.AmountIn != null && item.AmountIn > 0)
                    {
                        item.Remarks += ",OperationNumber:" + OperationNumber;
                        distributor_repo.DistributorTransaction_Add(item);
                    }
                }
                return true;
            }
            catch (Exception) { }
            return false;
        }

        #endregion
    }
}