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
//using PagedList.Mvc;
using PagedList;

namespace eBanking.Controllers
{
    [CustomAuth]
    public class VendorTransactionsController : Controller
    {
        //private eBankingDbContext db;
        //private IVendorTransactionRepository vendortransaction_repo;
        ////private IVendorTarrifRepository vendortarrif_repo;
        //private VendorTransaction vendortransaction;
        //private IVendorRepository vendor_repo;
        //private IServiceRepository service_repo;

        public VendorTransactionsController()
        {
            //this.db = new eBankingDbContext();
            //vendortransaction_repo = new VendorTransactionRepository();
            ////vendortarrif_repo = new VendorTarrifRepository();
            //vendortransaction=new VendorTransaction();
            //vendor_repo = new VendorRepository();
            //service_repo = new ServiceRepository();
           //IQueryable<VendorTarrif> VT=vendortarrif_repo.VendorTarrif_GetAll();
           // foreach(var VendorTarrif in VT)
           // {
           //     vendortransaction.VendorId = VendorTarrif.VendorId;
           //     vendortransaction.ServiceId = VendorTarrif.ServiceId;
           //     vendortransaction.VendorTarrifId = VendorTarrif.TarrifId;
           //     vendortransaction.CreatedOn = DateTime.Now;
           //     vendortransaction_repo.InserVendorTransaction(vendortransaction);
           // }
             
           

        }

        // GET: VendorTransactions
        public ActionResult Index(int? TransactionId, int? ServiceId, int? VendorId, int? Country, DateTime? FromDate, DateTime? ToDate, string CreatedBy,  int? page, int? ItemsPerPage)
        {

            #region Initialization

            int pageSize =  (ItemsPerPage ?? ConstMessage.ITEMS_PER_PAGE);
            ViewBag.ItemsPerPage = pageSize;
            ViewData["ItemsPerPageSelector"] = ConstMessage.ItemsPerPageSelector;
            int pageNumber = (page ?? 1);
            if (CreatedBy != null)
                ViewBag.CreatedBy = CreatedBy;               
            if (TransactionId != null)
                ViewBag._SelectedTransactionId = TransactionId;           
            if (FromDate != null)
                ViewBag.FromDate = FromDate;
            if (Country != null)
                ViewBag._SelectedCountryId = Country;
            if (ToDate != null)
                ViewBag.ToDate = ToDate;
            if (ServiceId != null)
                ViewBag._SelectedServiceId = ServiceId;
            if (VendorId != null)
                ViewBag._SelectedVendorId = VendorId;
            #endregion

            IServiceRepository service_repo = new ServiceRepository();
            IEnumerable<Service> ServiceList = service_repo.GetAll(true);
            var itemList = service_repo.CreateTree(ServiceList, 0); //
            var newList = new SelectList(itemList, "Id", "Name", "ParentName");
            ViewBag.Service2 = newList;

            //var rawItems = service_repo.CreateTree(service_repo.GetAll(true), 0); //
            //var newList = new SelectList(rawItems, "Id", "Name", "ParentName", 1);
            //ViewBag.ServiceList = newList;

            IVendorRepository vendor_repo = new VendorRepository();
            IEnumerable<Vendor> VendorList = vendor_repo.Vendor_GetAll();
            ViewBag.VendorList = new SelectList(VendorList, "Id", "Name");

            IDestinationRepository destination_repo = new DestinationRepository();
            IEnumerable<Destination> CountryList = destination_repo.GetAll();
            ViewBag.CountryList = new SelectList(CountryList, "Id", "DestinationName");
            var vtr = vendor_repo.VendorTransaction_GetAllQueryable().ToList();

            
            
            List<VendorTransactionMV> vTransaction = new List<VendorTransactionMV>();
            //var vTransaction = new VendorTransactionMV();
            vTransaction = vendor_repo.VendorTransaction_Search(vtr, TransactionId, ServiceId, VendorId, Country, FromDate, ToDate, CreatedBy, ServiceList, VendorList, CountryList);

            var VendorGroup = (from vendor in vTransaction
                               group vendor by vendor.VendorName into g  //new {vendor.VendorName, vendor.ServiceName} into g
                               select new
                               {
                                   VendorName = g.First().VendorName,
                                   ServiceList = g.Select(x=>x.ServiceName).Distinct().ToList(),
                                   ServiceName = g.Select(x=>x.ServiceName).First(),
                                   
                                   BalanceIn = g.Sum(x => x.AmountInUSD),
                                   BalanceOut = g.Sum(x => x.AmountOutUSD),
                               }).ToList();
            
            var VendorSummary = new VendorGP();
            List<VendorGP> vg = new List<VendorGP>();           
            foreach (var item in VendorGroup)
            {
                VendorSummary = new VendorGP();
                VendorSummary.VendorName = item.VendorName;
                //VendorSummary.ServiceList = item.ServiceList;
                VendorSummary.BalanceIn = Math.Round(Convert.ToDecimal(item.BalanceIn), 2);
                VendorSummary.BalanceOut = Math.Round(Convert.ToDecimal(item.BalanceOut), 2);
                VendorSummary.ServiceName = item.ServiceName;
                //foreach(var item2 in item.ServiceList)
                //{
                //    VendorSummary.ServiceList = item2.Count(x=>x.se)
                //}

                vg.Add(VendorSummary);
            }
            
            var vt = new VendorTransactionMV();
            List<VendorTransactionMV> vTt = new List<VendorTransactionMV>(); 
            int sampleNumber = 0;
            foreach(var sample in vTransaction)
            {
                vt = new VendorTransactionMV();
                if(sampleNumber==0)
                    vt.VendorGroup = vg;
                vt.AmountInLocal = sample.AmountInLocal;
                vt.AmountInUSD = sample.AmountInUSD;
                vt.AmountOutLocal = sample.AmountOutLocal;
                vt.AmountOutUSD = sample.AmountOutUSD;
                vt.ConversionRateUSD = sample.ConversionRateUSD;
                vt.CountryId = sample.CountryId;
                vt.CountryName = sample.CountryName;
                vt.CreatedBy = sample.CreatedBy;
                vt.CreatedOn = sample.CreatedOn;
                vt.ServiceId = sample.ServiceId;
                vt.ServiceName = sample.ServiceName;
                vt.TransactionId = sample.TransactionId;
                vt.VendorBalance = sample.VendorBalance;
                vt.VendorId = sample.VendorId;
                vt.VendorName = sample.VendorName;
                vt.VendorTarrifId = sample.VendorTarrifId;
                sampleNumber++;

                vTt.Add(vt);
            }
            // Vendor Transaction Summary
            var TotalBalanceIn = vTransaction.Sum(x => x.AmountInUSD);
            var TotalBalanceOut = vTransaction.Sum(x => x.AmountOutUSD);
            ViewBag.TotalBalanceIn = Math.Round(Convert.ToDecimal(TotalBalanceIn),2);
            ViewBag.TotalBalanceOut = Math.Round(Convert.ToDecimal(TotalBalanceOut));

            //List<VendorGP> VendorGroup = null; // new List<VendorGP>();
            

            //var vndr = vTransaction.GroupBy(x=>x.VendorName).Sum()

            //var Services = vendorTransaction.GroupBy(x => x.ServiceId);
            //var BalanceIn = vendorTransaction.
            
            

           
            
            var vTran = new VendorTransactionMV();
            vTran.VendorGroup = vg;

            //vTransaction.
            //vTransaction.Add(vg);

            return View(vTt.ToPagedList(pageNumber, pageSize));
        }

        // GET: VendorTransactions/Details/5
        public ActionResult Details(int? id)
        {
            IVendorRepository vendor_repo = new VendorRepository();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VendorTransaction vendorTransaction = vendor_repo.VendorTransaction_FindById((int)id);
            if (vendorTransaction == null)
            {
                return HttpNotFound();
            }
            return View(vendorTransaction);
        }

        // GET: VendorTransactions/Create
        public ActionResult Create()
        {
            ICurrencyRepository currency_repo = new CurrencyRepository();
            ViewBag.CurrencyList = new SelectList(currency_repo.GetAll(), "DestinationId", "ISO");
            return View();
        }

        // POST: VendorTransactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VendorTransaction vendorTransaction)
        {
            IVendorRepository vendor_repo = new VendorRepository();
            if (ModelState.IsValid)
            {
                vendorTransaction.CreatedBy = User.Identity.Name;
                vendor_repo.VendorTransaction_Add(vendorTransaction);
                //db.VendorTransactions.Add(vendorTransaction);
                //db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(vendorTransaction);
        }

        //// GET: VendorTransactions/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    IVendorRepository vendor_repo = new VendorRepository();
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    VendorTransaction vendorTransaction = vendor_repo.VendorTransaction_FindById((int)id);
        //    if (vendorTransaction == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(vendorTransaction);
        //}

        //// POST: VendorTransactions/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(VendorTransaction vendorTransaction)
        //{
        //    IVendorRepository vendor_repo = new VendorRepository();
        //    if (ModelState.IsValid)
        //    {
        //        vendor_repo.VendorTransaction_Edit(vendorTransaction);
        //        //db.Entry(vendorTransaction).State = EntityState.Modified;
        //        //db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(vendorTransaction);
        //}

        //// GET: VendorTransactions/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    IVendorRepository vendor_repo = new VendorRepository();
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    VendorTransaction vendorTransaction = vendor_repo.VendorTransaction_FindById((int)id);
        //    if (vendorTransaction == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(vendorTransaction);
        //}

        //// POST: VendorTransactions/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    IVendorRepository vendor_repo = new VendorRepository();
        //    VendorTransaction vendorTransaction = vendor_repo.VendorTransaction_FindById((int)id);
        //    //db.VendorTransactions.Remove(vendorTransaction);
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

        public ActionResult VendorRecharge()
        {
            IVendorRepository vendor_repo = new VendorRepository();
            ICurrencyRepository currency_repo = new CurrencyRepository();
            ViewBag.CurrencyList = new SelectList(currency_repo.GetAll(), "DestinationId", "ISO");
            var VendorList = new SelectList(vendor_repo.Get_ActiveVendor(), "Id", "Name");
            ViewBag.VendorList = VendorList;
    
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VendorRecharge(VendorTransaction vendorTransaction)
        {
            IVendorRepository vendor_repo = new VendorRepository();

            if(ModelState.IsValid)
            {
                vendorTransaction.CreatedOn = DateTime.Now;
                vendorTransaction.CreatedBy = User.Identity.Name;
                if (vendor_repo.VendorRechage(vendorTransaction))
                {
                    ModelState.AddModelError("", "Successfully Recharged");
                    return RedirectToAction("Index");
                }
            }
            ModelState.AddModelError("", "Recharge Failed");
            var VendorList = new SelectList(vendor_repo.Get_ActiveVendor(), "Id", "Name");
            ViewBag.VendorList = VendorList;

            return View(vendorTransaction);
        }
    }
}
