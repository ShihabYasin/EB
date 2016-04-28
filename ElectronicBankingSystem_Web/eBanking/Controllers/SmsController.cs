using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.Interface;
using eBanking.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace eBanking.Controllers
{
    [AllowAnonymous]
    public class SmsController : Controller
    {
        #region Local Variables, Repository and Instantiations
        //private eBankingDbContext db;
        private UserController userController = new UserController();
        private eBankingUser user = new eBankingUser();
        //private AccountController accountController = new AccountController();
        private IDestinationRepository destination_repo;
        private IServiceRepository service_repo;
        private IRatePlanRepository ratePlan_repo;
        private IUserRoleRepository userRole_repo;
        //private UserManager<eBankingUser> UserManager { get; set; }

        private HttpClient webClient = new HttpClient();

        public SmsController()
        {
            //db = new eBankingDbContext();
            destination_repo = new DestinationRepository();
            service_repo = new ServiceRepository();
            ratePlan_repo = new RateplanRepository();
            userRole_repo = new UserRoleRepository();

        
        }

        #endregion
        //
        // GET: /Sms/
        public ActionResult Index()
        {
            return View();
        }
        /*
         * Developed By  - Siddique
         * Developed On  - 12th September, 2015
         * Developed For - Receiving forwarded end-user sms request from SMS Client
         */
        [ActionName("SMSReceiver")]
        [HttpPost]
        public ActionResult SMSReceiver(IEnumerable<SMSResponse> smsList) //string UserName, string Message
        {
            AdminRepository admin_repo = new AdminRepository();
            List<SMSResponse> resultList = new List<SMSResponse>();
            foreach (var sms in smsList)
            {

                if (!string.IsNullOrWhiteSpace(sms.UserName) || !string.IsNullOrEmpty(sms.UserName))
                {
                    try
                    {
                        sms.MessageBody = sms.MessageBody.Trim().ToLower();                //convert all to lowercase for convenience
                        sms.MessageBody = System.Text.RegularExpressions.Regex.Replace(sms.MessageBody, @"\s+", " "); // trims all extra white space inside sms
                        string[] SplitMessages = sms.MessageBody.Split(' ');        //format --> top amount mobile_with_country_code pre/post
                        user = admin_repo.GetUserByUserName(sms.UserName);//userRole_repo.FindUserByUserName(sms.UserName);//accountController.UserManager.FindByName(UserName);

                        SMSResponse response = new SMSResponse();
                        SMSResponse response_additional = new SMSResponse();

                        #region Authentication (not implemented)
                        //LoginViewModel login = new LoginViewModel();
                        //login.UserName = UserName;
                        //login.Password = SplitMessages[1];
                        //login.RememberMe = false;
                        //accountController.Login(login,null);

                        //IPasswordHasher pass = new PasswordHasher() ;
                        //var passResult = pass.VerifyHashedPassword(user.PasswordHash, SplitMessages[1]);

                        //HttpResponseMessage response = null;
                        //int LoginStatus = 0;
                        //if(SplitMessages.Count() > 1)
                        //{
                        //    webClient = new HttpClient { BaseAddress = new Uri("http://localhost:36032/") };
                        //    var pairs = new List<KeyValuePair<string, string>>
                        //              {
                        //                new KeyValuePair<string, string>("UserName",UserName),
                        //                new KeyValuePair<string, string>("Password",SplitMessages[1]),
                        //                new KeyValuePair<string, string>("ApkVersionName",ConstMessage.APK_VERSION)
                        //              };

                        //    var content = new FormUrlEncodedContent(pairs);
                        //    response = webClient.PostAsync("Account/ApiLogin", content).Result;
                        //    var result = response.Content.ReadAsStringAsync().Result;
                        //    try
                        //    {
                        //        var obj = JObject.Parse(result);
                        //        LoginStatus = (int)obj["ApiStatus"];
                        //        if(LoginStatus == 1)
                        //            user = accountController.UserManager.FindByName(UserName);
                        //    }
                        //    catch (Exception) { }
                        //}
                        #endregion

                    
                        switch (SplitMessages[0])
                        {
                            case "reg":  //register
                                break;

                            case "rec":  //recharge
                                response = Recharge(sms.UserName, sms.MessageBody, (user != null),null);
                                //return Json(new { Data = response }, JsonRequestBehavior.AllowGet);
                                resultList.Add(response);
                                break;

                            case "top":
                                #region top up switch
                                //todo - service id is not sent..... will not be able to match number with destination
                                var toUser = userController.ValidateToUser(SplitMessages[2],null);//ValidateToUser(SplitMessages[2]);
                                    if (!toUser.Valid)                                      //not a valid user to recharge
                                        break;
                                    int phoneType = 1;

                                    #region multiple formatting of top up
                                    switch (SplitMessages.Length)
                                    { 
                                        case 4:
                                            {
                                                if (SplitMessages[3] == "po")
                                                {
                                                    phoneType = 2;
                                                    response = TopUp(sms.Id,sms.UserName, phoneType, sms.ClientId, SplitMessages[1], toUser);
                                                    //return Json(new { Data = response }, JsonRequestBehavior.AllowGet);
                                                    resultList.Add(response);
                                                    break;
                                                }
                                                else if(SplitMessages[3].Length == ConstMessage.VOUCHER_PIN_LENGTH)
                                                {
                                                    string rechargeMessageBody = "rec "+ SplitMessages[3];
                                                    response_additional = Recharge(sms.UserName, rechargeMessageBody, (user != null), null);
                                                    if (response_additional.MessageBody.Contains("credited"))
                                                        response_additional.MessageBody = "Your account has been successfully recharged";
                                                    else
                                                        response_additional.MessageBody = "Invalid Pin, recharge is not successful";
                                                    response = TopUp(sms.Id,sms.UserName, phoneType, sms.ClientId, SplitMessages[1], toUser);

                                                    SMSResponse mergedSms = MergeTwoResponses(response, response_additional);
                                                    //return Json(new { Data = mergedSms }, JsonRequestBehavior.AllowGet);
                                                    resultList.Add(mergedSms);
                                                    break;
                                                }
                                                break;
                                            }
                                        
                                        case 5:
                                            {
                                                if (SplitMessages[4].Length == ConstMessage.VOUCHER_PIN_LENGTH)
                                                {
                                                    string rechargeMessageBody = "rec " + SplitMessages[4];
                                                    response_additional = Recharge(sms.UserName, rechargeMessageBody, (user != null), null);
                                                }
                                               
                                                if (SplitMessages[3] == "po")
                                                    response = TopUp(sms.Id,sms.UserName, phoneType, sms.ClientId, SplitMessages[1], toUser);

                                                SMSResponse mergedSms = MergeTwoResponses(response, response_additional);
                                                //return Json(new { Data = mergedSms }, JsonRequestBehavior.AllowGet);
                                                resultList.Add(mergedSms);
                                                break;
                                            }
                                        default:
                                            {
                                                response = TopUp(sms.Id,sms.UserName, phoneType, sms.ClientId, SplitMessages[1], toUser);
                                                //return Json(new { Data = response }, JsonRequestBehavior.AllowGet);
                                                resultList.Add(response);
                                                break;
                                            }
                                    }
                                    #endregion 
                                break;
                                #endregion

                            //case "help":
                            //    {
                            //        response.UserName = sms.UserName;
                            //        response.ApiStatus = ConstMessage.STATUS_COMPLETE_ID;
                            //        response.MessageBody = ConstMessage.SMS_FORMATS_HELP;

                            //        //return Json(new { Data = response }, JsonRequestBehavior.AllowGet);
                            //        resultList.Add(response);
                            //        break;
                            //    }
                            default :
                                {
                                    if (SplitMessages.Length == 2 && SplitMessages[0].Length == ConstMessage.VOUCHER_PIN_LENGTH)
                                    {
                                        IPinRepository pin_repo = new PinRepository();
                                        int? PinServiceId = pin_repo.PinService(SplitMessages[0]);
                                        Pin pin = pin_repo.FindByPinCode(SplitMessages[0]);
                                        if(PinServiceId != null)
                                        {
                                            if (user == null)
                                            {
                                                AccountController accountController = new AccountController();
                                                RegisterViewModel reg = new RegisterViewModel();
                                                reg.UserName = sms.UserName;
                                                var registration = accountController.Register(reg, true).Result;
                                            }
                                            userController.VerifyTopUpService((int)PinServiceId, SplitMessages[1]);
                                            var toUserDefault = userController.ValidateToUser(SplitMessages[1],pin.ServiceId);
                                            //string PhoneNumber = "0" + toUserDefault.PhoneNumber;
                                            pin_repo.PinChangeStatus(pin.Id, sms.UserName, ConstMessage.STATUS_PROCESSING_ID);
                                            response = TopUp(sms.Id,sms.UserName, 1, sms.ClientId, SplitMessages[0], toUserDefault);

                                            if (response.ApiStatus == ConstMessage.STATUS_PENDING_ID || response.ApiStatus == ConstMessage.STATUS_COMPLETE_ID)
                                            {
                                                //Pin pin = pin_repo.FindByPinCode(SplitMessages[0]);
                                                //pin.ExecutionDate = DateTime.Now;
                                                //pin.UsedBy = sms.UserName;
                                                //pin.Status = ConstMessage.PIN_USED_STATUS_ID;
                                                //pin_repo.Edit(pin);
                                                pin_repo.PinChangeStatus(pin.Id, sms.UserName, ConstMessage.PIN_USED_STATUS_ID);

                                                //if(string.IsNullOrEmpty(user.DistributorCode) && !string.IsNullOrEmpty(pin.DistributorCode))
                                                //{
                                                //    user.DistributorCode = pin.DistributorCode;
                                                //    UserManager = new UserManager<eBankingUser>(new UserStore<eBankingUser>(new eBankingDbContext()));
                                                //    UserManager.Update(user);
                                                //}
                                            }
                                            else
                                            {
                                                pin_repo.PinReactivate(pin.Id);
                                            }

                                            //return Json(new { Data = response }, JsonRequestBehavior.AllowGet);
                                            resultList.Add(response);
                                            break;
                                        }
                                        else
                                        {
                                            sms.ApiStatus = ConstMessage.STATUS_FAILED_ID;
                                            if(pin != null && pin.Status == ConstMessage.PIN_USED_STATUS_ID)
                                                sms.MessageBody = "Pin is already used. Please try with a valid pin number or contact administrator.";
                                            else
                                                sms.MessageBody = "Invalid Pin. Please try with a proper pin number or contact administrator.";

                                            //return Json(new { Data = sms }, JsonRequestBehavior.AllowGet);
                                            resultList.Add(sms);
                                            break;
                                        }

                                        #region old implementation
                                        //string rechargeMessageBody = "rec " + SplitMessages[0];
                                        //response_additional = Recharge(sms.UserName, rechargeMessageBody, (user != null),1);
                                        //if (response_additional.ApiStatus == ConstMessage.STATUS_COMPLETE_ID || response_additional.ApiStatus == ConstMessage.PIN_USED_STATUS_ID)
                                        //{
                                        //    var to_user = ValidateToUser(SplitMessages[1]);
                                        //    if (!to_user.Valid)                                      //not a valid user to recharge
                                        //        break;

                                        //    string[] rechargeResponseSplitMessages = response_additional.MessageBody.Split(' ');
                                        //    string pinValue = rechargeResponseSplitMessages[6];
                                        //    string countryCode = to_user.CountryCode;

                                        //    //IPinRepository pin_repo = new PinRepository();
                                        //    string topUpValue = "0";//pin_repo.GetTopUpValueFromPinTopUP(pinValue, to_user.CountryCode);

                                        //    foreach (var item in ConstMessage.pinTopUps)
                                        //    {
                                        //        if (item.PinValue == pinValue && item.CountryCode == to_user.CountryCode)
                                        //            topUpValue = item.TopUpValue;
                                        //    }

                                        //    response = topUp(sms.UserName, 1, sms.ClientId, topUpValue, to_user.PhoneNumber, to_user.CountryCode);

                                        //    return Json(new { Data = response }, JsonRequestBehavior.AllowGet);
                                        //}
                                        //else
                                        //{
                                        //    return Json(new { Data = response_additional }, JsonRequestBehavior.AllowGet);
                                        //}
                                        #endregion
                                    }
                                    sms.MessageBody = "The request is invalid. Please try with proper SMS format.";
                                    sms.ApiStatus = ConstMessage.STATUS_FAILED_ID;
                                    //return Json(new { Data = sms }, JsonRequestBehavior.AllowGet);
                                    resultList.Add(sms);
                                    break;
                                }
                        }
                        //}
                    }
                    catch (Exception) { }
                }
                else
                {
                    sms.MessageBody = "The request is invalid. Please try with proper SMS format.";
                    sms.ApiStatus = ConstMessage.STATUS_FAILED_ID;
                    resultList.Add(sms);
                }
            }
            return Json(new { Data = resultList }, JsonRequestBehavior.AllowGet);
        }

        /*
         * Developed By  - Siddique
         * Developed On  - 16th September, 2015 
         * Developed For - Providing sms client with the functionality to check top up current status
         * 
         * To Do         - Needs to incorporate authentication, check code if the assignments will be done here or the TransactionRepository
         */
        [ActionName("StatusCheck")]
        [HttpPost]
        public ActionResult StatusCheck(IEnumerable<SMSResponse> models)
        {
            ITransactionRepository transaction_repo = new TransactionRepository();
            List<SMSResponse> result = new List<SMSResponse>();

            List<string> operationNumbers = new List<string>();
            foreach (var item in models)
                operationNumbers.Add(item.OperationNumber);
            IEnumerable<Transaction> transactions = transaction_repo.GetTransactionStatusByOrderNumberList(operationNumbers);

            result = PrepareSmsResponseFromTransactionList(transactions);
            return Json(new { Data = result }, JsonRequestBehavior.AllowGet);

        }



        /*
         * new implementation: auto recharge if user doesnt exist
         */
        private SMSResponse Recharge(string UserName, string Message,bool Exists, int? fromPinTopUp)
        {
            SMSResponse smsResponse = new SMSResponse();
            smsResponse.UserName = UserName;
            if (!Exists)
            {
                AccountController accountController = new AccountController();
                RegisterViewModel reg = new RegisterViewModel();
                reg.UserName = UserName;
                var response = accountController.Register(reg, true).Result;
            }
            string[] commands = Message.Split(' ');

            Pin pin = new Pin();
            pin.PinCode = commands[1];
            var result = userController.Voucher(pin, ConstMessage.API_FROM, UserName); //implement ConstMessage.FROM_SMS
            var serializer = new JavaScriptSerializer();
            var serialized = serializer.Serialize(result);
            var obj = JObject.Parse(serialized);
            smsResponse.ApiStatus = (int)obj["Data"]["ApiStatus"];
            //smsResponse.ApiStatus = ConstMessage.STATUS_COMPLETE_ID;
            if (smsResponse.ApiStatus == ConstMessage.STATUS_COMPLETE_ID)
                smsResponse.MessageBody = "Your Account has been credited with " +
                    (string)obj["Data"]["PinValue"] + " " + //"1.2658" + " " + 
                    ConstMessage.SELECTED_CURRENCY_UNIT + ".Your current balance is " +
                    obj["Data"]["Balance"] + " " + ConstMessage.SELECTED_CURRENCY_UNIT + "."; //"1.2658" + " " + ConstMessage.SELECTED_CURRENCY_UNIT + ".";

            else if (smsResponse.ApiStatus == ConstMessage.PIN_STATUS_INVALID_PIN_ID)
                smsResponse.MessageBody = "Invalid Pin code. Please try again.";

            else if (smsResponse.ApiStatus == ConstMessage.PIN_USED_STATUS_ID)
            {
                smsResponse.MessageBody = "The pin is already used. Please try a new Pin.";
                if (fromPinTopUp == 1)
                    smsResponse.MessageBody = "The pin is used, pin value: " + (string)obj["Data"]["PinValue"];
            }

            else
            {
                smsResponse.ApiStatus = ConstMessage.STATUS_FAILED_ID;
                smsResponse.MessageBody = "Recharge failed. Please try later.";
            }

            //if (obj["Data"]["OperationNumber"] != null)
            //    smsResponse.OperationNumber = (string)obj["Data"]["OperationNumber"];

            return smsResponse;
        }

    //    private SMSResponse TopUp(string UserName, int PhoneType, int ClientId, string Value, string ToUser, string CountryCode)
    //    {
            
    //        AdminRepository admin_repo = new AdminRepository();
    //        SMSResponse response = new SMSResponse();
    //        Destination destination;
    //        Service service;
    //        RatePlan ratePlan;

    //        response.UserName = UserName;
    //        user = admin_repo.GetUserByUserName(UserName);
    //        if (user != null)
    //        {
    //            destination = destination_repo.FindByCountryCode(CountryCode);

    //            service = service_repo.FindByDestinationAndValue(destination.Id, Convert.ToInt16(Value));
    //            if (service == null)
    //            {
    //                response.ApiStatus = ConstMessage.STATUS_FAILED_ID;
    ////to do =  the response message has to be dynamic. including the allowed amounts and currency unit.
    //                response.MessageBody = "Invalid amount. You can only Top Up 100, 300, 500, 1000 BDT. Please try again.";
    //                return response;
    //            }
    //            ratePlan = ratePlan_repo.FindByService(service.Id);

    //            decimal CreditLimit = user.CurrentBalance; //= User.CurrentBalance;
    //            decimal temp = CreditLimit + (CreditLimit * (ratePlan.ServiceCharge / 100));
    //            while (user.CurrentBalance < temp)
    //            {
    //                --CreditLimit;
    //                temp = CreditLimit + (CreditLimit * (ratePlan.ServiceCharge / 100));
    //            }
    //            decimal AmountInUsd = (decimal)service.value / ratePlan.ConvertionRate;
    //            decimal TotalMoney = AmountInUsd + ratePlan.ServiceCharge;

    //            if (TotalMoney > CreditLimit)
    //            {
    //                response.ApiStatus = ConstMessage.STATUS_FAILED_ID;
    //                response.MessageBody = "Insufficient balance";
    //                return response;
    //            }


    //            TopUp model = new TopUp();
    //            model.FromCurrencyId = destination.Id;
    //            model.RatePlanId = ratePlan.Id;
    //            model.ServiceId = service.Id;
    //            model.AmountInUSD = AmountInUsd;
    //            model.TotalAmount = TotalMoney;
    //            model.ProcessingFee = ratePlan.ServiceCharge;
    //            model.ToUser = "0" + ToUser;
    //            model.UserId = UserName;
    //            model.Value = Convert.ToDecimal(Value);
    //            model.Type = PhoneType;

    //            try
    //            {
    //                var result = userController.TopUp(model, ClientId, null);
    //                var serializer = new JavaScriptSerializer();
    //                var serialized = serializer.Serialize(result);
    //                var obj = JObject.Parse(serialized);
    //                response.ApiStatus = (int)obj["Data"]["result"];
    //                decimal UserCurrentBalance = (decimal)obj["Data"]["Balance"];
    //                UserCurrentBalance = Math.Round(UserCurrentBalance,6);
    //                if (response.ApiStatus == ConstMessage.STATUS_PENDING_ID)
    //                {
    //                    //response.ApiStatus = 1;
    //                    response.OperationNumber = (string)obj["Data"]["OperationNumber"];
    //                    response.MessageBody = "Top Up request accepted, confirmation SMS will be sent within 30 minutes. Your current balance is " + UserCurrentBalance + " " + ConstMessage.SELECTED_CURRENCY_UNIT + ".";
    //                }
    //                else if (response.ApiStatus == ConstMessage.STATUS_FAILED_ID)
    //                    response.MessageBody = "Top Up request failed. Please try again later.";
    //            }
    //            catch (Exception) { }
    //        }
    //        else
    //        {
    //            response.UserName = UserName;
    //            response.ApiStatus = ConstMessage.STATUS_FAILED_ID;
    //            response.MessageBody = "User not found, please register first";
    //        }
            

    //        return response;
    //    }

        private SMSResponse TopUp(int? Id,string UserName, int PhoneType, int ClientId, string PinCode, SMSToUser toUser)
        {
            SMSResponse response = new SMSResponse();
            response.UserName = UserName;
            response.Id = (Id ?? 0);
            //AdminRepository admin_repo = new AdminRepository();
            PinRepository pin_repo = new PinRepository();
            Pin pin = pin_repo.FindByPinCode(PinCode);
            Service service = service_repo.FindById(pin.ServiceId);
            RatePlan ratePlan = ratePlan_repo.FindByService(service.Id);

            //user = admin_repo.GetUserByUserName(UserName);
            

            TopUp model = new TopUp();
            model.OperatorPrefix = (toUser.OperatorPrefix ?? "");
            model.FromCurrencyId = service.DestinationId;//destination.Id;
            model.RatePlanId = ratePlan.Id;
            model.ServiceId = service.Id;
            if (ratePlan.MRPisPercentage)
            {
                model.AmountInUSD = (decimal)service.value + ((decimal)service.value * (ratePlan.MRP / 100));
            }else
                model.AmountInUSD = ratePlan.MRP;
            model.ProcessingFee = ratePlan.ServiceCharge;
            model.TotalAmount = model.AmountInUSD + model.ProcessingFee;
            //if (toUser.CountryCode == "880")
            //    model.ToUser = "0" + toUser.PhoneNumber;
            //else
            model.ToUser = toUser.PhoneNumber;
            model.UserId = UserName;
            model.Value = Convert.ToDecimal(service.value);
            model.Type = PhoneType;
            model.PinId = pin.Id;

            try
            {
                var result = userController.TopUp(model, ClientId, true, null);
                var serializer = new JavaScriptSerializer();
                var serialized = serializer.Serialize(result);
                var obj = JObject.Parse(serialized);
                response.ApiStatus = (int)obj["Data"]["result"];
                decimal UserCurrentBalance = (decimal)obj["Data"]["Balance"];
                UserCurrentBalance = Math.Round(UserCurrentBalance, 6);
                if (response.ApiStatus == ConstMessage.STATUS_PENDING_ID)
                {
                    //response.ApiStatus = 1;
                    response.OperationNumber = (string)obj["Data"]["OperationNumber"];
                    response.MessageBody = "Top Up request accepted, confirmation SMS will be sent within 30 minutes. Your current balance is " + UserCurrentBalance + " " + ConstMessage.SELECTED_CURRENCY_UNIT + ".";
                }
                else if (response.ApiStatus == ConstMessage.STATUS_COMPLETE_ID)
                {
                    response.OperationNumber = (string)obj["Data"]["OperationNumber"];
                    response.MessageBody = "Top Up request complete. Your Transaction Id - " + (string)obj["Data"]["TransactionId"];
                }
                else if (response.ApiStatus == ConstMessage.STATUS_FAILED_ID)
                    response.MessageBody = "Top Up request failed. Please try again later.";
            }
            catch (Exception) { }

            return response;
        }

        private SMSResponse MergeTwoResponses(SMSResponse response, SMSResponse response_additional)
        {
            SMSResponse mergedSms = new SMSResponse();
            mergedSms.ApiStatus = response.ApiStatus;
            mergedSms.OperationNumber = response.OperationNumber;  //response_additional.OperationNumber + ", " + 
            mergedSms.MessageBody = response_additional.MessageBody + ".\n " + response.MessageBody;

            if (response.UserName != null)
                mergedSms.UserName = response.UserName;
            else
                mergedSms.UserName = response_additional.UserName;
            return mergedSms;
        }

        /*
         * only implemented for bangladesh
         */

        public string GetApiResponseMessage(int ApiResponse, string referenceId)
        {
            switch (ApiResponse)
            {
                case 1:
                    return "Top up request is pending. Please wait.";
                case 20:
                case 25:
                    return "Top up request is processing. Please wait.";
                case 30:
                    return "Tou up is successfull (Transaction ID " + referenceId + ").";
                case 50:
                    return "Top up failed. Please try again later.";
                default:
                    return "Record not found";
            }
        }
        public List<SMSResponse> PrepareSmsResponseFromTransactionList(IEnumerable<Transaction> transactions)
        {
            List<SMSResponse> smsList = new List<SMSResponse>();
            foreach (var item in transactions)
            {
                SMSResponse smsResponse = new SMSResponse();
                try
                {
                    smsResponse.ApiStatus = (int)item.Status;
                    smsResponse.UserName = item.UserId;
                    smsResponse.OperationNumber = item.OperationNumber;
                    smsResponse.MessageBody = GetApiResponseMessage(smsResponse.ApiStatus, item.ReferenceId);
                    smsList.Add(smsResponse);
                }
                catch (Exception)
                {
                    //temp.MessageBody = "Record not found";
                }
        	}
            return smsList;
        }

	}
}