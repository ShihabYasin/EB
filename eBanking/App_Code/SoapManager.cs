using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Net;
using System.IO;
using eBanking.Models;
using eBanking.App_Code;
using System.Xml;

namespace eBanking.App_Code
{
    public class SoapManager
    {

        private string SOAPRequest(string SoapAction, string reqXml, string reqURL)
        {
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(reqURL);
            
            Req.ContentType = "text/xml; charset=\"utf-8\"";
            Req.Accept = "text/xml";
            Req.Headers.Add("SOAPAction:"+SoapAction);
            Req.Method = "POST";

            //Passes the SoapRequest String to the WebService
            using (Stream stm = Req.GetRequestStream())
            {
                using (StreamWriter stmw = new StreamWriter(stm))
                {
                    stmw.Write(reqXml); 
                }
            }

            string resposeMsg = "";

            try
            {
                //Gets the response
                WebResponse response = Req.GetResponse();
                //Writes the Response
                Stream responseStream = response.GetResponseStream();

                //return responseStream.ToString();
                StreamReader sr = new StreamReader(responseStream);
                resposeMsg = sr.ReadToEnd();
            }
            catch (Exception) { }
            //try
            //{
            //    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            //    var jsonObject = serializer.DeserializeObject(resposeMsg);
            //}
            //catch (Exception) { }
            
            return resposeMsg;

        }

        # region Swift SOAP Methods

        //private string SwiftGetProduct(RequestModel requestModel)
        //{ }

        public RequestResponse SwiftTopUP(string RefNo, string ProductCode,long Amount, string MobileNo)
        {
            RequestResponse RequestResponseModel = new RequestResponse();
            StringBuilder sBuilder = new StringBuilder();
            #region SOAP Envelope

            sBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sBuilder.Append(@"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">");
                sBuilder.Append("<soap:Body>");
                    sBuilder.Append(@"<TopUp xmlns=""http://swifttech.com.np"">");
                        sBuilder.Append("<res>");
                            sBuilder.Append("<USERNAME>"+ConstMessage.Swift_UserName+"</USERNAME>");
                            sBuilder.Append("<PASSWORD>"+ConstMessage.Swift_Password+"</PASSWORD>");
                            sBuilder.Append("<REFNO>" + RefNo + "</REFNO>");
                            sBuilder.Append("<PRODUCTCODE>" + ProductCode + "</PRODUCTCODE>");
                            sBuilder.Append("<AMOUNT>" + Amount + "</AMOUNT>");
                            sBuilder.Append("<MOBILENO>" + MobileNo + "</MOBILENO>");
                        sBuilder.Append("</res>");
                    sBuilder.Append("</TopUp>");
                sBuilder.Append("</soap:Body>");
            sBuilder.Append("</soap:Envelope>");

            #endregion

            #region SoapRequest and parse response
            try
            {
                string soapResponse= SOAPRequest(ConstMessage.Swift_SOAPAction_topUp,sBuilder.ToString(), "http://110.74.131.70:9005/api/ppws/SWS.asmx");
                
                //Parse XMl
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(soapResponse);
                XmlNodeList nodes = doc.GetElementsByTagName("TopUpResult");
                foreach (XmlNode node in nodes)
                {
                    RequestResponseModel.Status = int.Parse(node["CODE"].InnerText);
                    RequestResponseModel.RESPONSE_MSG = node["MESSAGE"].InnerText;
                    if (RequestResponseModel.Status == ConstMessage.NEPAL_SWIFT_SUCCESS)
                    {
                        RequestResponseModel.TransactionId= node["TRANNO"].InnerText;
                    }
                    RequestResponseModel.RefilSuccessDate = DateTime.Now;
                }
            }
            catch (Exception) { }
            #endregion 

            return RequestResponseModel;
        }

        //private string SwiftGetTxnReport(RequestModel requestModel)
        //{ }

        //private string SwiftGetTxnStatus(RequestModel requestModel)
        //{ }

        public decimal SwiftGetCurrentBalance(string RefNo)
        {
            decimal CurrentBalance = 0;
            StringBuilder sBuilder = new StringBuilder();

            #region SOAP Envelope            
            sBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>"); 
            sBuilder.Append(@"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">");
                sBuilder.Append("<soap:Body>");
                    sBuilder.Append(@"<GetCurrentBalance xmlns=""http://swifttech.com.np"">");
                        sBuilder.Append("<USERNAME>"+ConstMessage.Swift_UserName+"</USERNAME>");
                        sBuilder.Append("<PASSWORD>"+ConstMessage.Swift_Password+"</PASSWORD>");
                        sBuilder.Append("<REFNO>"+RefNo+"</REFNO>");
                    sBuilder.Append("</GetCurrentBalance>");
                sBuilder.Append("</soap:Body>");
            sBuilder.Append("</soap:Envelope>");
            #endregion
            
            #region SoapRequest and parse response
            string soapResponse = SOAPRequest(ConstMessage.Swift_SOAPAction_GetCurrentBalance, sBuilder.ToString(), "http://110.74.131.70:9005/api/ppws/SWS.asmx");
            //string soapResponse = @"<soap:Body xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/"><GetCurrentBalanceResponse xmlns=\"http://swifttech.com.np\"><GetCurrentBalanceResult><CODE>0</CODE><MESSAGE>Success</MESSAGE><CURRENTBALANCE>6000.0000</CURRENTBALANCE><TXNTODAYS>0</TXNTODAYS><AMOUNTTODAYS>0.0000</AMOUNTTODAYS></GetCurrentBalanceResult></GetCurrentBalanceResponse></soap:Body>";
            //Parse XMl
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(soapResponse);

            XmlNodeList nodes = doc.GetElementsByTagName("CURRENTBALANCE");//doc.DocumentElement.ChildNodes;//
            foreach (XmlNode node in nodes)
            {
                CurrentBalance = decimal.Parse(node.InnerText);
            }
            #endregion
            return CurrentBalance;
        }

        //private string SwiftChangePassword(RequestModel requestModel)
        //{ }

        public void SwiftGetProducts(string RefNo)
        {
            StringBuilder sBuilder = new StringBuilder();

            #region SOAP Envelope
            sBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sBuilder.Append(@"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">");
                sBuilder.Append("<soap:Body>");
                    sBuilder.Append(@"<GetProduct xmlns=""http://swifttech.com.np"">");
                        sBuilder.Append("<USERNAME>" + ConstMessage.Swift_UserName + "</USERNAME>");
                        sBuilder.Append("<PASSWORD>" + ConstMessage.Swift_Password + "</PASSWORD>");
                        sBuilder.Append("<REFNO>" + RefNo + "</REFNO>");
                        sBuilder.Append("</GetCurrentBalance>");
                sBuilder.Append("</soap:Body>");
            sBuilder.Append("</soap:Envelope>");
            #endregion

            string soapResponse = SOAPRequest(ConstMessage.Swift_SOAPAction_GetCurrentBalance, sBuilder.ToString(), "http://110.74.131.70:9005/api/ppws/SWS.asmx");
        }

        # endregion

    }                        
}