using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.Interface;
using eBanking.Models;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;


using PagedList;
using System.Collections.Generic;
using System.Web;
namespace eBanking.Controllers
{

      [CustomAuth]

    public class VendorsController : Controller
    {  
        private IVendorRepository vendor_repo;



        public VendorsController()
        {
            vendor_repo = new VendorRepository();
        }

        // GET: Vendors
        public ActionResult Index(int? Name, bool? IsActive, DateTime? FromDate, DateTime? ToDate)
        {
            IVendorRepository vendor_repo = new VendorRepository();
            var vendors = vendor_repo.Vendor_GetAll();
            ViewBag.VendorNameList = new SelectList(vendors,"Id" , "Name");
            ViewBag.IsActiveList = new SelectList(new[] {   new { Value= true, Name = "True" },
                                                            new { Value = false, Name = "False" }
                                                        }, "Value", "Name");
            IEnumerable<Vendor> vendorSearch = vendor_repo.Search(vendors, Name, IsActive, FromDate, ToDate);
            return View(vendorSearch);
            //return View(vendor_repo.Vendor_GetAll().ToList());
        }

        // GET: Vendors/Details/5
        public ActionResult Details(int? id)
        {
            IVendorRepository vendor_repo = new VendorRepository();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendor vendor = vendor_repo.FindVendorById(id); //db.Vendors.Find(id);
            if (vendor == null)
            {
                return HttpNotFound();
            }
            return View(vendor);
        }

        // GET: Vendors/Create
        public ActionResult Create()
        {
            ICurrencyRepository currency_repo = new CurrencyRepository();
            ViewBag.CurrencyList = new SelectList(currency_repo.GetAll(), "DestinationId", "ISO");
            return View();
        }

        // POST: Vendors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Vendor vendor)
        {
            IVendorRepository vendor_repo = new VendorRepository();
            ICurrencyRepository currency_repo = new CurrencyRepository();
            vendor.CreatedOn = DateTime.Now;
            vendor.CreatedBy = User.Identity.Name;

            if (ModelState.IsValid)
            {
                bool vendorExists = vendor_repo.FindVendorByName(vendor.Name);
                if(!vendorExists)
                {
                    bool createStatus = vendor_repo.Vendor_Create(vendor);

                    if (createStatus)
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Vendor already exists.");
                }
  
            }
            else
            {
                ModelState.AddModelError("", "Vendor Creation Failed");
            }

            
            var CurrencyList = new SelectList(currency_repo.GetAll(), "DestinationId", "ISO");

            ViewBag.CurrencyList = CurrencyList;
            return View(vendor);
        }

        // GET: Vendors/Edit/5
        public ActionResult Edit(int? id)
        {
            IVendorRepository vendor_repo = new VendorRepository();
            ICurrencyRepository currency_repo = new CurrencyRepository();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendor vendor = vendor_repo.FindVendorById(id);
            if (vendor == null)
            {
                return HttpNotFound();
            }
            var CurrencyList = new SelectList(currency_repo.GetAll(), "DestinationId", "ISO");
            ViewBag.CurrencyList = CurrencyList;
            return View(vendor);
        }

        // POST: Vendors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Vendor vendor)
        {
            vendor.CreatedOn = DateTime.Now;
            vendor.CreatedBy = User.Identity.Name;
           
            if (ModelState.IsValid)
            {
                //bool hasExist = vendor_repo.FindVendorByName(vendor.Name);

                //if(hasExist!=null)
                //{
                //    bool edit=Vendor.Vendor_Edit(vendor);
                //    if (edit == true)
                //        return RedirectToAction("Index");
                //    else
                //        ModelState.AddModelError("", "Edit Failed.Please Try again.");
                //}
                //else
                //    ModelState.AddModelError("", "Destination already exists.");
               // IVendorRepository vendor_repo = new VendorRepository();
                vendor_repo.Vendor_Edit(vendor);

                return RedirectToAction("Index");
            }
            return View(vendor);
        }

        //// GET: Vendors/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Vendor vendor = db.Vendors.Find(id);
        //    if (vendor == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(vendor);
        //}

        //// POST: Vendors/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    //Vendor vendor = db.Vendors.Find(id);
        //    //db.Vendors.Remove(vendor);
        //    //db.SaveChanges();

        //    Vendor veDelete = Vendor.Vendor_Delete(id);
        //    if (veDelete!=null)
        //     return RedirectToAction("Index");
        //        else
        //        ModelState.AddModelError("", "Delete Failed.Please Try again.");
        //    return View(Vendor);


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
