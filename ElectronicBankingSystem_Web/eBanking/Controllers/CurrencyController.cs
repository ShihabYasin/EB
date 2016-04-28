using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.Interface;
using eBanking.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;

namespace eBanking.Controllers
{
    [CustomAuth]
    public class CurrencyController : Controller
    {
        private ICurrencyRepository currency_repo = new CurrencyRepository();
        private IEnumerable<CurrencyViewModel> currencyViewModelList;
        private UserMenuGenarator user_menu = new UserMenuGenarator();
        
        public CurrencyController()
        {
            this.currency_repo = new CurrencyRepository();
        }
        // GET: /Currency/

        //display active currency with country name by filtering from eBankingTask.detailsCurrency

        public ActionResult Index()
        {
           
            try
            {
                currencyViewModelList = currency_repo.ActiveCurrency(null);
                if(currencyViewModelList!=null)
                {
                    //ViewBag.Menu = user_menu.GenarateMenu(HttpContext.User.Identity.Name);
                    return View(currencyViewModelList);
                }
                    
            }
            catch (Exception) 
            {
                currencyViewModelList = null;
            }

            return HttpNotFound();
        }


        //display a single active currency with country name by filtering from eBankingTask.SingleCurrency
        // GET: /Currency/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Currency currency = currency_repo.FindById(id);

            if (currency != null)
            {

                try
                {
                    CurrencyViewModel detailsCurrency = currency_repo.GetCurrencyVMById(id);

                    if (detailsCurrency != null)
                    {
                        //ViewBag.Menu = user_menu.GenarateMenu(HttpContext.User.Identity.Name);
                        return View(detailsCurrency);
                    }
                        

                }
                catch (Exception)
                {

                }
            }
            return HttpNotFound();
        }

        // GET: /Currency/Create
        public ActionResult Create()
        {
            ViewData["destination"] = currency_repo.GetDestinationSelectList();
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Currency currency)
        {
            if (ModelState.IsValid)
            {
                Currency IsExists = currency_repo.FindByName(currency.CurrencyName);
               

                if (IsExists != null)
                {
                    ModelState.AddModelError("", "Currency already exists.");                    
                }
                    
                else
                {
                    Currency IsDestinationExists = currency_repo.FindByDestinationID(currency.DestinationId);

                    if (IsDestinationExists != null)
                    {
                        ModelState.AddModelError("", "Destination already exists.");        
                    }
                    else
                    {
                        bool add = currency_repo.Add(currency);

                        if (add == true)
                            return RedirectToAction("Index");
                        else
                            ModelState.AddModelError("", "Can not create currency.Please Try again.");
                    }
                }
                    
            }

            ViewData["destination"] = currency_repo.GetDestinationSelectList();
            return View(currency);
        }

        // GET: /Currency/Edit/5
        public ActionResult Edit(int? id)
        {        
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Currency currency = currency_repo.FindById(id);

            ViewData["destination"] = currency_repo.GetDestinationSelectList();
       
            if (currency == null)
            {
                return HttpNotFound();
            }
            return View(currency);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Currency currency)
        {
            if (ModelState.IsValid)
            {
                Currency IsExists = currency_repo.FindByName(currency.CurrencyName);

                if (IsExists == null)
                {
                bool edit = currency_repo.Edit(currency);

                if (edit == true)
                    return RedirectToAction("Index");
                else
                    ModelState.AddModelError("", "Edit Failed.Please Try again.");
                }
                else
                    ModelState.AddModelError("", "Currency already exists.");

            }
            ViewBag.Destination = currency_repo.GetDestinationSelectList();
            
            return View(currency);
        }

        // GET: /Currency/Delete/5
        public ActionResult Delete(int? id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Currency currency = currency_repo.FindById(id);

            if (currency != null)
            {

                try
                {
                    CurrencyViewModel detailsCurrency = currency_repo.GetCurrencyVMById(id);
                    if (detailsCurrency != null)
                        return View(detailsCurrency);

                }
                catch (Exception)
                {

                }
            }
            return HttpNotFound();
            
        }

        // POST: /Currency/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)    // Currency currencyv
        {
              Currency currency = currency_repo.Delete(id);
                
                if (currency !=null)
                    return RedirectToAction("Index");
                else
                    ModelState.AddModelError("", "Delete Failed.Please Try again.");

                CurrencyViewModel detailsCurrency = currency_repo.GetCurrencyVMById(id);
                if (detailsCurrency == null)
                {
                    return HttpNotFound();    
                }
                
            return View(detailsCurrency);
            
        }
    }
}
