using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eBanking.Interface;
using eBanking.Models;
using System.Data.Entity;
using PagedList;
using eBanking.App_Code;

namespace eBanking.Abstract
{
    public class DistributorRepository:IDistributorRepository
    {
        private eBankingDbContext db;

        public DistributorRepository()
        {
            this.db = new eBankingDbContext();
        }
        public DistributorRepository(eBankingDbContext dbContext)
        {
            this.db = dbContext;
        }
        public IEnumerable<DistributorTransaction> GetAllDistributorTransaction()
        {
            return db.DistributorTransactions.ToList();
        }

        public IQueryable<Distributor> Distributor_GetAllQueryable()
        {
            try
            {
                return db.Distributors;
            }
            catch (Exception) { }
            return null;
        }
        public IPagedList<DistributorTransactionVM> DistributorTransactionVM(IQueryable<DistributorTransaction> DT, int pageNumber, int pageSize) //IEnumerable<DistributorTransactionVM>
        {
            IPagedList<DistributorTransactionVM> dtvm = null;
            IQueryable<Distributor> distributor = Distributor_GetAllQueryable();
            //Service ser = new Service();
            IQueryable<Service> service = db.Services;
            try
            {
                var temp = DT.ToList();
                dtvm = (from dt in DT
                            join d in distributor on dt.DistributorId equals d.DistributorId
                            join s in service on dt.ServiceId equals s.Id into sjoin

                            from sj in sjoin.DefaultIfEmpty()
                            select new 
                            {
                               Id = dt.DTId,
                               ServiceName = sj.Name,
                               DistributorName = d.UserName,
                               dt.AmountIn,
                               dt.AmountOut,
                               dt.CurrentBalance,
                               dt.CreatedBy,
                               dt.CreatedOn,
                               dt.AmountInLocal,
                               dt.AmountOutLocal,
                               dt.CurrencyId,
                               dt.ConvertToUsd,
                               dt.HasDetails
                            }).AsEnumerable().Select(distributorTransaction => new DistributorTransactionVM
                            {
                                Id = distributorTransaction.Id,
                                ServiceName = distributorTransaction.ServiceName,
                                DistributorName = distributorTransaction.DistributorName,
                                AmountIn = distributorTransaction.AmountIn,
                                AmountOut = distributorTransaction.AmountOut,
                                CurrentBalance = distributorTransaction.CurrentBalance,
                                CreatedBy = distributorTransaction.CreatedBy,
                                CreatedOn = distributorTransaction.CreatedOn,
                                AmountInLocal = distributorTransaction.AmountInLocal,
                                AmountOutLocal = distributorTransaction.AmountOutLocal,
                                CurrencyId = distributorTransaction.CurrencyId,
                                ConvertToUsd = distributorTransaction.ConvertToUsd,
                                HasDetails = distributorTransaction.HasDetails
                            }
                            ).OrderByDescending(dt=>dt.CreatedOn).ToPagedList(pageNumber, pageSize);
            }
            catch (Exception) { }
            return dtvm;
        }
        public IQueryable<DistributorTransaction> DistributorTransaction_Search(int? ServiceId, int? DistributorId, DateTime? FromDate, DateTime? ToDate)//IQueryable<DistributorTransaction> dt,
        {
            IQueryable<DistributorTransaction> dt = db.DistributorTransactions;
            if (ServiceId != null)
                dt = dt.Where(x => x.ServiceId == ServiceId);
            if (DistributorId != null)
                dt = dt.Where(x => x.DistributorId == DistributorId);

            if (FromDate != null && FromDate != DateTime.MinValue)
            {
                dt = dt.Where(x => x.CreatedOn >= FromDate);
            }
            if (ToDate != null && ToDate != DateTime.MinValue)
            {
                dt = dt.Where(x => x.CreatedOn <= ToDate);
            }
            return dt;
        }
        public Distributor Distributor_FindById(int DistributorId)
        {
            try
            {
                return db.Distributors.Where(d => d.DistributorId == DistributorId).SingleOrDefault();
            }
            catch (Exception) { }
            return null;
        }
        public Distributor Distributor_FindByUserName(string UserName)
        {
            try
            {
                return db.Distributors.Where(d => d.UserName == UserName).SingleOrDefault();
            }
            catch (Exception) { }
            return null;
        }
        public IEnumerable<Distributor> Distributor_GetAll()
        {
            try
            {
                return db.Distributors.Where(d=>d.IsActive == true).ToList();
            }
            catch (Exception) { }
            return null;
        }

        public bool Distributor_Add(Distributor distributor)
        {
            try
            {
                using (var contextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        distributor.DistributorBalance = 0;
                        db.Distributors.Add(distributor);
                        Save();
                        if(distributor.ParentId == 0)
                            distributor.DistributorCode = distributor.DistributorId.ToString();
                        else
                        {
                            distributor.DistributorCode = distributor.DistributorCode + "-" + distributor.DistributorId.ToString();
                        }
                        db.Entry(distributor).State = EntityState.Modified;
                        Save();
                        contextTransaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        contextTransaction.Rollback();
                    }
                }
            }
            catch (Exception) { }
            return false;
        }

        public IEnumerable<DistributorCommissionRateplan> DCRP_GetAll()
        {
            IEnumerable<DistributorCommissionRateplan> DCRP_List = null;
            try
            {
                DCRP_List = db.DistributorCommissionRateplans.ToList();
            }
            catch (Exception) { }
            return DCRP_List;
        }

        public IQueryable<DistributorCommissionRateplan> DCRP_GetAllQueryable()
        {
            IQueryable<DistributorCommissionRateplan> DCRP_List = null;
            try
            {
                DCRP_List = db.DistributorCommissionRateplans;
            }
            catch (Exception) { }
            return DCRP_List;
        }
        public IQueryable<DistributorCommissionRateplanViewModel> DCRP_GetAllVMByDistributorId(int? DistributorId)
        {
            IQueryable<DistributorCommissionRateplan> DCRP_qList = null;
            IQueryable<DistributorCommissionRateplanViewModel> DCRP_List = null;
            IQueryable<Service> services = db.Services;
            IQueryable<Distributor> distributors = db.Distributors;
            try
            {
                if(DistributorId != null)
                    DCRP_qList = db.DistributorCommissionRateplans.Where(d => d.DistributorId == DistributorId);
                else
                    DCRP_qList = db.DistributorCommissionRateplans;
                DCRP_List = (from dcrp in DCRP_qList
                            join srvc in services on dcrp.ServiceId equals srvc.Id into serviceJoin
                            join dist in distributors on dcrp.DistributorId equals dist.DistributorId into distributorJoin
                            from s in serviceJoin.DefaultIfEmpty()
                            from d in distributorJoin.DefaultIfEmpty()
                             select new DistributorCommissionRateplanViewModel
                            {
                                Commission = dcrp.Commission,
                                CreatedBy = dcrp.CreatedBy,
                                CreatedOn = dcrp.CreatedOn,
                                DCRP_ID = dcrp.DCRP_ID,
                                Discount = dcrp.Discount,
                                DistributorId = dcrp.DistributorId,
                                DistributorUserName = d.UserName,
                                IsActive = dcrp.IsActive,
                                IsPercentage = dcrp.IsPercentage,
                                ServiceId = dcrp.ServiceId,
                                ServiceName = s.Name,
                                ServiceCharge = dcrp.ServiceCharge,
                                RateName = dcrp.RateName,
                                Remarks = dcrp.Remarks
                            });

            }
            catch (Exception) { }
            return DCRP_List;
        }
        public bool DCRP_DeactivatePrevious(int DistributorId, int ServiceId)
        {
            try
            {
                IEnumerable<DistributorCommissionRateplan> list = DCRP_GetAllQueryable().Where(x => x.ServiceId == ServiceId && x.DistributorId == DistributorId && x.IsActive == true).Select(x => x).ToList();

                foreach (var item in list)
                {
                    item.IsActive = false;
                    db.Entry(item).State = EntityState.Modified;
                    Save();
                    return true;
                }
            }
            catch (Exception){ }
            return false;
        }
        public DistributorCommissionRateplan DCRP_GetById(int DCRP_ID)
        {
            try
            {
                return db.DistributorCommissionRateplans.Where(d=>d.DCRP_ID == DCRP_ID).SingleOrDefault();
            }
            catch (Exception) { }
            return null;
        }
        public bool DCRP_Create(DistributorCommissionRateplan DCRP_Model)
        {
            try
            {
                db.DistributorCommissionRateplans.Add(DCRP_Model);
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }
        public DistributorServiceChargeVM DCRP_GetDistributorServiceCharge(int ServiceId, string DistributorCode)
        {
            DistributorServiceChargeVM result = new DistributorServiceChargeVM();
            result.ServiceId = ServiceId;
            try
            {
                string[] parts = DistributorCode.Split('-');
                List<int> distributors = new List<int>();
                for (int i = parts.Length - 1; i>=0; i-- )
                {
                    distributors.Add(int.Parse(parts[i]));
                }
                int firstDistributor = distributors.First();
                var RetailDistributor = Distributor_FindById(firstDistributor);
                UserRoleRepository userRole_repo = new UserRoleRepository(db);
                string LastDistributorRole = userRole_repo.GetRoleByUserName(RetailDistributor.UserName);
                if (LastDistributorRole == ConstMessage.ROLE_NAME_RETAILER)
                {
                    var temp = DCRP_GetAllQueryable().Where(d => d.DistributorId == firstDistributor && d.ServiceId == ServiceId && d.IsActive == true).FirstOrDefault();
                    if (temp != null)
                    {
                        result.IsPercentage = false;
                        if(temp.ServiceCharge != null)
                            result.ServiceCharge += temp.ServiceCharge;
                    }
                }

                foreach (var distributor in distributors)
                {
                    var tempServiceCharge = DCRP_GetAssignedToDCRP(distributor).Where(dcrp=>dcrp.ServiceId == ServiceId).FirstOrDefault();
                    if (tempServiceCharge != null && tempServiceCharge.ServiceCharge != null)
                    {
                        result.ServiceCharge += tempServiceCharge.ServiceCharge;
                    }
                }
                return result;

            }
            catch (Exception) { }
            return null;
        }
        public IEnumerable<DistributorServiceChargeVM> DCRP_GetDSCforEachDistributor(int serviceId, string distributorCode)
        {
            List<DistributorServiceChargeVM> result = new List<DistributorServiceChargeVM>();
            try
            {
                string[] parts = distributorCode.Split('-');
                List<int> distributors = new List<int>();
                for (int i = parts.Length - 1; i >= 0; i--)
                {
                    distributors.Add(int.Parse(parts[i]));
                }
                int firstDistributor = distributors.First();
                var RetailDistributor = Distributor_FindById(firstDistributor);
                UserRoleRepository userRole_repo = new UserRoleRepository(db);
                string LastDistributorRole = userRole_repo.GetRoleByUserName(RetailDistributor.UserName);
                if (LastDistributorRole == ConstMessage.ROLE_NAME_RETAILER)
                {
                    var temp = DCRP_GetAllQueryable().Where(d => d.DistributorId == firstDistributor && d.ServiceId == serviceId && d.IsActive == true).FirstOrDefault();
                    if (temp != null)
                    {
                        DistributorServiceChargeVM addResult = new DistributorServiceChargeVM();
                        addResult.DistributorId = firstDistributor;
                        addResult.ServiceId = temp.ServiceId;
                        addResult.IsPercentage = false;
                        if (temp.ServiceCharge != null)
                            addResult.ServiceCharge = temp.ServiceCharge;

                        result.Add(addResult);
                    }
                }

                foreach (var distributor in distributors)
                {
                    var tempAssignedRateplan = DCRP_GetAssignedToDCRP(distributor).Where(dcrp => dcrp.ServiceId == serviceId).FirstOrDefault();
                    if (tempAssignedRateplan != null && tempAssignedRateplan.ServiceCharge != null)
                    {
                        DistributorServiceChargeVM addResult = new DistributorServiceChargeVM();
                        var originalDCRP = DCRP_GetById(tempAssignedRateplan.DCRP_ID);
                        addResult.ServiceCharge = originalDCRP.ServiceCharge;
                        addResult.DistributorId = originalDCRP.DistributorId;
                        addResult.ServiceId = originalDCRP.ServiceId;
                        addResult.IsPercentage = false;
                        result.Add(addResult);
                    }
                }
                return result;
            }
            catch (Exception) { }
            return null;
        }
        public IQueryable<DistributorCommissionRateplanViewModel> DCRP_GetAssignedToDCRP(int distributorId)
        {
            IQueryable<DistributorCommissionRateplanViewModel> DCRPList = null;
            IServiceRepository service_repo = new ServiceRepository(db);
            
            try
            {
                var currentDistributor = Distributor_FindById(distributorId);
                IQueryable<DistributorCommissionRateplan> DCRPListExtended = DCRP_GetAllQueryable();
                IQueryable<AssignDistributorRateplan> assignedTo = ADR_GetAllQueryable().Where(d => d.DistributorId == currentDistributor.DistributorId && d.IsActive == true);
                IQueryable<Service> allService = service_repo.GetAllQueryable();
                DCRPList = (from ato in assignedTo
                            join dcrp in DCRPListExtended on ato.RateplanId equals dcrp.DCRP_ID
                            join svcs in allService on dcrp.ServiceId equals svcs.Id
                            select new DistributorCommissionRateplanViewModel
                            {
                                Commission = dcrp.Commission,
                                CreatedBy = dcrp.CreatedBy,
                                CreatedOn = dcrp.CreatedOn,
                                DCRP_ID = dcrp.DCRP_ID,
                                Discount = dcrp.Discount,
                                DistributorId = dcrp.DistributorId,
                                DistributorUserName = currentDistributor.UserName,
                                IsActive = dcrp.IsActive,
                                IsPercentage = dcrp.IsPercentage,
                                ServiceId = dcrp.ServiceId,
                                ServiceName = svcs.Name,
                                ServiceCharge = dcrp.ServiceCharge,
                                RateName = dcrp.RateName,
                                Remarks = dcrp.Remarks
                            }).OrderBy(d => d.ServiceId);
            }
            catch (Exception) { }
            return DCRPList;
        }
        public decimal? DistributorBalanceUpdate(int DistributorId, decimal Amount)
        {
            try
            {
                Distributor dist = db.Distributors.Where(d => d.DistributorId == DistributorId).FirstOrDefault();
                dist.DistributorBalance = (dist.DistributorBalance ?? 0) + Amount;
                db.Entry(dist).State = EntityState.Modified;
                Save();
                return dist.DistributorBalance;
            }
            catch (Exception) { }
            return null;
        }
        public IQueryable<DistributorTransaction> DistributorTransaction_GetAllQueryable()
        {
            try
            {
                return db.DistributorTransactions;
            }
            catch (Exception) { }
            return null;
        }
        public int? DistributorTransaction_Add(DistributorTransaction transaction)
        {
            using (var contextTransaction = db.Database.BeginTransaction())
            {     
                try
                {
                    transaction.CurrentBalance = DistributorBalanceUpdate(transaction.DistributorId, (decimal)(transaction.AmountIn - transaction.AmountOut));
                    db.DistributorTransactions.Add(transaction);
                    Save();
                    contextTransaction.Commit();
                    return transaction.DTId;
                }
                catch (Exception) 
                {
                    contextTransaction.Rollback();
                }
            }
            return null;
        }
        public int? DistributorTransaction_Update(DistributorTransaction transaction, decimal? PreviousAmountIn, decimal? PreviousAmountOut)
        {
            using (var contextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    decimal originalAmountIn = 0;
                    originalAmountIn = (decimal)((decimal)transaction.AmountIn - (decimal)PreviousAmountIn);
                    originalAmountIn -= (decimal)((decimal)transaction.AmountOut - (PreviousAmountOut ?? 0));
                    transaction.CurrentBalance = DistributorBalanceUpdate(transaction.DistributorId, originalAmountIn);
                    db.Entry(transaction).State = EntityState.Modified;
                    //db.DistributorTransactions.Add(transaction);
                    Save();
                    contextTransaction.Commit();
                    return transaction.DTId;
                }
                catch (Exception)
                {
                    contextTransaction.Rollback();
                }
            }
            return null;
        }
        public bool DistributorTransactionDetails_Add(DistributorTransactionDetail model)
        {
            try
            {
                db.DistributorTransactionDetails.Add(model);
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }
        public IQueryable<DistributorTransactionDetail> DistributorTransactionDetails_GetAllQueryable()
        {
            try
            {
                return db.DistributorTransactionDetails;
            }
            catch (Exception) { }
            return null;
        }
        public decimal? GetDistributorBalanceByDistCode(string DistributorCode)
        {
            try
            {
                return db.Distributors.Where(d => d.DistributorCode == DistributorCode).SingleOrDefault().DistributorBalance;
            }
            catch (Exception) { }
            return null;
        }
        public int GetDistributorIdFromDistributorCode(string DistributorCode)
        {
            string[] parts = DistributorCode.Split('-');
            return int.Parse(parts[parts.Length - 1]);
        }
        public IQueryable<AssignDistributorRateplan> ADR_GetAllQueryable()
        {
            try
            {
                return db.AssignDistributorRateplans;
            }
            catch (Exception) { }
            return null;
        }
        public bool ADR_Create(AssignDistributorRateplan model)
        {
            try
            {
                db.AssignDistributorRateplans.Add(model);
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }
        public bool ADR_Edit(AssignDistributorRateplan model)
        {
            try
            {
                db.Entry(model).State = EntityState.Modified;
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }
        public void Save()
        {
            db.SaveChanges();
            //try
            //{
            //    db.SaveChanges();
            //}
            //catch (OptimisticConcurrencyException)
            //{
            //    db.Refresh(RefreshMode.ClientWins, db.Distributors);
            //    db.SaveChanges();
            //}
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
    }
}