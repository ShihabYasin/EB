using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eBanking.Models;
using System.Web.Mvc;

namespace eBanking.App_Code
{
    public class ConstMessage
    {

        //success save
        public static String SUCCESS_SAVE = "SUCCESS: Data Saved Successfully.";
        //success Update
        public static String SUCCESS_UPDATE = "SUCCESS: Data Updated Successfully.";
        //Error Save
        public static String ERROR_SAVE = "ERROR: Error Saving Data.";
        //Error Update
        public static String ERROR_UPDATE = "ERROR: Error Updating Data.";
        //No Record Found
        public static String NO_RECORD_FOUND = "No Record found.";

        public static string CustomAuthErrorMessage = "You don't Have sufficient right to Access this!!";

        public static int NullReference = 0;

        public static int CretionFailed = 1;

        public static int Dupliacation = 2;

        public static int NotCallAjax = 3;

        public static string ForgotPassword = "Yes";
        public static int TransactionFromApi = 5;

        public const string SMSTransactionReportUsers = "8801713336506, 8801713336512, 8801973336464, 60164434164";
        
        public const string eBankingStaticApiKey = "ANbMjOArB7j9x6w7ifzoP2rSDZ897AByoY05cMJuvfr9GMRsDH0aAY2jowWqMSg";
        public const string eBankingStaticApiUsername = "8801790585222";//8801713336512
        public const string eBankingStaticApiUserPass = "passCode2323";
        public const int eBankingStaticApiClientId = 1001;
        public const string eBankingStaticUserIp = "119.81.196.235";


        public static decimal DistributorCommissionNotReturned = -1;
        

        public static int VENDOR_ID = 1;

        public static string SMS_FORMATS_HELP = "For Recharge -  REC<space>PIN_NUMBER\nFor TopUp-  TOP<space>AMOUNT<space>MOBILE_NO\nFor TopUp (PostPaid)- TOP<space>AMOUNT<space>MOBILE_NO<space>PO";
        public static string SMS_FORMATS_INVALID = "The request is invalid. Please try with proper SMS format.";

        public static long PIN_SERIAL_OFFSET = 10102015000;
        #region Pagination
        public static int ITEMS_PER_PAGE = 20;

        public static SelectList ItemsPerPageSelector = new SelectList(new[] 
                                        {
                                            new { Value= 10, Name = "10" },
                                            new { Value = 20, Name = "20" },
                                            new { Value = 50, Name = "50" },
                                            new { Value = 100, Name = "100" },
                                            new { Value = 500, Name = "500" },
                                        },
                                        "Value", "Name", 20);
        #endregion

        public static SelectList MonthSelector = new SelectList(new[] 
                                        {
                                            new { Value= 1, Name = "January" },
                                            new { Value= 2, Name = "February" },
                                            new { Value= 3, Name = "March" },
                                            new { Value= 4, Name = "April" },
                                            new { Value= 5, Name = "May" },
                                            new { Value= 6, Name = "June" },
                                            new { Value= 7, Name = "July" },
                                            new { Value= 8, Name = "August" },
                                            new { Value= 9, Name = "September" },
                                            new { Value= 10, Name = "October" },
                                            new { Value= 11, Name = "November" },
                                            new { Value= 12, Name = "December" }
                                        },
                                        "Value", "Name");
        public static SelectList YearSelector = new SelectList(new[] 
                                        {
                                            new { Value= "2015", Name = "2015" },
                                            new { Value= "2016", Name = "2016" }
                                        },
                                        "Value", "Name");

        #region PinCode Status
        public static int VOUCHER_PIN_LENGTH = 11;

        public static string PIN_BATCH_PREFIX = "PN";

        public static int PIN_STATUS_ERROR_ID= 50;
        public static string PIN_STATUS_ERROR_MSG = "Recharge failed, please try later";

        public static int PIN_UN_USED_ID = 101;
        public static string PIN_UN_USED_MSG = "NotUsed";

        public static int PIN_USED_STATUS_ID = 102;
        public static string PIN_USED_STATUS_ERROR_MSG = "Pin already use, please try a new pin";
        public static string PIN_USED_STATUS_MSG = "Used";

        public static bool PIN_ISACTIVE_STATUS_ID = true;

        public static int PIN_RECHARGED_ID = 3;
        public static string PIN_RECHARGED_MSG = "Recharged";

        public static string PIN_STATUS_INVALID_PIN_MSG = "Invalid Pin";
        public static string PIN_STATUS_INVALID_PIN_ERROR_MSG = "Invalid Pin, please try again";
        public static int PIN_STATUS_INVALID_PIN_ID = 105;

        public static int PIN_HISTORY_CREATE = 1;
        public static int PIN_HISTORY_ACTIVATE = 2;
        //public static int PIN_HISTORY_ASSIGNTO = 3;
        //public static int PIN_HISTORY_RETURN = 4;
        public static SelectList PinStatuses = new SelectList(new[] 
                                        {
                                            new { Value= "101", Name = "UnUsed" },
                                            new { Value= "102", Name = "Used" }
                                        },
                                        "Value", "Name");

        #endregion

        #region Currency Controller QueryStatus

        public static string FromIndex = "Index";

        public static string FromdetailsDelete = "DetailsDelete";

        #endregion

        #region RatePlan Status

        public static string RequestFrmRatePlan = "Request From RatePlan";
        #endregion

        #region Selected Value
       
        //Currency
        //public static int SELECTED_CURRENCY_ID = 5;
        public static string SELECTED_CURRENCY_UNIT = "USD";

        //DEstination
        public static int SELECTED_USD_DESTINATION_ID = 189;
        public static int SELECTED_BAN_DESTINATION_ID = 14;
        public static int SELECTED_NEP_DESTINATION_ID = 123;

        //Service
        public static int SELECTED_SERVICE_ID = 2;

        #endregion

        #region UserManager

        public static string RoleNotFound = "No Role found";

        public static int VendorMaxGet = 2;
        #endregion

        #region TRANSACTION_STATUS

        public static int STATUS_PENDING_ID = 1;
        public static string STATUS_PENDING_MSG = "Pending";
        //StatusPendingMsg
        public static int STATUS_PROCESSING_ID = 20;
        public static string STATUS_PROCESSING_MSG = "Processing";

        public static int STATUS_VENDOR_PROCESSING_ID = 25;
        public static string STATUS_VENDOR_PROCESSING_MSG = "Vendor Processing";

        public static int STATUS_COMPLETE_ID = 30;  //50;
        public static string STATUS_COMPLETE_MSG = "Complete";

        public static int STATUS_CANCELED_ID = 40;
        public static string STATUS_CANCELED_MSG = "Canceled";

        public static int STATUS_FAILED_ID= 50;
        public static string STATUS_FAILED_MSG = "Failed";

  

        public static int STATUS_MODELSTATE_ISVALID_FAILED = 60;
        public static string STATUS_MODELSTATE_ISVALID_MSG = "ModelState.Isvalid Failed";
            //"given model data is not valid,some required filed may empty.";
            
    

        #endregion


        #region Service_MoneyTransfer_Status

       
        public static int STATUS_MONEY_TRANSFER_SUCCESS_ID = 1;
        public static int STATUS_MONEY_TRANSFER_FAILED_ID = 0;

        #endregion

        #region Service_CreditTransfer_Status

        public static int STATUS_CREDIT_TRANSFER_FAILED_ID = 0;
        public static string STATUS_CREDIT_TRANSFER_FAILED_MSG = "Credit Transfer Failed !!Try again.";

        public static int STATUS_CREDIT_TRANSFER_SUCCESS_ID = 1;
        public static string STATUS_CREDIT_TRANSFER_SUCCESS_MSG = "Credit Transfer Successful";

        public static int STATUS_CREDIT_TRANSFER_USER_TOUSER_SAME_ID = 2;
        public static string STATUS_CREDIT_TRANSFER_USER_TOUSER_SAME_MSG = "You don't transfer credit in your own account";

        public static int STATUS_CREDIT_TRANSFER_EXCEED_BALANCE_ID = 3;
        public static string STATUS_CREDIT_TRANSFER_EXCEED_BALANCE_MSG = "Your credit must not exceed your available balance";
        #endregion

        #region Service_All_Status

        public static int STATUS_SERVICE_VOUCHER_RECHARGED_ID = 9;

        public static int STATUS_SERVICE_CREDIT_MANAGEMENT_ID = 11;
        public static string STATUS_SERVICE_CREDIT_MANAGE_MSG = "Credit Management";
        public static int STATUS_SERVICE_CREDIT_TRANSFER_ID = 12;
        public static string STATUS_SERVICE_CREDIT_TRANSFER_MSG = "Credit Transfer";
        public static int STATUS_SERVICE_EBANKING_CASH_IN_ID = 21; //20 TODO - CHANGE VALUE
        public static int STATUS_SERVICE_EBANKING_CASH_OUT_ID = 31;
        public static int STATUS_SERVICE_PIN_ASSIGN = 32;
        public static int STATUS_SERVICE_PIN_RETURN = 33;
        public static string STATUS_SERVICE_EBANKING_CASH_IN_MSG = "eBanking Cash In";
        public static int STATUS_SERVICE_EBANKING_PIN_ACTIVATION = 22; //TODO - CHANGE VALUE
        #endregion


        #region Role

        public static string ROLE_REGISTERED_NAME = "Registered";

        public static string CUSTOMER_SUPPORT_ROLE_ID = "3c4a1b62-a830-4da3-9f97-711fa79a8cea";

        public static string ROLE_NAME_ADMIN = "Admin";
        public static string ROLE_NAME_WHOLESELLER = "WholeSeller";
        public static string ROLE_NAME_RETAILER = "Retailer";

        #endregion

        #region Service

        public static int SERVICES_MONEYTRANSFER_ID = 1;
        public static string SERVICES_MONEYTRANSFER_MSG = "Money Transfer";

        public static int SERVICES_TOPUP_ID = 5;
        public static string SERVICES_TOPUP_MSG = "Top Up";

        public const int SERVICES_BKASH_PERSONAL = 2;
        public static int SERVICES_BKASH_PERSONAL_TYPE = 1;
        public const int SERVICES_BKASH_AGENT = 3;
        public static int SERVICES_BKASH_AGENT_TYPE = 2;

        public static int SERVICES_CREDIT_MANAGEMENT_ID = 11;

        public const int Services_topUp_Parent = 5;

        public const int Services_Vendor_Recharge = 30;
        public const int Services_Vendor_Balance_Return = 25;

        public const int Service_Voucher_Recharge = 9;

        public const int UserBalanceReturn = 24;
        public const int DistributorCommissionReturn = 26;

        public const int DistributorOpeartions = 34;
        public const int SERVICE_DISTRIBUTOR_CASHIN_ID= 35;
        public const int Services_Distributor_Disputes = 37;
        #endregion

        #region Distributor
        public static SelectList Distributors = new SelectList(new[] 
                                        {
                                            new { Value= "WholeSeller", Name = "WholeSale Distributor" },
                                            new { Value= "Retailer", Name = "Retail Distributor" }
                                        },
                                        "Value", "Name");
        #endregion

        #region Login_Register_Status

        public static int FROM_SERVER = 0;
        public static int API_FROM = 1;
        public static int FROM_SMS = 501;

        public static int STATUS_LOGIN_APK_VERSION_FAILED = 3;

        public static int STATUS_LOGIN_FAILED_ID = 0;
        public static string STATUS_LOGIN_FAILED_MSG = "Log In failed!!Try again.";

        public static int STATUS_LOGIN_SUCCESS_ID = 1;

        public static int STATUS_REGISTER_FAILED_ID = 0;

        public static int STATUS_REGISTER_ROLE_SUCCESS_ID = 1;

        public static int STATUS_REGISTER_SUCCESS_ID = 2;

        public static int STATUS_REGISTER_USERNAME_USED = 102;

               
        #endregion 

        #region Exception

        public static int EXCEPTION_STATUS_ID = 9999;
        #endregion

        #region User Error Status

        public static int STATUS_USER_NOT_FOUND_ID = 500;
        public static string STATUS_USER_NOT_FOUND_MSG = "User not found.Please Log In!";

        public static int STATUS_INTERNAL_ERROR_ID = 501;
        public static string STATUS_INTERNAL_ERROR_MSG = "Internal error !!";

        public static int STATUS_INVALID_USER_ID = 503;
        public static string STATUS_INVALID_USER_MSG = "Invalid User!!";
            
        
        #endregion

        #region Voucher Recharge Message

        public static string STATUS_RECHARGE_SUCCESS_MSG = "You are successfully recharged";
        #endregion
        
        #region LOG OFF STATUS

        public static int STATUS_LOGG_OFF_FAILED_ID = 0;
        public static int STATUS_LOGG_OFF_SUCCESS_ID = 1;

        #endregion        

        #region Change_Password_Status

        public static int STATUS_PASSWORD_CHANGE_FAILED_ID = 0;
        public static int STATUS_PASSWORD_CHANGE_SUCCESS_ID = 1;

        #endregion

        #region RegisteredUserDashboard

        public static decimal? TopUpPrePaid = 0;
        public static decimal? TopUpPostPaid = 0;
        public static decimal? MoneyTransferIn = 0;
        public static decimal? MoneyTransferOut = 0;
        

        #endregion

        #region Get_User_Balance

        public static int STATUS_GET_USER_BALANCE_FAILED_ID = 0;

        public static int STATUS_GET_USER_BALANCE_SUCCESS_ID = 1;

        public static int STATUS_SET_USER_BALANCE = -1;
        #endregion

        #region Transaction_Operation_Number

        public static string PREFIX_MONEY_TRANSFER = "MN";

        public static string PREFIX_TOP_UP = "TU";

        public static string PREFIX_RECHARGE = "RH";

        public static string PREFIX_DISTRIBUTOR_COMISSION = "DC";

        public static string PREFIX_CREDIT_TRANSFER = "CT";

        public static string PREFIX_EBANKING_CASH_IN = "CI";

        public static string PREFIX_EBANKING_PIN_ACTIVATE = "PA";

        public static string PREFIX_USER_BALANCE_RETURN = "UBR";

        public static string PREFIX_PIN_TOPUP = "PTU";
           

        #endregion

        #region Top Up Status

        public static string TOPUP_BATCH_PREFIX = "TP";

        public static string TOP_UP_SUCCESS_MSG = "Successfully Toped Up Amount ";

        public static string TOP_UP_FAILED_MSG = "Top up can't process Pls try latter."; //Top Up Failed!!


        #region Top Up Type Value

        public static int TOP_UP_PRE_PAID_VALUE = 1;
        public static string TOP_UP_PRE_PAID_MSG = "Pre Paid";

        public static int TOP_UP_POST_PAID_VALUE = 2;
        public static string TOP_UP_POST_PAID_MSG = "Post Paid";

        #endregion

        #endregion

        #region User_LockOut_Status

        public static int DEFAULT_ACCOUNT_LOCKOUT_TIME_SPAN_MINUTES=5;

        public static int MAX_FAILED_ACCESS_ATTEMPTS_BEFORE_LOCKOUT = 3;

        public static int LOCKOUT_AFTER_MULTIPLE_LOGINFAILED_IN_TIME = 10;

        #endregion

        #region Money_Transfer_Configuration
        public static bool MONEY_TRANSFER_TIMEOUT_CONFIG = false;
        #endregion
        public static string StaticUserName = "CSD1";

        #region GLOBAL VENDOR STATUS AND INFORMATION

        public const int Request_Type_Request = 1;
        public const int Request_Type_Response = 2;

        #region bdGW-AbrarTel

        //public static String SMSGatewayURL = "http://load.abrartel.com/m.abrartel/mobile/";
        public const string abrar_vendor_prefix = "AbrarTel";
        public static String SMSGatewayURL = "http://198.148.110.233/m.abrartel/mobile/";//changed on 19.March.2016:  198.148.110.233 || new 38.108.92.51  || previous 38.108.92.174

        public static String SMSGatewayUserId = "amartaka";  //tk2bdtk2bd    //amartaka
        public const int abrar_VendorId = 3;

        public static String SMSGatewayPwd = "@martak@";   //123456123456  //@martak@
        public static String SMSGatewayRequestPage = "flexi.jsp?";
        public static String SMSGatewayResponsePage = "response.jsp?";
        public static String SMSGatewayReq = "Request=TopUp";
        public static String SMSGatewayCountryCode = "&CountryCode=BD";

        public static String SEND_AIRTIME_REQUEST_SUCCESSFUL_RESPONSE_MSG = "RequestID";
        public static string SEND_AIRTIME_REQUEST_ERROR_RESPONSE_MSG = "Error";

        #endregion
        #region ezze
        public const string ezze_vendor_prefix = "ezze";
        public static string ezze_SMSGatewayURL = "http://ezzetopup.com/ezzeapi/";
        public static string ezze_Request_topUp = "request/flexiload?";
        public static string ezze_Request_bkash = "request/bkash?";
        public static string ezze_Request_status = "status?";
        public static int ezze_Bkash_Min_Amount = 500;
        public static int ezze_Bkash_Max_Amount = 20000;

        #region old
        public const int ezze_VendorId = 2;
        public static string ezze_UserName = "amartaka";
        public static string ezze_Password = "AmarTaka#$34";//amarTaKa#45
        public static string ezze_Pin = "238647";//23423
        public static string ezze_API_Key = "b73ea3b2874dced5e589d462a4a8087f";
        #endregion

        #region ezze_topup
        public const int ezze_topup_VendorId = 5;
        public static string ezze_topup_UserName = "amartaka";
        public static string ezze_topup_Password = "AmarTaka#$34";//amarTaKa#45
        public static string ezze_topup_Pin = "238647";//23423
        public static string ezze_topup_API_Key = "b73ea3b2874dced5e589d462a4a8087f";
        #endregion

        #region ezze_bkash
        public const int ezze_bkash_VendorId = 6;
        public static string ezze_bkash_UserName = "amartaka";
        public static string ezze_bkash_Password = "AmarTaka#$34";//amarTaKa#45
        public static string ezze_bkash_Pin = "238647";//23423
        public static string ezze_bkash_API_Key = "b73ea3b2874dced5e589d462a4a8087f";
        #endregion

        #endregion


        #region SwiftTopUp-Nepal
        public const string Swift_vendor_prefix = "swift";
        public const int Swift_VendorId = 4;
        public static string Swift_GatewayURL = "http://110.74.131.70:9005/api/ppws/SWS.asmx";
        public static string Swift_SOAPAction_topUp = "http://swifttech.com.np/TopUp";
        public static string Swift_SOAPAction_GetCurrentBalance = "http://swifttech.com.np/GetCurrentBalance";
        public static string Swift_SOAPAction_GetTxnStatus = "http://swifttech.com.np/GetTxnStatus";

        public static string Swift_UserName = "arrivo";
        public static string Swift_Password = "@rr1v070pUp#@!#00";

        #region nepal phone operator prefixes

        public static string NEPAL_SWIFT_NCELL = "NCELL";
        public static string NEPAL_SWIFT_PREPAID = "Prepaid";
        public static string NEPAL_SWIFT_POSTPAID = "Postpaid";
        #endregion

        #region nepal swift statuscodes
        public static int NEPAL_SWIFT_SUCCESS = 0;
        public static int NEPAL_SWIFT_ERROR = 1;
        public static int NEPAL_SWIFT_SUSPICIOUS_TRANSACTION = 12;
        public static int NEPAL_SWIFT_TECHNICAL_ERROR = 999;
        #endregion

        #endregion
        #region Terminator 

        public static string terminator_SMSGatewayURL = "http://192.168.1.113:8080/api/Terminators?";
        public const int terminator_VendorId = 10;
        public static string terminator_UserName = "amartaka";
        public static string terminator_Password = "AmarTaka#$34";
        public static string terminator_Pin = "238647";
        public static string terminator_API_Key = "b73ea3b2874dced5e589d462a4a8087f";
        public const string terminator_vendor_prefix = "term";

        #endregion

        #region Sending Flexi Request: BD Failed Response

        //ERROR_CODE_1
        public static String SEND_AIRTIME_REQUEST_ERROR_CODE_1_MSG = "enough required information not supplied";
        public static int SEND_AIRTIME_REQUEST_ERROR_CODE_1_VALUE = 1;
        //ERROR_CODE_2
        public static String SEND_AIRTIME_REQUEST_ERROR_CODE_2_MSG = "unauthorized user";
        public static int SEND_AIRTIME_REQUEST_ERROR_CODE_2_VALUE = 2;
        //ERROR_CODE_3
        public static String SEND_AIRTIME_REQUEST_ERROR_CODE_3_MSG = "mobile no format error";
        public static int SEND_AIRTIME_REQUEST_ERROR_CODE_3_VALUE = 3;
        //ERROR_CODE_4
        public static String SEND_AIRTIME_REQUEST_ERROR_CODE_4_MSG = "minimum or maximum recharge amount not matched";
        public static int SEND_AIRTIME_REQUEST_ERROR_CODE_4_VALUE = 4;
        //ERROR_CODE_5
        public static String SEND_AIRTIME_REQUEST_ERROR_CODE_5_MSG = "this recharge amount not available for you";
        public static int SEND_AIRTIME_REQUEST_ERROR_CODE_5_VALUE = 5;
        //ERROR_CODE_6
        public static String SEND_AIRTIME_REQUEST_ERROR_CODE_6_MSG = "already sent a request for the number in last 15 minutes";
        public static int SEND_AIRTIME_REQUEST_ERROR_CODE_6_VALUE = 6;
        //ERROR_CODE_7
        public static String SEND_AIRTIME_REQUEST_ERROR_CODE_7_MSG = "system error";
        public static int SEND_AIRTIME_REQUEST_ERROR_CODE_7_VALUE = 7;

        #endregion

        #region Sending Flexi Request for Response:BD Response

        public static String SEND_AIRTIME_REQUEST_FOR_RESPONSE_1_MSG = "Pending";
        public static int SEND_AIRTIME_REQUEST_FOR_RESPONSE_1_VALUE = 1;

        public static String SEND_AIRTIME_REQUEST_FOR_RESPONSE_2_MSG = "Successful";
        public static int SEND_AIRTIME_REQUEST_FOR_RESPONSE_2_VALUE = 2;

        public static String SEND_AIRTIME_REQUEST_FOR_RESPONSE_3_MSG = "Rejected";
        public static int SEND_AIRTIME_REQUEST_FOR_RESPONSE_3_VALUE = 3;

        public static string GET_AIRTIME_TRANSACTION_ID_TAG_NAME = "TransactionID";

        #endregion

        #region SMS_SENDER_VENDOR_INFO_GLOBAL

        public static int SMS_SELECTED_VENDOR = 2;

        public static int SMS_VENDOR_ID_SINFINI = 1;
        public static string SMS_SENDER_GateWay_Uri = "http://global.sinfini.com/api/v3/index.php?";
        public static string SMS_SENDER_Method_ApiKey = "method=sms&api_key=Ade51f88b3f3b7c573fcb14fbb51d7481";

        public static string SMS_SENDER_To = "&to=";
        public static string SMS_SENDER_Sender = "&sender=AmarTaka";
        public static string SMS_SENDER_Message = "&message=";
        public static string SMS_SENDER_SUCCESS_STATUS = "OK";


        public static int SMS_VENDOR_ID_EZZE = 2;

        public static string SMS_SENDER_GATEWAY_URI_EZZE = "http://ezzetopup.com/smsApi?";
        public static string SMS_SENDER_User_EZZE = "user=amartaka";
        public static string SMS_SENDER_Method_ApiKey_EZZE = "&key=b73ea3b2874dced5e589d462a4a8087f";
        public static string SMS_SENDER_To_EZZE = "&mobile=";
        public static string SMS_SENDER_SUCCESS_STATUS_EZZE = "SUBMIT_SUCCESS";

        #region Message

        public static string SMS_SENDER_SendingMessage = "Your Password  ";
        public static string SMS_SENDER_SendingMessage_For_APK = "You are successfully registered.A short notification will send you as SMS containing your Password.You must login using this password for first time.Then you should change your password.";
        public static string Registration_successfulMessage = "Thank you for registering and Welcome to our service. Your account is activated. Please save the password to access your account. You can change it after first Login.";
    
        #endregion

        #endregion

        #endregion

        #region handle APK Version
        public static string APK_VERSION = "1.1.2";
#endregion

        #region USER_ROLE_NAME

        public static string REGISTER_USER_ROLE_NAME = "Registered";
        public static string RESELLER_USER_ROLE_NAME ="Reseller";

        #endregion

        #region PinTopUP list

        public static IEnumerable<PinTopUp> pinTopUps = new List<PinTopUp>{                   // new values     old values
            new PinTopUp { CountryCode = "880", TopUpValue =  "100", PinValue =  "1.25"}, //  1.25           1.265823
            new PinTopUp { CountryCode = "880", TopUpValue =  "200", PinValue =  "2.5"}, //  2.5            2.531646
            new PinTopUp { CountryCode = "880", TopUpValue =  "300", PinValue =  "3.75"}, //  3.75           3.797469
            new PinTopUp { CountryCode = "880", TopUpValue =  "500", PinValue =  "6.25"}, //  6.25           6.329114
            new PinTopUp { CountryCode = "880", TopUpValue = "1000", PinValue = "12.5"}  // 12.5           12.658228
        };

        #endregion

        #region phone number length

        public static int TOPUP_BD_MAX_LENGTH = 13;
        public static int TOPUP_BD_MIN_LENGTH = 11;
        public static int TOPUP_NP_MAX_LENGTH = 13;
        public static int TOPUP_NP_MIN_LENGTH = 10;

        #endregion

        

        #region Sequencer constants
        public static int PrefixType_Services = 0;
        public static int PrefixType_Pins = 1;
        #endregion


        public static int Balance_Add = 1;
        public static int Balance_Deduct = 2;

        public static string url_smsreport="http://1.32.57.26:8080/api/SmsModels?ClientId=501";
        public static string url_smsSysChk = "http://1.32.57.26:8080/api/SmsModels/CheckSystemStatus?UserName=csduser&Password=eBanking123654&ClientId=501";
        public static string url_statusupdate = "http://1.32.57.26:8080/api/SmsModels";//1.32.57.26:8080  //http://192.168.1.113:8080/api/SmsModels/ResponseSms?ClientId=501

    }
}