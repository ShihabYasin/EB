using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eBanking.Models;
using eBanking.Interface;
using System.Data.Entity;

namespace eBanking.Abstract
{
    public class RateplanRepository:IRatePlanRepository
    {
        private eBankingDbContext db;
        private IEnumerable<RatePlan> RatePlanList;
        private RatePlan ratePlan;
        
        private CurrencyRepository currency_repo;
        private ServiceRepository service_repo;


        public RateplanRepository()
        {
            this.db = new eBankingDbContext();
            currency_repo = new CurrencyRepository(this.db);
            service_repo = new ServiceRepository(this.db);
        }
        public RateplanRepository(eBankingDbContext context)
        {
            this.db = context;
            currency_repo = new CurrencyRepository(this.db);
            service_repo = new ServiceRepository(this.db);
        }
        public IEnumerable<RatePlan> GetAll()
        {
            try
            {
                RatePlanList =db.RatePlans.Where(x=>x.IsActive==true).ToList();
                return RatePlanList;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public IQueryable<RatePlan> GetAllQueryable()
        {
            try
            {
                return db.RatePlans;
            }
            catch(Exception){ }
            return null;
        }

        public IEnumerable<RatePlanViewModel> GetAlltoRatePlanVM()
        {
            IQueryable<RatePlan> ratePlans = GetAllQueryable();
            IQueryable<Currency> currencys = currency_repo.GetAllQueryable();
            IQueryable<Service> services = service_repo.GetAllQueryable().Where(s=>s.IsActive == true);
            
            IEnumerable<RatePlanViewModel> RatePlanViewList = null;
            try
            {
                RatePlanViewList = (from r in ratePlans
                                    join s in services on r.ServiceId equals s.Id
                                    join c in currencys on r.FromCurrencyId equals c.Id
                                    join c1 in currencys on r.ToCurrencyId equals c1.Id
                                    select new RatePlanViewModel
                                    {
                                        Id = r.Id,
                                        ServiceName = s.Name,
                                        FromCurrencyName = c.CurrencyName,
                                        ToCurrencyName = c1.CurrencyName,
                                        ServiceCharge = r.ServiceCharge,
                                        ServiceChargeIsPercentage = r.ServiceChargeIsPercentage,
                                        ConvertionRate = r.ConvertionRate,
                                        OtherCharge = r.OtherCharge,
                                        CreatedDate = r.CreatedDate,
                                        CreatedBy = r.CreatedBy,
                                        IsActive = r.IsActive,
                                        MRP = r.MRP,
                                        MRPisPercentage = r.MRPisPercentage,
                                        Cost = r.Cost
                                    }).ToList();


            }
            catch (Exception)
            {

            }
            
            return RatePlanViewList;
        }

        public RatePlan FindById(int? Id)
        {
            try
            {
                ratePlan = db.RatePlans.Where(x => x.Id == Id).Select(x => x).SingleOrDefault();
                return ratePlan;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public RatePlanViewModel GetRatePlanVMById(int? Id)
        {
            IQueryable<RatePlan> ratePlans = GetAllQueryable();
            IQueryable<Currency> currencys = currency_repo.GetAllQueryable();
            IQueryable<Service> services = service_repo.GetAllQueryable().Where(s => s.IsActive == true);

            RatePlanViewModel ratePlanVM = null;

            try
            {
                ratePlanVM = (from r in ratePlans
                              join s in services on r.ServiceId equals s.Id
                              join c in currencys on r.FromCurrencyId equals c.Id
                              join c2 in currencys on r.ToCurrencyId equals c2.Id
                                where r.Id == Id
                                select new RatePlanViewModel
                                {
                                    Id = r.Id,
                                    ServiceName = s.Name,
                                    FromCurrencyName = c.CurrencyName,
                                    ToCurrencyName = c2.CurrencyName,
                                    ServiceCharge = r.ServiceCharge,
                                    ConvertionRate = r.ConvertionRate,
                                    OtherCharge = r.OtherCharge,
                                    CreatedDate = r.CreatedDate,
                                    CreatedBy = r.CreatedBy,
                                    IsActive = r.IsActive,
                                    MRP = r.MRP,
                                    MRPisPercentage = r.MRPisPercentage,
                                    Cost = r.Cost
                                }).SingleOrDefault();

                //return RatePlanView;
            }catch(Exception){ }

            return ratePlanVM;
        }

        public RatePlan FindByName(string Name)
        {
            try
            {
                //ratePlan = db.RatePlans.Where(x => x. == Name && x.IsActive == true).Select(x => x).SingleOrDefault();
                //return ratePlan;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }
        public RatePlan FindByService(int serviceId)
        {
            try
            {
                ratePlan = db.RatePlans.Where(r => r.ServiceId == serviceId && r.IsActive).SingleOrDefault();
                return ratePlan;
            }
            catch (Exception)
            {

            }
            return null;
        }
        public bool Add(RatePlan entity)
        {
            try
            {
                db.RatePlans.Add(entity);
                Save();
                return true;
            }
            catch (Exception)
            {

            }

            return false;
        }

        public RatePlan Delete(int Id)
        {
            try
            {
                ratePlan = FindById(Id);
               
                if (ratePlan != null)
                {
                    db.RatePlans.Remove(ratePlan);
                    Save();
                    return ratePlan;
                }
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }

            return null;

        }

        public bool Edit(RatePlan entity)
        {
            try
            {
                db.Entry(entity).State = EntityState.Modified;
                Save();
                return true;
            }
            catch (Exception) { }

            return false;
        }

        public void InActiveRatePlan(int serviceId)
        {
            try
            {
                IEnumerable<RatePlan> list = GetAll().Where(x => x.ServiceId == serviceId && x.IsActive==true).Select(x => x).ToList();

                foreach (var item in list)
                {
                    try
                    {
                        item.IsActive = false;
                        db.Entry(item).State = EntityState.Modified;
                        Save();
                    }
                    catch(Exception){}
                
                }

            }
            catch (Exception)
            { 
            
            }
        }

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
    }
}