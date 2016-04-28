using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using eBanking.Models;
using System.Web.Script.Serialization;
using System.IO;
using Newtonsoft.Json.Linq;
using eBanking.App_Code;
using PagedList;

namespace eBanking.Controllers
{
    public class SMSTransactionReportsController : Controller
    {
        //private eBankingDbContext db = new eBankingDbContext();

        // GET: SMSTransactionReports
        [Authorize]
        [HttpGet]
        public ActionResult Index(int? page, string FromDate, string ToDate, string PinNumber, string UserName, int? itemsPerPage, int? FromAPI)
        {
            //int pageSize = (itemsPerPage ?? ConstMessage.ITEMS_PER_PAGE);
            int pageSize = (itemsPerPage ?? ConstMessage.ITEMS_PER_PAGE);
            ViewBag.ItemsPerPage = pageSize;
            ViewData["ItemsPerPageSelector"] = ConstMessage.ItemsPerPageSelector;
            var recieved = string.Empty;
            IEnumerable<SMSTransactionReport> sms = new List<SMSTransactionReport>();
            ViewBag.ItemsPerPage = pageSize;
            int pageNumber = (page ?? 1);

            string url_SmsSearch = ConstMessage.url_smsreport + "&fromDate=" + FromDate + "&toDate=" + ToDate + "&pin=" + PinNumber + "&userName=" + UserName;
            var httpRequest = (HttpWebRequest)WebRequest.Create(url_SmsSearch);
            httpRequest.Accept = "Application/json";
            httpRequest.Method = "GET";

            try
            {
                var response = (HttpWebResponse)httpRequest.GetResponse();
                if (response != null)
                {
                    var jsonSerialiser = new JavaScriptSerializer() { MaxJsonLength = 86753090 };
                    var reader = new StreamReader(response.GetResponseStream());
                    recieved = reader.ReadToEnd();
                    recieved = jsonSerialiser.Deserialize<string>(recieved);
                    JArray jArray = JArray.Parse(recieved);
                    sms = jsonSerialiser.Deserialize<List<SMSTransactionReport>>(recieved.ToString());
                }
            }
            catch (Exception)
            {

            }
            var result = sms.ToPagedList(pageNumber, pageSize);
            if (FromAPI == ConstMessage.API_FROM)
                return Json(new { smsResponses = result, totalPages = result.PageCount }, JsonRequestBehavior.AllowGet);
            return View(result);
  
        }
        [Authorize]
        [HttpGet]
        public ActionResult SMSClient()
        {
            string status = string.Empty;
            var httpRequest = (HttpWebRequest)WebRequest.Create(ConstMessage.url_smsSysChk);
            httpRequest.Method = "GET";
            try
            {
                var response = (HttpWebResponse)httpRequest.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var reader = new StreamReader(response.GetResponseStream());
                    status = reader.ReadToEnd();
                    status = status.Replace("\"", "");
                    if (status.Contains("ok"))
                    {
                        status = "System ok";
                    }
                    else
                    {
                        status = "System Down";
                    }
                }


            }
            catch (Exception)
            {
                status = "Failed to check";
            }
            ViewBag.Status = status;
            return View();
        }
        //[HttpPost]
        //public ActionResult Index(int? page,DateTime? FromDate, DateTime? ToDate,string PinNumber, string UserName)
        //{
        //    var recieved = string.Empty;
        //    IEnumerable<SMSTransactionReport> sms = new List<SMSTransactionReport>();
        //    int pageSize = ConstMessage.ITEMS_PER_PAGE;     
        //    ViewBag.ItemsPerPage = pageSize;
        //    int pageNumber = (page ?? 1);

        //    string url_SmsSearch = ConstMessage.url_smsreport+"&fromDate=" + FromDate + "&toDate=" + ToDate + "&pin=" + PinNumber + "&userName='" + UserName + "'";

        //    var httpRequest = (HttpWebRequest)WebRequest.Create(url_SmsSearch);
        //    httpRequest.Accept = "Application/json";
        //    httpRequest.Method = "GET";

        //    try
        //    {
        //        var response = (HttpWebResponse)httpRequest.GetResponse();
        //        if (response != null)
        //        {
        //            var jsonSerialiser = new JavaScriptSerializer();
        //            var reader = new StreamReader(response.GetResponseStream());
        //            recieved = reader.ReadToEnd();
        //            recieved = jsonSerialiser.Deserialize<string>(recieved);
        //            JArray jArray = JArray.Parse(recieved);
        //            sms = jsonSerialiser.Deserialize<List<SMSTransactionReport>>(recieved.ToString());
        //        }
        //    }
        //    catch (Exception)
        //    {

        //    }

        //    return View(sms.ToPagedList(pageNumber, pageSize));
        //}

        // GET: SMSTransactionReports/Details/5
        //public ActionResult Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    SMSTransactionReport sMSTransactionReport = db.SMSTransactionReports.Find(id);
        //    if (sMSTransactionReport == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(sMSTransactionReport);
        //}

        //// GET: SMSTransactionReports/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: SMSTransactionReports/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "Id,UserName,ReqMessage,ReqTime,MessageBody,ResTime,OperationNumber,ApiStatus")] SMSTransactionReport sMSTransactionReport)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.SMSTransactionReports.Add(sMSTransactionReport);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(sMSTransactionReport);
        //}

        //// GET: SMSTransactionReports/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    SMSTransactionReport sMSTransactionReport = db.SMSTransactionReports.Find(id);
        //    if (sMSTransactionReport == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(sMSTransactionReport);
        //}

        //// POST: SMSTransactionReports/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,UserName,ReqMessage,ReqTime,MessageBody,ResTime,OperationNumber,ApiStatus")] SMSTransactionReport sMSTransactionReport)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(sMSTransactionReport).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(sMSTransactionReport);
        //}

        //// GET: SMSTransactionReports/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    SMSTransactionReport sMSTransactionReport = db.SMSTransactionReports.Find(id);
        //    if (sMSTransactionReport == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(sMSTransactionReport);
        //}

        //// POST: SMSTransactionReports/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    SMSTransactionReport sMSTransactionReport = db.SMSTransactionReports.Find(id);
        //    db.SMSTransactionReports.Remove(sMSTransactionReport);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
