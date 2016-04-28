using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.Interface;
using eBanking.Models;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Web;
//using Newtonsoft.Json.Linq;

//using System.IO;
//using System.Linq;

//using System.Web.Script.Serialization;
namespace eBanking.Controllers
{
    public class RequestApiController : ApiController
    {
        [HttpGet]
        [ActionName("ServiceRequest")]
        [AllowAnonymous]
        public HttpResponseMessage ServiceRequest(string request, string country, string user, string apikey, string phonenumber, int? phonetype, int amount)//string pass, 
        {
            bool requestIsValid = false;

            string IpAddress = HttpContext.Current.Request.UserHostAddress;
            requestIsValid = ValidateRequest(user, apikey,IpAddress); 
            SMSResponse response = new SMSResponse();
            response.UserName = user;

            if (requestIsValid)
            {
                response.ClientId = ConstMessage.eBankingStaticApiClientId;

                if (request.ToLower() == "topup")
                {
                    TopUp model = new TopUp();
                    model = PrepareTopupModel(user, phonenumber, country, amount, phonetype);
                    if (model != null && !string.IsNullOrEmpty(model.UserId))
                    {
                        try
                        {
                            UserController userController = new UserController();
                            var result = userController.TopUp(model, ConstMessage.eBankingStaticApiClientId, false, user);
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
                                response.MessageBody = "Top Up request accepted. Your current balance is " + UserCurrentBalance + " " + ConstMessage.SELECTED_CURRENCY_UNIT + ".";
                            }
                            else if (response.ApiStatus == ConstMessage.STATUS_COMPLETE_ID)
                            {
                                response.OperationNumber = (string)obj["Data"]["OperationNumber"];
                                response.MessageBody = "Top Up request complete. Your Transaction Id - " + (string)obj["Data"]["TransactionId"];
                            }
                            else if (response.ApiStatus == ConstMessage.STATUS_FAILED_ID)
                            {
                                response.MessageBody = "Top Up request failed. Please try again later. Your current balance is " + UserCurrentBalance + " " + ConstMessage.SELECTED_CURRENCY_UNIT + ".";
                                if (obj["Data"]["ErrorMessage"] != null)
                                    response.MessageBody = obj["Data"]["ErrorMessage"].ToString() + " Your current balance is " + UserCurrentBalance + " " + ConstMessage.SELECTED_CURRENCY_UNIT + ".";
                            }
                        }
                        catch (Exception) { }
                    }
                    else
                        response.MessageBody = "Invalid data. Please try again.";
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { response });
            }
            response.MessageBody = "Access Denied";
            response.ApiStatus = ConstMessage.STATUS_FAILED_ID;
            return Request.CreateResponse(HttpStatusCode.OK, new { response }); //var.LocalBalance
        }
        [HttpGet]
        [ActionName("ServiceResponse")]
        [AllowAnonymous]
        public HttpResponseMessage ServiceResponse(string user, string apikey, string operationnumber)
        {
            bool requestIsValid = false;
            string IpAddress = HttpContext.Current.Request.UserHostAddress;
            requestIsValid = ValidateRequest(user, apikey, IpAddress);
            List<string> operationNumberList = new List<string>();
            SMSResponse response = new SMSResponse();
            response.UserName = user;
            response.OperationNumber = operationnumber;
            if (requestIsValid)
            {
                operationNumberList.Add(operationnumber);
                TransactionRepository tran_repo = new TransactionRepository();
                var tranList = tran_repo.GetTransactionStatusByOrderNumberList(operationNumberList);
                SmsController smscontroller = new SmsController();
                var smsList = smscontroller.PrepareSmsResponseFromTransactionList(tranList);

                response.ApiStatus = smsList[0].ApiStatus;
                response.MessageBody = smscontroller.GetApiResponseMessage(smsList[0].ApiStatus, tranList.FirstOrDefault().ReferenceId);
                return Request.CreateResponse(HttpStatusCode.OK, new { response });

            }
            response.MessageBody = "Access Denied";
            response.ApiStatus = ConstMessage.STATUS_FAILED_ID;
            return Request.CreateResponse(HttpStatusCode.OK, new { response });
        }
        private bool ValidateRequest(string user, string apikey)//string pass, 
        {
            if (!string.IsNullOrEmpty(user) || !string.IsNullOrEmpty(apikey))//!string.IsNullOrEmpty(pass) || 
            {
                if (apikey == ConstMessage.eBankingStaticApiKey && user == ConstMessage.eBankingStaticApiUsername)//&& pass == ConstMessage.eBankingStaticApiUserPass
                {
                    //implement login
                    return true;
                }
            }
            return false;
        }

        private bool ValidateRequest(string user, string apikey,string ip)//string pass, 
        {
            if (!string.IsNullOrEmpty(user) || !string.IsNullOrEmpty(apikey) || !string.IsNullOrEmpty(ip))//!string.IsNullOrEmpty(pass) || 
            {
                if (apikey == ConstMessage.eBankingStaticApiKey && user == ConstMessage.eBankingStaticApiUsername && ip==ConstMessage.eBankingStaticUserIp)//&& pass == ConstMessage.eBankingStaticApiUserPass
                {
                    //implement login
                    return true;
                }
            }
            return false;
        }
        private TopUp PrepareTopupModel(string username, string phonenumber, string country, int amount, int? phonetype)
        {

            TopUp model = new TopUp();
            try
            {
                UserController userController = new UserController();
                Service selectedService = GetTopUpService(country, amount);
                IRatePlanRepository rateplan_repo = new RateplanRepository();
                RatePlan ratePlan = rateplan_repo.FindByService(selectedService.Id);
                var toUser = userController.ValidateToUser(phonenumber, selectedService.Id);


                model.OperatorPrefix = (toUser.OperatorPrefix ?? "");
                model.FromCurrencyId = selectedService.DestinationId;//destination.Id;
                model.RatePlanId = ratePlan.Id;
                model.ServiceId = selectedService.Id;
                if (ratePlan.MRPisPercentage)
                {
                    model.AmountInUSD = (decimal)selectedService.value + ((decimal)selectedService.value * (ratePlan.MRP / 100));
                }
                else
                    model.AmountInUSD = ratePlan.MRP;
                model.ProcessingFee = ratePlan.ServiceCharge;
                model.TotalAmount = model.AmountInUSD + model.ProcessingFee;
                //if (toUser.CountryCode == "880")
                //    model.ToUser = "0" + toUser.PhoneNumber;
                //else
                model.ToUser = toUser.PhoneNumber;
                model.UserId = username;
                model.Value = Convert.ToDecimal(selectedService.value);
                if (toUser.IsPostPaid)
                    model.Type = ConstMessage.TOP_UP_POST_PAID_VALUE;
                else if (phonetype.HasValue)
                    model.Type = (int)phonetype;
                //model.PinId = pin.Id;

            }
            catch (Exception)
            {
                return null;
            }

            return model;
        }
        private Service GetTopUpService(string country, int amount)
        {
            Service service = new Service();
            IServiceRepository service_repo = new ServiceRepository();
            service = service_repo.GetAllQueryable().Where(x => x.IsActive == true && x.ParentId == ConstMessage.SERVICES_TOPUP_ID && x.DestinationId == ConstMessage.SELECTED_NEP_DESTINATION_ID && x.value == amount).SingleOrDefault();
            //service_repo.FindByDestinationAndValue(ConstMessage.SELECTED_NEP_DESTINATION_ID,amount);
            return service;
        }


        //// GET api/<controller>
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/<controller>/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/<controller>
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/<controller>/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/<controller>/5
        //public void Delete(int id)
        //{
        //}
    }
}