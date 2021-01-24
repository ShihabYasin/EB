using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.Interface;
using eBanking.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace eBanking.Controllers
{
    [CustomAuth]
    public class RatePlanController : Controller
    {
        #region Repository, Context And Variable Declaration
        private IRatePlanRepository rateplan_repo;
        private ICurrencyRepository currency_Repo;
        private IServiceRepository service_repo;
        private IDestinationRepository destination_repo;

        private IEnumerable<RatePlanViewModel> rateplanViewList;
        private RatePlanViewModel rateplanViewModel;

        private UserMenuGenarator user_menu = new UserMenuGenarator();

        public RatePlanController()
        {
            rateplan_repo = new RateplanRepository();
            currency_Repo = new CurrencyRepository();
            service_repo = new ServiceRepository();
            destination_repo = new DestinationRepository();
        }
        #endregion
        // GET: /RatePlan/


        /*---------------------------------------------------------
        *  Display rate plan including rateplan service name and rateplan currencyName
        *  so we filter Rateplan with service and currency by getting currency name and service name
        *  from rateplan currency Id and rateplan service Id
        *  eBankingTask.RatePlanIndex() method filter this
        *---------------------------------------------------------*/
        public ActionResult Index()
        {
            try
            {
                rateplanViewList = rateplan_repo.GetAlltoRatePlanVM();

                if (rateplanViewList != null)
                {                   
                    return View(rateplanViewList);
                }
            }
            catch (Exception)
            {

            }
            finally 
            { 
            
            }

            return View("Error");
        }


        /*---------------------------------------------------------
        *  to display a rate paln in details get this rateplan from
        *  rateplan_repo.FindById(id) and then filter to prepare a rateplan viewmodel
        *  view model contains rateplan name ,service name,its currency name and etc
        *  from eBankingTask.SingleRatePlan()
        *  and return this if error return Error page 
        *---------------------------------------------------------*/

        // GET: /RatePlan/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {                
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var rateplan =rateplan_repo.FindById(id);

            if (rateplan != null)
            {
                try
                {
                    rateplanViewModel = rateplan_repo.GetRatePlanVMById(id);
                   
                    if (rateplanViewModel !=null)
                    {
                        return View(rateplanViewModel);
                    }                        
                }
                catch (Exception)
                { 
                
                }
            }

            return View("Error");
           
           
        }

        /*---------------------------------------------------------
        *  this is create rate paln httpget method
        *  hear  ViewData["FromCurrency"], ViewData["ToCurrency"], ViewData["ChildService"]
        *  is used for dropdrownlist
        *---------------------------------------------------------*/

        // GET: /RatePlan/Create
        public ActionResult Create()
        {
            try
            {
                var ToCurrency = currency_Repo.ActiveCurrency(ConstMessage.RequestFrmRatePlan);
                var service = new SelectList(service_repo.GetRatePlanServices(), "Id", "Name");

               
                ViewData["ChildService"] = service;
                ViewData["FromCurrency"] = new SelectList(ToCurrency.Where(x => x.Id != ConstMessage.SELECTED_USD_DESTINATION_ID).ToList(), "Id", "DestinationName");
                ViewData["ToCurrency"] = new SelectList(ToCurrency.Where(x => x.Id == ConstMessage.SELECTED_USD_DESTINATION_ID).ToList(), "Id", "DestinationName", ConstMessage.SELECTED_USD_DESTINATION_ID);

                var relations = service_repo.GetAll(null);
                ViewBag.MyService = JsonConvert.SerializeObject(relations);

                return View();
            }
            catch (Exception) 
            { 
                //ViewData can not assigned Null value.Exp message:Value cannot be null.Parameter name: source
            }

            return View("Error");
        }

        /*---------------------------------------------------------
        *  this is ratepaln create httppost method
        *  first check posted rateplan data validity 
        *  then InActive previous rate plan of this service if this service has any rateplane in previous rateplan_repo.InActiveRatePlan
        *  then add new rateplan of given services using  rateplan_repo.Add(rateplan)
        *  if add success then it returns true else false  
        *---------------------------------------------------------*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RatePlan rateplan)
        {
            rateplan.CreatedDate = DateTime.Now;
            rateplan.CreatedBy = HttpContext.User.Identity.Name;

            if (ModelState.IsValid)
            {
                //In Active previous Rate Plan of this service
                rateplan_repo.InActiveRatePlan(rateplan.ServiceId);


                if (rateplan.Cost <= rateplan.MRP)

                {
                    bool add = rateplan_repo.Add(rateplan);

                    if (add == true)
                        return RedirectToAction("Index");
                    else
                        ModelState.AddModelError("", "Can not create currency.Please Try again.");

                }
                else
                    ModelState.AddModelError("", "Coast can not be greater than MRP");
                
            }


            var service = new SelectList(service_repo.GetAll(true).Where(x => x.ParentId != 0).Select(x => x).ToList(), "Id", "Name", ConstMessage.SELECTED_SERVICE_ID);
            var ToCurrency = currency_Repo.ActiveCurrency(ConstMessage.RequestFrmRatePlan);
            
            ViewData["ChildService"] = service;
            ViewData["FromCurrency"] = new SelectList(ToCurrency.Where(x => x.Id != ConstMessage.SELECTED_USD_DESTINATION_ID).ToList(), "Id", "DestinationName");
            ViewData["ToCurrency"] = new SelectList(ToCurrency.Where(x => x.Id == ConstMessage.SELECTED_USD_DESTINATION_ID).ToList(), "Id", "DestinationName", ConstMessage.SELECTED_USD_DESTINATION_ID);               
                                      
            ViewBag.MyService = JsonConvert.SerializeObject(service);
            return View(rateplan);
        }



        /*---------------------------------------------------------
        *  display edit form to edit a rateplan
        *  ViewData["FromCurrency"] ,   ViewData["ChildService"],ViewData["ToCurrency"] fro dropdownlist
        *  
        *---------------------------------------------------------*/
        // GET: /RatePlan/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    RatePlan rateplan =rateplan_repo.FindById(id);
            
        //    if (rateplan != null)
        //    {
        //        try
        //        {
                    
        //            var service = new SelectList(service_repo.GetRatePlanServices(), "Id", "Name",rateplan.ServiceId);
        //            var ToCurrency = currency_Repo.ActiveCurrency(ConstMessage.RequestFrmRatePlan);

        //            ViewData["ChildService"] = service;
        //            ViewData["FromCurrency"] = new SelectList(ToCurrency.Where(x => x.Id != ConstMessage.SELECTED_USD_DESTINATION_ID).ToList(), "Id", "DestinationName");
        //            ViewData["ToCurrency"] = new SelectList(ToCurrency.Where(x => x.Id == ConstMessage.SELECTED_USD_DESTINATION_ID).ToList(), "Id", "DestinationName", ConstMessage.SELECTED_USD_DESTINATION_ID);

        //            ViewBag.MyService = JsonConvert.SerializeObject(service);
        //            return View(rateplan);
        //        }
        //        catch(Exception){}
        //    }

        //    return HttpNotFound();
       
        //}


        /*---------------------------------------------------------
        *  edit post method edit a rateplan entity using rateplan_repo.Edit(rateplan)
        *---------------------------------------------------------*/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(RatePlan rateplan)
        //{
        //    rateplan.CreatedDate = DateTime.Now;
        //    rateplan.CreatedBy = HttpContext.User.Identity.Name;

        //    if (ModelState.IsValid)
        //    {
        //        bool edit = rateplan_repo.Edit(rateplan);
              
        //        if(edit==true)
        //            return RedirectToAction("Index");
        //        else
                   
        //            ModelState.AddModelError("", "Edit Failed.Please Try again.");
        //    }

        //    try
        //    {
        //        var ToCurrency = currency_Repo.ActiveCurrency(ConstMessage.RequestFrmRatePlan);

        //        ViewData["FromCurrency"] = new SelectList(ToCurrency.ToList(), "Id", "DestinationName", rateplan.FromCurrencyId);

        //        ViewData["ChildService"] = new SelectList(service_repo.GetAll(true).Where(x => x.ParentId != 0).Select(x => x).ToList(), "Id", "Name", rateplan.ServiceId);

        //        ViewData["ToCurrency"] = new SelectList(ToCurrency.Where(x => x.Id != rateplan.FromCurrencyId).ToList(), "Id", "DestinationName", rateplan.ToCurrencyId);

        //        return View(rateplan);
        //    }
        //    catch (Exception) { }

        //    return HttpNotFound();
        //}


        /*---------------------------------------------------------
        *  to delete a rateplan display this ratepaln to the user to ensure
        *  that is the user wants to delete this rateplan
        *  when user wants to delete this just click delete button  DeleteConfirmed(int id)
        *  method is fired 
        *---------------------------------------------------------*/
        // GET: /RatePlan/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    RatePlan rateplan = rateplan_repo.FindById(id);
           
        //    if (rateplan != null)
        //    {
        //        try
        //        {
        //            rateplanViewModel = rateplan_repo.GetRatePlanVMById(id);

        //            if (rateplanViewModel != null)
        //                return View(rateplanViewModel);

        //        }
        //        catch (Exception) { }

        //    }

        //    return HttpNotFound();
         
        //}




        /*---------------------------------------------------------
        *  to delete a rate plan pass rate plan Id to  rateplan_repo.Delete(id) method
        *  when delete success this method return deleted ratplan entity
        *  if delete failed it returns null 
        *  when rateplan is unsuccess then filter this rateplan of given id add error with delete failed message and return to the user
        *  that this rateplan is not deleted 
        *---------------------------------------------------------*/
        // POST: /RatePlan/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    RatePlan rateplan = rateplan_repo.Delete(id);

        //    if (rateplan != null)
        //        return RedirectToAction("Index");
        //    else
        //        ModelState.AddModelError("", "Delete Failed.Please Try again.");

        //    try
        //    {
        //        rateplanViewModel = rateplan_repo.GetRatePlanVMById(id);

        //        if (rateplanViewModel != null)
        //            return View(rateplanViewModel);

        //    }
        //    catch (Exception) { }

        //    return View("Error");
        //}

    }
}
