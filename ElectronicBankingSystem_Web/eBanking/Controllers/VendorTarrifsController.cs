using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using eBanking.Models;
using eBanking.Interface;
using eBanking.Abstract;
using eBanking.App_Code;

namespace eBanking.Controllers
{
    [CustomAuth]
    public class VendorTarrifsController : Controller
    {
        //private eBankingDbContext db;
        //private IVendorTarrifRepository vendorTarrif_repo;
        //private IVendorRepository vendor_repo = new VendorRepository();
        //private IServiceRepository service_repo = new ServiceRepository();

       public VendorTarrifsController()
        {
            //vendorTarrif_repo = new VendorTarrifRepository();
            
            //this.db = new eBankingDbContext();
        }

        // GET: VendorTarrifs
        public ActionResult Index()
        {
            IVendorRepository vendor_repo = new VendorRepository();
            return View(vendor_repo.VendorTarrif_GetAll().ToList());
        }

        // GET: VendorTarrifs/Details/5
        public ActionResult Details(int? id)
        {
            IVendorRepository vendor_repo = new VendorRepository();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VendorTarrif vendorTarrif = vendor_repo.FindVendorTarrifById(id);
            if (vendorTarrif == null)
            {
                return HttpNotFound();
            }
            return View(vendorTarrif);
        }

        // GET: VendorTarrifs/Create
        public ActionResult Create()
        {
            IVendorRepository vendor_repo = new VendorRepository();
            IServiceRepository service_repo = new ServiceRepository();
            var VendorList = new SelectList(vendor_repo.Vendor_GetAll(), "Id", "Name");
            ViewBag.VendorList = VendorList;

            var ServiceList = new SelectList(service_repo.GetAllQueryable(), "Id", "Name");
            ViewBag.ServiceList = ServiceList;

            return View();
        }

        // POST: VendorTarrifs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VendorTarrif vendorTarrif)
        {
            IVendorRepository vendor_repo = new VendorRepository();
            IServiceRepository service_repo = new ServiceRepository();

            vendorTarrif.CreatedOn = DateTime.Now;
            vendorTarrif.CreatedBy = User.Identity.Name;
            var VendorList = new SelectList(vendor_repo.Vendor_GetAll(), "Id", "Name");
            ViewBag.VendorList = VendorList;


            if (vendorTarrif.Cost == 0)
                vendorTarrif.IsPercentage = true;
            if (ModelState.IsValid)
            {

                vendor_repo.VendorTarrif_DeactivatePrevious(vendorTarrif.ServiceId, vendorTarrif.VendorId);

                bool CreateStatus = vendor_repo.VendorTarrif_Create(vendorTarrif);
              
                if(CreateStatus)
                {
                    return RedirectToAction("Index");
                }
            
            }
            var ServiceList = new SelectList(service_repo.GetAllQueryable(), "Id", "Name");
            ViewBag.ServiceList = ServiceList;
            return View(vendorTarrif);
        }

        // GET: VendorTarrifs/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    IVendorRepository vendor_repo = new VendorRepository();
        //    IServiceRepository service_repo = new ServiceRepository();

        //    var VendorList = new SelectList(vendor_repo.Vendor_GetAll(), "Id", "Name");
        //    ViewBag.VendorList = VendorList;

        //    var ServiceList = new SelectList(service_repo.GetAllQueryable(), "Id", "Name");
        //    ViewBag.ServiceList = ServiceList;
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    VendorTarrif vendorTarrif = vendor_repo.FindVendorTarrifById(id);
        //    if (vendorTarrif == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(vendorTarrif);
        //}

        //// POST: VendorTarrifs/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(VendorTarrif vendorTarrif)
        //{
        //    IVendorRepository vendor_repo = new VendorRepository();
        //    IServiceRepository service_repo = new ServiceRepository();

        //    vendorTarrif.CreatedOn = DateTime.Now;
           
        //    if (ModelState.IsValid)
        //    {
        //        vendor_repo.VendorTarrif_Edit(vendorTarrif);
        //        //db.Entry(vendorTarrif).State = EntityState.Modified;
        //        //db.SaveChanges();

        //        return RedirectToAction("Index");
                      
        //    }
        //    return View(vendorTarrif);
        //}

        // GET: VendorTarrifs/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    IVendorRepository vendor_repo = new VendorRepository();
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    VendorTarrif vendorTarrif = vendor_repo.FindVendorTarrifById(id);
        //    if (vendorTarrif == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(vendorTarrif);
        //}

        //// POST: VendorTarrifs/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    IVendorRepository vendor_repo = new VendorRepository();
        //    vendor_repo.VendorTarrif_Delete(id);

        //    //VendorTarrif vendorTarrif = vendor_repo.FindVendorTarrifById(id);
        //    //db.VendorTarrifs.Remove(vendorTarrif);
        //    //db.SaveChanges();
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
