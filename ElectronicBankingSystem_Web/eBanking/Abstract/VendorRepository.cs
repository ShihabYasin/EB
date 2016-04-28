using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eBanking.Models;
using eBanking.Interface;
using System.Data.Entity;
using eBanking.App_Code;

namespace eBanking.Abstract
{
    public class VendorRepository:IVendorRepository
    {
        #region Constructors and global variables
        
        private eBankingDbContext db;
        public VendorRepository()
        {
            this.db = new eBankingDbContext();
        }
        public VendorRepository(eBankingDbContext dbContext)
        {
            this.db = dbContext;
        }
        
        #endregion
        public List<VendorTransactionMV> VendorTransaction_Search(IEnumerable<VendorTransaction> vtr, int? TransactionId, int? ServiceId, int? VendorId, int? Country, DateTime? FromDate, DateTime? ToDate, string CreatedBy, IEnumerable<Service> ServiceList, IEnumerable<Vendor> VendorList, IEnumerable<Destination> CountryList)
        {

            List<VendorTransactionMV> vendorTransaction = null;
            IEnumerable<VendorTransactionMV> Tran_temp = null, Tran_temp2 = null;
            //IEnumerable<VendorTransactionMV> Tran_temp2 = null;

            vendorTransaction = (from vt in vtr
                                 join s in ServiceList on vt.ServiceId equals s.Id
                                 join v in VendorList on vt.VendorId equals v.Id
                                 join c in CountryList on vt.UsedCurrencyId equals c.Id

                                 select new 
                                 {
                                     TransactionId = vt.TransactionId,
                                     ServiceName = s.Name,
                                     VendorName = v.Name,
                                     CountryName = c.DestinationName,
                                     VendorTarrifId = vt.VendorTarrifId,
                                     AmountInLocal=vt.AmountInLocal,
                                     AmountOutLocal=vt.AmountOutLocal,
                                     ConversionRateUSD=vt.ConversionRateUSD,
                                     AmountInUSD=vt.AmountInUSD,
                                     AmountOutUSD=vt.AmountOutUSD,
                                     CreatedOn=vt.CreatedOn,
                                     CreatedBy=vt.CreatedBy,
                                     VendorBalance = vt.VendorBalance,
                                     ServiceId = vt.ServiceId,
                                     VendorId = vt.VendorId,
                                     CountryId = vt.UsedCurrencyId
                                 }).AsEnumerable().Select(VendorT => new VendorTransactionMV
                                    {
                                        TransactionId = VendorT.TransactionId,
                                        ServiceName = VendorT.ServiceName,
                                        VendorName = VendorT.VendorName,
                                        CountryName = VendorT.CountryName,
                                        VendorTarrifId = VendorT.VendorTarrifId,
                                        AmountInLocal= VendorT.AmountInLocal,
                                        AmountOutLocal = VendorT.AmountOutLocal,
                                        ConversionRateUSD = VendorT.ConversionRateUSD,
                                        AmountInUSD = VendorT.AmountInUSD,
                                        AmountOutUSD = VendorT.AmountOutUSD,
                                        CreatedOn = VendorT.CreatedOn,
                                        CreatedBy = VendorT.CreatedBy,
                                        VendorBalance = VendorT.VendorBalance,
                                        ServiceId = VendorT.ServiceId,
                                        VendorId = VendorT.VendorId,
                                        CountryId = VendorT.CountryId
                                    }).ToList();

            if (TransactionId > 0)
                vendorTransaction = vendorTransaction.Where(x => x.TransactionId.Equals(TransactionId)).ToList();
            //if (ServiceId > 0)
            //    vendorTransaction = vendorTransaction.Where(x => x.ServiceId.Equals(ServiceId));
            if (VendorId > 0)
                vendorTransaction = vendorTransaction.Where(x => x.VendorId.Equals(VendorId)).ToList();
            if (Country > 0)
                vendorTransaction = vendorTransaction.Where(x => x.CountryId.Equals(Country)).ToList();
            if (FromDate != null && FromDate != DateTime.MinValue)
            {
                vendorTransaction = vendorTransaction.Where(x => x.CreatedOn.Date >= FromDate.Value.Date).ToList();
            }
            if (ToDate != null && ToDate != DateTime.MinValue)
            {
                vendorTransaction = vendorTransaction.Where(x => x.CreatedOn.Date <= ToDate.Value.Date).ToList();
            }

            if (ServiceId > 0)
            {
                Tran_temp2 = vendorTransaction.Where(x => x.ServiceId.Equals(ServiceId)).ToList();

                if (ServiceList.Where(x => x.Id.Equals(ServiceId)).SingleOrDefault().IsGroup)
                {
                    var GroupServices = ServiceList.Where(x => x.ParentId.Equals(ServiceId));

                    foreach (var item in GroupServices)
                    {
                        Tran_temp = vendorTransaction.Where(x => x.ServiceId == item.Id);
                        Tran_temp2 = Tran_temp2.Concat(Tran_temp);
                    }
                }
                //vendorTransaction = Tran_temp2;
            }

            //// Vendor Transaction Summary
            //var TotalBalanceIn = vendorTransaction.Sum(x => x.AmountInUSD);
            //var TotalBalanceOut = vendorTransaction.Sum(x => x.AmountInUSD);
            ////var Services = vendorTransaction.GroupBy(x => x.ServiceId);
            ////var BalanceIn = vendorTransaction.

            //var VendorSummary = new VendorTransactionMV();
            //VendorSummary.TotalBalanceIn = TotalBalanceIn;
            //VendorSummary.TotalBalanceOut = TotalBalanceOut;
            //vendorTransaction.Add(VendorSummary);
            return vendorTransaction;
        }
        public IEnumerable<Vendor> Search(IEnumerable<Vendor> vendor, int? Name, bool? IsActive, DateTime? FromDate, DateTime? ToDate)
        {
            if (Name>0)
                vendor = vendor.Where(x => x.Id.Equals(Name));
            if (IsActive != null)
                vendor = vendor.Where(x => x.IsActive.Equals(IsActive));
            if (FromDate != null && FromDate != DateTime.MinValue)
                vendor = vendor.Where(x => x.CreatedOn >= FromDate);
            if (ToDate != null && ToDate != DateTime.MinValue)
                vendor = vendor.Where(x => x.CreatedOn <= ToDate);
            
            return vendor;
        }
        /// Vendor Create
        #region Vendor
        public IQueryable<Vendor> Vendor_GetAll()
        {
            try
            {
                return db.Vendors;
            }
            catch(Exception )
            {

            }
            return null;
        }

        public bool Vendor_Create(Vendor vendor)
        {
            try
            {
                db.Vendors.Add(vendor);
                Save();
                return true;
            }
            catch (Exception) { }

            return false;
        }
        public Vendor Vendor_Delete(int id)
        {
            Vendor vendor = new Vendor();
              try
                {
                    vendor = FindVendorById(id);
                    if (vendor != null)
                    {

                        db.Vendors.Remove(vendor);
                        Save();
                        return vendor;
                    }
                }
            catch(Exception)
              {

              }
            
            return null;
        }

        public bool Vendor_Edit(Vendor vendor)
        {
            try
            {
                db.Entry(vendor).State = EntityState.Modified;
                Save();
                return true;
            }
            catch (Exception e) 
            {
              string mgs= e.Message.ToString();
            }

            return false;
        }

        public Vendor FindVendorById(int? id)
        {
            Vendor vendor = new Vendor();
            try
            {
                vendor = db.Vendors.Where(v => v.Id == id).Select(v => v).SingleOrDefault();
                return vendor;
            }
            catch(Exception)
            {

            }
            return null;
        }

        public bool  FindVendorByName(string Name)
        {
            Vendor vendor = new Vendor();
            try
            {
                vendor = db.Vendors.Where(v => v.Name == Name).Select(v => v).SingleOrDefault();
                if (vendor!=null)
                 {
                     return true;
                 }
                
            }
            catch(Exception)
            {

            }
            return false;

        }

        public decimal? VendorBalanceUpdate(int VendorId, decimal Amount)
      {
          try
          {
              var vendor = db.Vendors.Where(V => V.Id == VendorId).FirstOrDefault();
              vendor.CurrentBalance += Amount;
              db.Entry(vendor).State = EntityState.Modified;
              Save();
              return vendor.CurrentBalance;
          }
          catch (Exception) { }
          return null;
      }
        public IQueryable<Vendor> Get_ActiveVendor()
        {

            try
            {
                return db.Vendors.Where(V => V.IsActive == true);
            }
            catch (Exception)
            {

            }
            return null;

        }
        #endregion

        #region Vendor Tarrif
        public IQueryable<VendorTarrif> VendorTarrif_GetAll()
        {
            try
            {
                return db.VendorTarrifs;
            }
            catch (Exception) { }
            return null;
        }

        public bool VendorTarrif_Create(VendorTarrif vendorTarrif)
        {
            try
            {
                db.VendorTarrifs.Add(vendorTarrif);
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public bool VendorTarrif_Delete(int id)
        {
            try
            {
                db.VendorTarrifs.Remove(FindVendorTarrifById(id));
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
            //throw new NotImplementedException();
        }

        public bool VendorTarrif_Edit(VendorTarrif vendorTarrif)
        {
            try
            {
                db.Entry(vendorTarrif).State = EntityState.Modified;
                Save();
                return true;
            }
            catch (Exception e)
            {
                string mgs = e.Message.ToString();
            }

            return false;
        }

        public VendorTarrif FindVendorTarrifById(int? id)
        {
            VendorTarrif vendorTarrif = new VendorTarrif();
            try
            {
                vendorTarrif = db.VendorTarrifs.Where(VT => VT.TarrifId == id).Select(VT => VT).SingleOrDefault();

                return vendorTarrif;
            }
            catch (Exception) { }
            return null;
        }

        public bool FindVendorTarrifByName(string Name)
        {
            throw new NotImplementedException();
        }


        public void VendorTarrif_DeactivatePrevious(int serviceId, int VendorId)
        {
            try
            {
                IEnumerable<VendorTarrif> list = VendorTarrif_GetAll().Where(VT => (VT.ServiceId == serviceId && VT.VendorId == VendorId) && VT.IsActive == true).Select(VT => VT).ToList();

                foreach (var item in list)
                {
                    try
                    {
                        item.IsActive = false;
                        db.Entry(item).State = EntityState.Modified;
                        Save();
                    }
                    catch (Exception) { }

                }

            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region Vendor Route
        public IEnumerable<VendorRoute> VendorRoute_getAll()
        {
            try
            {
                return db.VendorRoutes.ToList();
            }
            catch (Exception) { }
            return null;
        }
        public IEnumerable<VendorRoute> VendorRoute_GetAllByService(int ServiceId)
        {
            try
            {
                var element = db.VendorRoutes.Where(v => v.ServiceId == ServiceId).ToList();
                return element.OrderByDescending(v => v.Priority).ThenBy(v => v.PendingRequest).ToList();
            }
            catch (Exception) { }
            return null;
        }
        public VendorRoute VendorRoute_GetByServiceAndVendor(int ServiceId, int VendorId)
        {
            try
            {
                return db.VendorRoutes.Where(r => r.ServiceId == ServiceId && r.VendorId == VendorId).SingleOrDefault();
            }
            catch (Exception) { }
            return null;
        }
        public bool VendorRoute_Add(VendorRoute model)
        {
            try
            {
                db.VendorRoutes.Add(model);
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public bool VendorRoute_UpdatePendingQty(int Id, bool IsIncrement)
        {
            try
            {
                VendorRoute route = db.VendorRoutes.Find(Id);

                if (IsIncrement)
                    route.PendingRequest += 1;
                else
                    route.PendingRequest -= 1;

                db.Entry(route).State = EntityState.Modified;
                Save();
                return true;
            }
            catch (Exception e)
            {
                string mgs = e.Message.ToString();
            }

            return false;
        }
        
        public IQueryable<VendorRoute> VendorRoute_GetAllQueryable()
        {
            try
            {
                return db.VendorRoutes;
            }
            catch (Exception) { }
            return null;
        }
        #endregion

        #region Vendor Transaction
        public VendorTransaction VendorTransaction_FindById(int Id)
        {
            try
            {
                return db.VendorTransactions.Find(Id);
            }
            catch (Exception) { }
            return null;
        }
        public IQueryable<VendorTransaction> VendorTransaction_GetAllQueryable()
        {
            try
            {
                return db.VendorTransactions;
            }
            catch (Exception) { }
            return null;
        }


        public bool VendorTransaction_Add(Transaction transaction, int BalanceUpdate)
        {
            VendorTransaction vendortransaction = new VendorTransaction();
            //IVendorRepository vendor_repo = new VendorRepository();

            vendortransaction.TransactionId = transaction.Id;
            vendortransaction.ServiceId = transaction.ServiceId;
            vendortransaction.AmountOutLocal = transaction.InsertedAmount;
            vendortransaction.CreatedOn = DateTime.Now;
            vendortransaction.CreatedBy = transaction.UserId;

            //transaction.VendorId = 4;
            vendortransaction.VendorId = transaction.VendorId;
            var vendorTarrif = db.VendorTarrifs.Where(VT => (VT.VendorId == transaction.VendorId) && (VT.ServiceId == transaction.ServiceId)).SingleOrDefault();

            vendortransaction.ConversionRateUSD = vendorTarrif.ConversionRate;
            //todo - check if the rateplan is percantage or fixed amount and calculte accordingly
            if (vendorTarrif.IsPercentage)
            {
                decimal ServiceCharge = ((vendorTarrif.Cost / 100) * (transaction.InsertedAmount ?? 0));
                //decimal TotalCost = ( ServiceCharge + (transaction.InsertedAmount ?? 0));
                vendortransaction.AmountOutUSD = Math.Round((ServiceCharge / vendorTarrif.ConversionRate), 6);
            }
            else
                vendortransaction.AmountOutUSD = Math.Round((vendorTarrif.Cost / vendorTarrif.ConversionRate),6);//(decimal)transaction.AmountOut;

            var vendor = db.Vendors.Where(V => V.Id == transaction.VendorId).SingleOrDefault();

            vendortransaction.UsedCurrencyId = vendor.LocalCurrencyId;
            if (BalanceUpdate == ConstMessage.Balance_Deduct)
            {
                vendortransaction.VendorBalance = VendorBalanceUpdate(transaction.VendorId, -(decimal)transaction.InsertedAmount);
            }
            else
            {
                vendortransaction.VendorBalance = VendorBalanceUpdate(transaction.VendorId, (decimal)transaction.InsertedAmount);
                vendortransaction.AmountInUSD = vendortransaction.AmountOutUSD;
                vendortransaction.AmountOutUSD = 0;
            }

            try
            {
                this.db.VendorTransactions.Add(vendortransaction);
                Save();
                return true;
            }
            catch (Exception)
            {

            }
            return false;
        }


        public bool VendorTransaction_Add(VendorTransaction vendortransaction)
        {
            try
            {
                db.VendorTransactions.Add(vendortransaction);
                Save();
                return true;
            }
            catch (Exception)
            {

            }

            return false;
        }

        public bool VendorTransaction_Edit(VendorTransaction vendorTransaction)
        {
            try
            {
                db.Entry(vendorTransaction).State = EntityState.Modified;
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public bool VendorRechage(VendorTransaction vendortransaction)
        {
            //IVendorRepository vendor_repo = new VendorRepository();
            IServiceRepository service_repo = new ServiceRepository();
            var vendor = db.Vendors.Where(V => (V.Id == vendortransaction.VendorId && V.IsActive == true)).SingleOrDefault();

            if (vendor != null)
            {
                //Service service = new Service();
                vendortransaction.AmountInUSD = vendortransaction.AmountInLocal / vendortransaction.ConversionRateUSD;
                //service = service_repo.FindByName("VendorRecharge");
                vendortransaction.ServiceId = ConstMessage.Services_Vendor_Recharge;//service.Id
                try
                {
                    vendortransaction.VendorBalance = VendorBalanceUpdate(vendor.Id, (decimal)vendortransaction.AmountInLocal);
                    this.db.VendorTransactions.Add(vendortransaction);
                    Save();
                    return true;
                }
                catch (Exception) { }

            }

            return false;
        }

        public bool VendorBalanceReturn(int TransactionId)
        {
            try
            {
                //ITransactionRepository tran_repo = new TransactionRepository();
                var oldTransaction = db.VendorTransactions.Where(vt => vt.TransactionId == TransactionId).SingleOrDefault();

                VendorTransaction vendorTransaction = new VendorTransaction();
                vendorTransaction.ServiceId = ConstMessage.Services_Vendor_Balance_Return;
                vendorTransaction.TransactionId = oldTransaction.TransactionId;
                vendorTransaction.UsedCurrencyId = oldTransaction.UsedCurrencyId;
                vendorTransaction.VendorId = oldTransaction.VendorId;
                vendorTransaction.VendorTarrifId = 0;
                vendorTransaction.AmountInLocal = (oldTransaction.AmountOutLocal > 0 ? oldTransaction.AmountOutLocal : 0);
                vendorTransaction.AmountInUSD = (oldTransaction.AmountOutUSD > 0 ? oldTransaction.AmountOutUSD : 0);
                vendorTransaction.AmountOutLocal = 0;
                vendorTransaction.AmountOutUSD = 0;
                vendorTransaction.ConversionRateUSD = oldTransaction.ConversionRateUSD;
                vendorTransaction.CreatedBy = "system";
                vendorTransaction.CreatedOn = DateTime.Now;

                vendorTransaction.VendorBalance = VendorBalanceUpdate(vendorTransaction.VendorId, (decimal)vendorTransaction.AmountInLocal);
                
                return VendorTransaction_Add(vendorTransaction);

            }
            catch (Exception) { }

            return false;
        }
        #endregion

        #region Vendor Request Log
        public IQueryable<VendorRequestLog> VendorReqLog_GetAll()
        {
            try
            {
                return db.VendorRequestLogs;
            }
            catch (Exception) { }
            return null;
        }
        public IQueryable<VendorRequestLog> GetActiveVendorReqWithDetailsByVendorId(int VendorId)
        {
            try
            {
                return db.VendorRequestLogs.Where(v => v.VendorId == VendorId);
            }
            catch (Exception) { }
            return null;
        }

        public IQueryable<VendorRequestLog> GetVendorReqWithReqId(int ReqId)
        {
            try
            {
                return db.VendorRequestLogs.Where(v => v.RequestId == ReqId);
            }
            catch (Exception) { }
            return null;
        }

        public bool VendorRequestLog_Edit(VendorRequestLog vendorRequestLog)
        {
            try
            {
                db.Entry(vendorRequestLog).State = EntityState.Modified;
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public bool VendorRequestLog_Add(VendorRequestLog vrl)
        {
            try
            {
                db.VendorRequestLogs.Add(vrl);
                Save();
            }
            catch (Exception) { }
            return false;
        }
        #endregion

        #region Shared Functions
        public void Save()
        {
            db.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
        
    }
}