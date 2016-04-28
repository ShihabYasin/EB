using eBanking.Abstract;
using eBanking.Controllers;
using eBanking.Interface;
using eBanking.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Script.Serialization;


namespace eBanking.App_Code
{
    public class SMSDR_Helper
    {
        #region global variables
        //private eBankingDbContext db;
        //private eBankingDbContext db=new eBankingDbContext();
        private Variable _variable = new Variable();
        //private IEnumerable<SMSDR> tDs;
        #endregion

        #region vendor dependent functions
        public string SendVendorHttpRequest(int VendorId, int? RequestType, int? service, string PhoneNumber, int? Amount, string ReferenceId, int? TopUpType)//, string countryCode, 
        {//string VendorPrefix, 
            #region Variables

            string postData = "";//= new StringBuilder();
            string responseMessage = string.Empty;
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader respStreamReader = null;
            //int TopUpType;
            ServiceRepository service_repo = new ServiceRepository();
            int serviceParent = 0;
            if (service != null)
                serviceParent = service_repo.FindById(service).ParentId;
            #endregion

            try
            {
                //Prepare POST data  
                switch (VendorId)
                {
                    case ConstMessage.abrar_VendorId:
                        {
                            #region abrar tel
                            TopUpType = (TopUpType ?? 1);
                            postData += ConstMessage.SMSGatewayURL;
                            if (RequestType == ConstMessage.Request_Type_Request)
                            {
                                switch (serviceParent)
                                {
                                    case ConstMessage.Services_topUp_Parent:
                                        {
                                            postData += ConstMessage.SMSGatewayRequestPage;
                                            postData += ConstMessage.SMSGatewayReq;
                                            postData += ConstMessage.SMSGatewayCountryCode;
                                        }
                                        break;
                                }
                            }
                            else if (RequestType == ConstMessage.Request_Type_Response)
                            {
                                postData += ConstMessage.SMSGatewayResponsePage;
                                postData += "RequestID=" + ReferenceId;
                            }


                            postData += "&UserId=" + ConstMessage.SMSGatewayUserId;
                            postData += "&Password=" + ConstMessage.SMSGatewayPwd;
                            if (!string.IsNullOrEmpty(PhoneNumber))
                                postData += "&MobileNumber=" + PhoneNumber;
                            if (Amount > 0)
                                postData += "&Amount=" + Amount;
                            postData += "&NumberType=" + TopUpType;
                            #endregion
                        }
                        break;
                    case ConstMessage.ezze_VendorId:
                    case ConstMessage.ezze_topup_VendorId:
                    case ConstMessage.ezze_bkash_VendorId:
                        {
                            #region ezzeTopUp
                            //TopUpType = 1;                              //initialize this variable at least once for every vendor 
                            postData += ConstMessage.ezze_SMSGatewayURL;

                            if (RequestType == ConstMessage.Request_Type_Request)
                            {
                                switch (service)
                                {
                                    case ConstMessage.SERVICES_BKASH_PERSONAL:
                                    case ConstMessage.SERVICES_BKASH_AGENT:
                                        {
                                            postData += ConstMessage.ezze_Request_bkash;
                                            if (Amount < ConstMessage.ezze_Bkash_Min_Amount || Amount > ConstMessage.ezze_Bkash_Max_Amount)
                                            {
                                                responseMessage = "Amount must between " + ConstMessage.ezze_Bkash_Min_Amount + " and " + ConstMessage.ezze_Bkash_Max_Amount + " BDT.";

                                                throw new System.ArgumentOutOfRangeException(responseMessage);
                                            }
                                            if (service == ConstMessage.SERVICES_BKASH_AGENT)
                                                TopUpType = ConstMessage.SERVICES_BKASH_AGENT_TYPE;
                                            break;
                                        }
                                    default:
                                        postData += ConstMessage.ezze_Request_topUp;
                                        break;
                                }
                                postData += "number=" + PhoneNumber;
                                postData += "&amount=" + Amount;
                                postData += "&type=" + TopUpType;
                            }
                            else if (RequestType == ConstMessage.Request_Type_Response)
                            {
                                postData += ConstMessage.ezze_Request_status;
                            }
                            postData += "&id=" + ReferenceId;
                            if (VendorId == ConstMessage.ezze_bkash_VendorId)
                            {
                                postData += "&user=" + ConstMessage.ezze_bkash_UserName;
                                postData += "&key=" + ConstMessage.ezze_bkash_API_Key;
                            }
                            else if (VendorId == ConstMessage.ezze_topup_VendorId)
                            {
                                postData += "&user=" + ConstMessage.ezze_topup_UserName;
                                postData += "&key=" + ConstMessage.ezze_topup_API_Key;
                            }
                            else
                            {
                                postData += "&user=" + ConstMessage.ezze_UserName;
                                postData += "&key=" + ConstMessage.ezze_API_Key;
                            }

                            #endregion
                        }
                        break;
                        //23/3/2016 developed by mojahid
                    case ConstMessage.terminator_VendorId :
                        {
                            postData += ConstMessage.terminator_SMSGatewayURL;

                             if (RequestType == ConstMessage.Request_Type_Request)
                             {
                                 postData += "&UserName=" + ConstMessage.terminator_UserName;
                                 postData += "&ApiKey=" + ConstMessage.terminator_API_Key;
                                 if (!string.IsNullOrEmpty(PhoneNumber))
                                     postData += "&MobileNumber=" + PhoneNumber;
                                 if (Amount > 0)
                                 postData += "&NumberType=" + TopUpType;
                                 postData += "&Amount=" + Amount;
                                 postData += "&Operator=" + 1;
                                 postData += "&RequestID=" + ReferenceId;
                             }
                            else if(RequestType==ConstMessage.Request_Type_Response)
                             {
                                 postData += "&UserName=" + ConstMessage.terminator_UserName;
                                 postData += "&ApiKey=" + ConstMessage.terminator_API_Key;
                             }
                        }

                        break;
                }

                request = (HttpWebRequest)WebRequest.Create(postData);
                response = (HttpWebResponse)request.GetResponse();
                respStreamReader = new System.IO.StreamReader(response.GetResponseStream());
                responseMessage = respStreamReader.ReadToEnd();
                //responseMessage = "SUCCESS";

            }
            catch (Exception ex)
            {
                if (ex is ArgumentOutOfRangeException)
                    responseMessage = ex.Message;
                responseMessage = "FAILED";
            }
            finally
            {
                if (respStreamReader != null && response != null)
                {
                    respStreamReader.Close();
                    response.Close();
                }
            }

            return responseMessage;
        }

        //public string SendVendorSoapRequest()
        //{
        //    SoapClient framework1 = new SoapClient();
        //    return "";
        //}

        //Top Up Response Spilter
        //split response to get REQUEST_ID or other Info
        public RequestResponse SpiltResponse(string ResponseMessage, int VendorId, int? RequestType)//string[] 
        {
            RequestResponse requestResponse = new RequestResponse();
            
            try
            {
                //string value = ResponseMessage;
                requestResponse.RESPONSE_MSG = ResponseMessage;
                char[] delimiters = new char[] { '\n', '=', ',' };
                string[] parts = ResponseMessage.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);


                switch (VendorId)
                {
                    case ConstMessage.abrar_VendorId:
                        #region vendor - abrar
                        {
                            if (parts.Length > 1 && parts[0] == "Error")//ResponseMessage.Contains("Error")
                            {
                                requestResponse.Status = ConstMessage.STATUS_FAILED_ID;
                                requestResponse.RESPONSE_MSG += " -- " + GetErrorMessage( int.Parse(parts[1]) );
                                break;
                            }
                            else if (RequestType == ConstMessage.Request_Type_Request)
                            {
                                requestResponse.TransactionId = parts[1];
                                break;
                            }
                            

                            delimiters = new char[] { ':' };
                            string[] Status = parts[0].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            string[] TransactionID = parts[1].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                            //string[] Message = parts[2].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                            //get date from message
                            if (parts.Length > 5)
                            {
                                string transactionSuccessDate = parts[5].Substring(1, parts[5].Length - 1);
                                transactionSuccessDate += " " + parts[6].Substring(0, 8);
                                try
                                {
                                    requestResponse.RefilSuccessDate = DateTime.ParseExact(transactionSuccessDate, "yy/MM/dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (Exception) { }
                            }

                            if (!Status[1].Equals(ConstMessage.SEND_AIRTIME_REQUEST_FOR_RESPONSE_1_VALUE.ToString()))  //"1"
                            {
                                if (Status[1].Equals(ConstMessage.SEND_AIRTIME_REQUEST_FOR_RESPONSE_2_VALUE.ToString())) //"2"
                                {
                                    requestResponse.Status = ConstMessage.STATUS_COMPLETE_ID; //30
                                }
                                else if (Status[1].Equals(ConstMessage.SEND_AIRTIME_REQUEST_FOR_RESPONSE_3_VALUE.ToString()))  //"3"
                                {
                                    requestResponse.Status = ConstMessage.STATUS_FAILED_ID; //50
                                }

                            }
                            if (TransactionID.Length > 1 && TransactionID[0].Equals(ConstMessage.GET_AIRTIME_TRANSACTION_ID_TAG_NAME))
                                requestResponse.TransactionId = TransactionID[1].ToString();
                        }
                        #endregion
                        break;
                    case ConstMessage.ezze_VendorId:
                    case ConstMessage.ezze_topup_VendorId:
                    case ConstMessage.ezze_bkash_VendorId:
                        #region vendor - ezzetopup
                        {
                            ResponseMessage = ResponseMessage.ToLower();
                            delimiters = new char[] { ':', ' ' };
                            string[] Status = ResponseMessage.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                            if(Status[0].Contains("completed") || Status[0].Contains("success") )
                            {
                                requestResponse.Status = ConstMessage.STATUS_COMPLETE_ID;

                                if (Status.Length >= 4)
                                {
                                    if (Status[1].Contains("trxid"))
                                    requestResponse.TransactionId = Status[1] + ": " + Status[2] + ", " + Status[3] + ": " + Status[4];
                                    else
                                    requestResponse.TransactionId = "trxid" + ": " + Status[1] + ", " + Status[2] + ": " + Status[3];
                                }
                                else
                                    requestResponse.TransactionId = Status[1];
                                
                            }
                            else if (Status[0].Contains("cancelled")) // || Status[0].Contains("failed") 
                                requestResponse.Status = ConstMessage.STATUS_FAILED_ID;
                                
                        }
                        #endregion
                        break;
                }
                
            }
            catch (Exception) 
            {
                requestResponse.RESPONSE_MSG = "Splitting failed - " + ResponseMessage;
                requestResponse.Status = ConstMessage.STATUS_FAILED_ID;
            }
            

            return requestResponse;
        }

        #endregion
        public void UpdateStatusOfPending()
        {

            eBankingDbContext db = new eBankingDbContext();
            ITransactionRepository tran_repo = new TransactionRepository(db);
            IAdminRepository admin_repo = new AdminRepository(db);
            IRatePlanRepository rateplan_repo = new RateplanRepository(db);
            IVendorRepository vendor_repo = new VendorRepository(db);
            SmsController smscontroller = new SmsController();
            List<Transaction> tranlist=new List<Transaction>();
            
            //var pending = admin_repo.SmsdrGetAllQueryable().Where(s => s.REQUEST_ID != "" && s.STATUS == ConstMessage.STATUS_PENDING_ID).ToList();
            var pendingSmsdrList = GetAllPending();

            foreach(var pendingSmsdrItem in pendingSmsdrList)
            {
                string responseMessage = SendVendorHttpRequest(pendingSmsdrItem.VendorId,ConstMessage.Request_Type_Response,null,null,null,pendingSmsdrItem.REQUEST_ID,null);
                RequestResponse requestResponse = SpiltResponse(responseMessage,pendingSmsdrItem.VendorId,ConstMessage.Request_Type_Response);

                if (pendingSmsdrItem.STATUS != requestResponse.Status)
                {
                    try
                    {
                        //if (tran.Status == 30)
                        //    allSuccessful.Add(pendingSmsdrItem);

                        pendingSmsdrItem.STATUS = (int)requestResponse.Status;
                        pendingSmsdrItem.RESPONSE_MSG = responseMessage;
                        pendingSmsdrItem.REFILL_SUCCESS_TIME = (requestResponse.RefilSuccessDate ?? DateTime.Now);

                        Transaction tran = tran_repo.FindById(pendingSmsdrItem.TRANSACTION_ID);
                        tran.ReferenceId = requestResponse.TransactionId;
                        tran.Status = requestResponse.Status;
                        tran.Remarks = responseMessage;
                        if (pendingSmsdrItem.STATUS == ConstMessage.STATUS_FAILED_ID)
                        {
                            tran.DistributorCommission = 0;

                            DistributorRepository distributor_repo = new DistributorRepository();
                            var distributorServiceCharges = distributor_repo.DistributorTransaction_GetAllQueryable().Where(dt => dt.Remarks.Contains("OperationNumber:" + tran.OperationNumber)).ToList();
                            foreach (var dt in distributorServiceCharges)
                            {
                                DistributorTransaction returnChargeToUser = new DistributorTransaction();
                                returnChargeToUser.AmountOut = dt.AmountIn;
                                returnChargeToUser.AmountOutLocal = dt.AmountInLocal;
                                returnChargeToUser.ConvertToUsd = dt.ConvertToUsd;
                                returnChargeToUser.CreatedBy = "system";
                                returnChargeToUser.CreatedOn = DateTime.Now;
                                returnChargeToUser.CurrencyId = dt.CurrencyId;
                                returnChargeToUser.DistributorId = dt.DistributorId;
                                returnChargeToUser.Remarks = "User Transaction failed of Operation Number - " + tran.OperationNumber + ". Distributor balance adjusted.";
                                returnChargeToUser.ServiceId = ConstMessage.Services_Distributor_Disputes;

                                distributor_repo.DistributorTransaction_Add(returnChargeToUser);
                            }
                        }

                        

                        admin_repo.UpdatePendingRequest_ALL_Link(tran, pendingSmsdrItem); //smsdr 
                        //Successful transaction status post to AmarTakaSmsManager
                       
                        if(tran.Status==ConstMessage.STATUS_COMPLETE_ID && tran.ClientId==ConstMessage.FROM_SMS)
                        {

                            tranlist.Add(tran);
                        }
    
                        //deduct the pending request
                        int vendorRouteId = vendor_repo.VendorRoute_GetByServiceAndVendor(rateplan_repo.FindById(pendingSmsdrItem.RATE_PLANE_ID).ServiceId, pendingSmsdrItem.VendorId).Id;
                        vendor_repo.VendorRoute_UpdatePendingQty(vendorRouteId, false);

                    }

                    catch (Exception) 
                    {
                        continue;
                    }
                }

            }
            //if(allSuccessful != null)
            //    ReturnDistributorCommission(allSuccessful);
            List<SMSResponse> result = smscontroller.PrepareSmsResponseFromTransactionList(tranlist);
            if (result.Count>0)
            {
                PostResponsetoSmsClient(result);
            }
            

        }

        public void ReturnDistributorCommission()
        {
            try
            {
                ITransactionRepository tran_repo = new TransactionRepository();
                var temp_transactions = tran_repo.GetAllQueryable().Where(t => t.DistributorCommission == ConstMessage.DistributorCommissionNotReturned && t.Status == ConstMessage.STATUS_COMPLETE_ID).ToList();
                ReturnDistributorCommission(temp_transactions);
            }
            catch (Exception) { }
        }
        #region Helpers
        public IEnumerable<SMSDR> GetAllPending()//IEnumerable<SMSDR> pendingList
        {
            eBankingDbContext db = new eBankingDbContext();
            IAdminRepository admin_repo = new AdminRepository(db);
            var pendingList = admin_repo.SmsdrGetAllQueryable().Where(s => s.REQUEST_ID != "" && s.STATUS == ConstMessage.STATUS_PENDING_ID);
            ITransactionRepository tran_repo = new TransactionRepository(db);
            IQueryable<Transaction> transactionList = tran_repo.GetAllQueryable();
            //Transaction dummyTransaction = new Transaction();
            //dummyTransaction.ServiceId = 0;
            IEnumerable<SMSDR> pendingSmsdrList = null;
            try
            {
                pendingSmsdrList = (from sd in pendingList
                                    join tran in transactionList on sd.TRANSACTION_ID equals tran.Id into tranjoin
                                    from tranj in tranjoin.DefaultIfEmpty()
                                    //group sd by tranj.ServiceId into sdg
                                    select new //SMSDR
                                    {
                                        ServiceId = (tranj == null ? 0 : tranj.ServiceId),
                                        sd.Id,
                                        sd.GW_ID,
                                        sd.COST_AMOUNT,
                                        sd.TRANSACTION_ID,
                                        sd.STATUS,
                                        sd.REQUEST_ID,
                                        sd.CREATED_BY,
                                        sd.VendorId,
                                        sd.PHN_NO,
                                        sd.PHN_NO_TYPE,
                                        sd.RATE_PLANE_ID,
                                        sd.REFILL_SUCCESS_TIME,
                                        sd.REQUEST_TIME,
                                        sd.RESPONSE_MSG,
                                        sd.TRANS_AMOUNT

                                    })
                                    .AsEnumerable().Select(smsdr => new SMSDR
                                    {
                                        COST_AMOUNT = smsdr.COST_AMOUNT,
                                        CREATED_BY = smsdr.CREATED_BY,
                                        GW_ID = smsdr.GW_ID,
                                        Id = smsdr.Id,
                                        PHN_NO = smsdr.PHN_NO,
                                        PHN_NO_TYPE = smsdr.PHN_NO_TYPE,
                                        RATE_PLANE_ID = smsdr.RATE_PLANE_ID,
                                        REFILL_SUCCESS_TIME = smsdr.REFILL_SUCCESS_TIME,
                                        REQUEST_ID = smsdr.REQUEST_ID,
                                        REQUEST_TIME = smsdr.REQUEST_TIME,
                                        RESPONSE_MSG = smsdr.RESPONSE_MSG,
                                        ServiceId = smsdr.ServiceId,
                                        STATUS = smsdr.STATUS,
                                        TRANS_AMOUNT = smsdr.TRANS_AMOUNT,
                                        TRANSACTION_ID = smsdr.TRANSACTION_ID,
                                        VendorId = smsdr.VendorId,
                                    }).ToList();
            }
            catch (Exception) { }

            return pendingSmsdrList;
        }
        private void ReturnDistributorCommission(List<Transaction> pendingTransactions)
        {
            IAdminRepository admin_repo = new AdminRepository();
            ITransactionRepository tran_repo = new TransactionRepository();
            List<DistributorTransaction> distributorCommission = new List<DistributorTransaction>();
            DistributorRepository distributor_repo = new DistributorRepository();
            try
            {
                foreach (var item in pendingTransactions)
                {
                    List<int> distributors = new List<int>();
                    var distributorCode = admin_repo.GetUserByUserName(item.UserId).DistributorCode;
                    
                    //split distributor code to distributorId's
                    if (!string.IsNullOrEmpty(distributorCode))
                    {
                        distributors = distributorCode.Split('-').Select(int.Parse).ToList();
                    }
                    decimal previousReturn = 0;
                    for (int i = (distributors.Count - 1); i >= 0; i--)
                    {
                        int temp_distributorId = distributors[i];
                        var assignedRatePlan = distributor_repo.ADR_GetAllQueryable().Where(arp => arp.DistributorId == temp_distributorId && arp.IsActive == true);
                        var distRateplan = distributor_repo.DCRP_GetAllQueryable().Where(d => d.ServiceId == item.ServiceId && d.IsActive == true);
                         
                        var selectedRatePlan = (from assigned in assignedRatePlan
                                                join rates in distRateplan on assigned.RateplanId equals rates.DCRP_ID
                                                select rates).SingleOrDefault();
                        decimal cAmount = 0;
                        if (selectedRatePlan != null)
                        {

                            if (selectedRatePlan.IsPercentage)
                            {
                                cAmount = (decimal)(item.AmountOut * (selectedRatePlan.Commission / 100));
                            }
                            else
                            {
                                cAmount = selectedRatePlan.Commission;
                            }
                            var temp_swap = cAmount;
                            cAmount -= previousReturn;
                            previousReturn = temp_swap;


                            var currentDistributorTran = distributorCommission.Where(dt => dt.DistributorId == temp_distributorId).FirstOrDefault();


                            if (currentDistributorTran == null)
                            {
                                DistributorTransaction dTran = new DistributorTransaction();
                                dTran.AmountIn += cAmount;
                                dTran.AmountInLocal += cAmount;
                                dTran.AmountOut = 0;
                                dTran.ConvertToUsd = 1;
                                dTran.CreatedBy = "system";
                                dTran.CreatedOn = DateTime.Now;
                                dTran.CurrencyId = ConstMessage.SELECTED_USD_DESTINATION_ID;
                                dTran.DistributorId = temp_distributorId;
                                dTran.ServiceId = ConstMessage.DistributorCommissionReturn;
                                dTran.Remarks = "From Transaction IDs - " + item.Id.ToString();
                                dTran.HasDetails = true;

                                distributor_repo.DistributorTransaction_Add(dTran);
                                distributorCommission.Add(dTran);

                                DistributorTransactionDetail dTranDetails = new DistributorTransactionDetail();
                                dTranDetails.UserName = item.UserId;
                                dTranDetails.UserTransactionNo = item.Id;
                                dTranDetails.DistributorCommission = cAmount;
                                dTranDetails.DistributorCommissionRateId = selectedRatePlan.DCRP_ID;
                                dTranDetails.DTId = dTran.DTId;

                                distributor_repo.DistributorTransactionDetails_Add(dTranDetails);

                            }
                            else
                            {
                                decimal? previousAmountIn = currentDistributorTran.AmountIn;
                                currentDistributorTran.AmountIn += cAmount;
                                currentDistributorTran.AmountInLocal = currentDistributorTran.AmountIn;
                                currentDistributorTran.Remarks += ", " + item.Id.ToString();


                                DistributorTransactionDetail dTranDetails = new DistributorTransactionDetail();
                                dTranDetails.UserName = item.UserId;
                                dTranDetails.UserTransactionNo = item.Id;
                                dTranDetails.DistributorCommission = cAmount;
                                dTranDetails.DistributorCommissionRateId = selectedRatePlan.DCRP_ID;
                                dTranDetails.DTId = currentDistributorTran.DTId;

                                distributor_repo.DistributorTransaction_Update(currentDistributorTran, previousAmountIn, null);
                                distributor_repo.DistributorTransactionDetails_Add(dTranDetails);
                            }

                            tran_repo.DistributorCommisionReturned(item.Id, previousReturn);
                        }


                    }

                   
                }

            }
            catch (Exception ex) {
            
            }


        }

        public void PostResponsetoSmsClient(List<SMSResponse> ResponseSmsList)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(ConstMessage.url_statusupdate);
            httpRequest.ContentType = "Application/json";
            httpRequest.Method = "POST";
            try
            {
                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(ResponseSmsList);
                    

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }

            }
            catch (Exception)
            {

            }
        }
        
        #endregion


        public string GetErrorMessage(int ErrorCode)
        {
            if (ErrorCode == ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_1_VALUE)
                return ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_1_MSG;

            else if (ErrorCode == ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_2_VALUE)
                return ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_2_MSG;

            else if (ErrorCode == ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_3_VALUE)
                return ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_3_MSG;

            else if (ErrorCode == ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_4_VALUE)
                return ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_4_MSG;

            else if (ErrorCode == ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_5_VALUE)
                return ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_5_MSG;

            else if (ErrorCode == ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_6_VALUE)
                return ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_1_MSG;

            else if (ErrorCode == ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_7_VALUE)
                return ConstMessage.SEND_AIRTIME_REQUEST_ERROR_CODE_7_MSG;
            else
                return "Error !!";
        }

        
    }

    
}