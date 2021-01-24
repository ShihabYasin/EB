using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using eBanking.Models;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using eBanking.App_Code;

namespace eBanking.Controllers
{
    public class AjaxController : Controller
    {

        private eBankingDbContext db;

        private Variable _var;
        public AjaxController()
        {
            db = new eBankingDbContext();
            _var = new Variable();
        }

        public JsonResult GetDestinationId(string ServiceId)
        {   
            _var.Flag = Convert.ToInt32(ServiceId);
           List<Service> ser = db.Services.Where(x=>x.Id==_var.Flag).Select(x=>x).ToList();

            return Json(ser, JsonRequestBehavior.AllowGet);
        
        }
        public JsonResult getRatePlan(string fromCurrencyId, string serviceId)
        {

            int FromCurrencyId = Convert.ToInt32(fromCurrencyId);
            int ServiceId = Convert.ToInt32(serviceId);

            List<RatePlan> ratePlans = db.RatePlans.Where(x => x.FromCurrencyId == FromCurrencyId && x.ServiceId == ServiceId && x.ToCurrencyId ==ConstMessage.SELECTED_USD_DESTINATION_ID).ToList(); ;


            return Json(ratePlans, JsonRequestBehavior.AllowGet);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public ActionResult getJson()
        {
            //one way
            var jsondata = Json();

            JavaScriptSerializer serializer = new JavaScriptSerializer();
           var a= (serializer.Serialize(jsondata));

                   
            var obj = JObject.Parse(a);

           
          //  var url = (string)obj["Data"]["data"]; //data is tag number
            
            //2nd whay
            var Name = (string)obj["Data"]["_stu"]["Name"];

            int age = (int)obj["Data"]["_stu"]["Age"];

            return View();
        }



        public ActionResult Json()
        {

            List<Stu> _stu = new List<Stu>
            {
              new Stu{ Name="Rak",Age=23},
              new Stu{Name="Nasir",Age=23}
            };
            return Json(new { _stu},JsonRequestBehavior.AllowGet);
        }

        public class Stu
        {
            public string Name { get; set; }

            public int Age { get; set; }
        }
     
	}
}