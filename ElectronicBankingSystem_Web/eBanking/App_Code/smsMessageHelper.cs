using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eBanking.App_Code;
using System.Text;
using System.Net;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;

namespace eBanking.App_Code
{
    public class smsMessageHelper
    {
        //Send SMS to a specific user
        //while registered 

        private Variable _variable = new Variable();

        public string SendMessage(string user_number, string DisplayMessage, int smsVendorId)
        {
            _variable.postData =new StringBuilder();
            try
            {

                if (smsVendorId == ConstMessage.SMS_VENDOR_ID_SINFINI)
                {
                    #region old sms vendor
                    _variable.postData.Append(ConstMessage.SMS_SENDER_GateWay_Uri);
                    _variable.postData.Append(ConstMessage.SMS_SENDER_Method_ApiKey);

                    _variable.postData.Append(ConstMessage.SMS_SENDER_To + user_number);
                    _variable.postData.Append(ConstMessage.SMS_SENDER_Sender);
                    _variable.postData.Append(ConstMessage.SMS_SENDER_Message + DisplayMessage);
                    #endregion
                }
                else if (smsVendorId == ConstMessage.SMS_VENDOR_ID_EZZE)
                {
                    #region sms vendor ezze
                    //http://ezzetopup.com/smsApi?user=!LOGIN!&key=!PASS!&sender=!FROM!&mobile=!TO!&message=!TEXT!
                    _variable.postData.Append(ConstMessage.SMS_SENDER_GATEWAY_URI_EZZE);
                    _variable.postData.Append(ConstMessage.SMS_SENDER_User_EZZE);
                    _variable.postData.Append(ConstMessage.SMS_SENDER_Method_ApiKey_EZZE);
                    _variable.postData.Append(ConstMessage.SMS_SENDER_Sender);
                    _variable.postData.Append(ConstMessage.SMS_SENDER_To_EZZE + user_number);
                    _variable.postData.Append(ConstMessage.SMS_SENDER_Message + DisplayMessage);
                    #endregion
                }

                 // Prepare web request c
                _variable.request = (HttpWebRequest)WebRequest.Create(_variable.postData.ToString());

                    // Prepare web response
                _variable.response = (HttpWebResponse)_variable.request.GetResponse();

                    // Send the request and get a response 
                _variable.respStreamReader = new System.IO.StreamReader(_variable.response.GetResponseStream());

                    // Read the response 
                _variable.responseMessage = _variable.respStreamReader.ReadToEnd();

                //parese  _variable.responseMessage 
                _variable.responseMessage = _variable.responseMessage.Replace(@"\","");

                //JavaScriptSerializer serializer = new JavaScriptSerializer();
                //var temp = (serializer.Serialize(_variable.responseMessage));

                var obj = JObject.Parse(_variable.responseMessage);
                _variable.Message = (string)obj["status"];

            }
            catch (Exception)
            {
            }
            finally
            {
                _variable.respStreamReader.Close();
                _variable.response.Close();
            }

            return _variable.Message;
                    
        }

       
    }
}