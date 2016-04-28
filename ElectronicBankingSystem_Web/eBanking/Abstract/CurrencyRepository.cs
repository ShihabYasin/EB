using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using eBanking.Interface;
using eBanking.Models;
using System.Data.Entity;
using eBanking.App_Code;
using System.Web.Mvc;
namespace eBanking.Abstract
{
    public class CurrencyRepository:ICurrencyRepository
    {
        private eBankingDbContext db;
        private IEnumerable<Currency> currencylist;
        //private IQueryable<Currency> currencyListQueryable;
        private Currency currencyModel;
        private ICurrencyRepository currency_repo;
        private IDestinationRepository destination_repo;
        public CurrencyRepository()
        {
            this.db = new eBankingDbContext();
            this.destination_repo = new DestinationRepository(db);
            this.currency_repo = this;
        }
        public CurrencyRepository(eBankingDbContext context)
        {
            this.db = context;
            this.destination_repo = new DestinationRepository(db);
            this.currency_repo = this;
           
        }

        public IEnumerable<Currency> GetAll()
        {
            try
            {
                currencylist = db.Currencies.ToList();
                return currencylist;
            }
            catch (Exception)
            { 
              //To do log file why not 
            }

            return null;
        }
        public IQueryable<Currency> GetAllQueryable()
        {
            try
            {
                return db.Currencies;
            }
            catch (Exception) { }
            return null;
        }

        public Currency FindById(int? Id)
        {
            try
            {
                currencyModel = db.Currencies.Where(x=>x.Id==Id).Select(x=>x).SingleOrDefault();
                return currencyModel;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public Currency FindByName(string Name)
        {
            try
            {
                currencyModel = db.Currencies.Where(x => x.CurrencyName == Name).Select(x => x).SingleOrDefault();
                return currencyModel;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }
        public Currency FindByDestinationID(int DestId)
        {
            try
            {
                currencyModel = db.Currencies.Where(x => x.DestinationId == DestId).Select(x => x).SingleOrDefault();
                return currencyModel;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        
        }

        public CurrencyViewModel GetCurrencyVMById(int? Id)
        {
            try
            {
                IQueryable<Currency> currencyQuerable = db.Currencies.Where(c=>c.Id == Id);
                IQueryable<Destination> destination = destination_repo.GetAllQueryable().Where(x => x.IsActive == true);
                CurrencyViewModel CurrencyVM = (from c in currencyQuerable
                                                from d in destination
                                                where d.Id == c.DestinationId
                                                select new CurrencyViewModel
                                                {
                                                    Id = c.Id,
                                                    CurrencyName = c.CurrencyName,
                                                    DestinationName = d.DestinationName,
                                                    ISO = c.ISO,
                                                    Sign = c.Sign
                                                }).SingleOrDefault();
                return CurrencyVM;
            }
            catch (Exception) { }
            return null;
        }
        public bool Add(Currency entity)
        {
            try
            {
                db.Currencies.Add(entity);
                Save();
                return true;
            }
            catch (Exception)
            { 

            }

            return false;
        }

        public Currency Delete(int Id)
        {
            try
            {
               currencyModel =FindById(Id);
             
                if (currencyModel != null)
                {
                    db.Currencies.Remove(currencyModel);
                    Save();
                    return currencyModel;
                }
            }
            catch (Exception ex) 
            {
                string a = ex.Message;
            }

            return null;
          
        }

        public decimal ConvertToLocal(decimal Amount, int FromCurrencyId)
        {
            try
            {
                if(FromCurrencyId == 0)
                    return (Amount * db.Currencies.Find(14).ConvertFromUsd);
                return (Amount * db.Currencies.Find(FromCurrencyId).ConvertFromUsd);
            }
            catch (Exception) { }
            return 0;
        }
        public decimal GetConversionRate(int CurrencyId)
        {
            try
            {
                return db.Currencies.Find(CurrencyId).ConvertFromUsd;
            }
            catch (Exception) { }
            return 0;
        }
        public bool Edit(Currency entity)
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

        public void Save()
        {
            db.SaveChanges();
        }

        public IEnumerable<CurrencyViewModel> ActiveCurrency(string Request)
        {
            IEnumerable<CurrencyViewModel> currencyViewModel = null;
            IQueryable<Currency> tempCurrencyList = currency_repo.GetAllQueryable();
            //var destination = destination_repo.GetAll().Where(x => x.IsActive == true).Select(x => x).ToList();
            IQueryable<Destination> destination = destination_repo.GetAllQueryable().Where(x => x.IsActive == true);

            if (tempCurrencyList != null && destination != null)
            {
                try
                {
                    if (Request == null)
                    {
                        currencyViewModel =(    from c in tempCurrencyList
                                                from d in destination
                                                where c.DestinationId == d.Id
                                                select new CurrencyViewModel
                                                {
                                                 Id = c.Id,
                                                 CurrencyName = c.CurrencyName,
                                                 DestinationName = d.DestinationName,
                                                 ISO = c.ISO,
                                                 Sign = c.Sign
                                                }).ToList();

                            //tempCurrencyList.Where(c => destination.Any(d => d.Id == c.DestinationId)).Select( s => new CurrencyViewModel { Id = }).ToList();
                    }
                    else if (Request == ConstMessage.RequestFrmRatePlan)
                    {
                        currencyViewModel = (from c in tempCurrencyList
                                             from d in destination
                                             where c.DestinationId == d.Id
                                             select new CurrencyViewModel
                                             {
                                                 Id = c.Id,
                                                 DestinationName = c.ISO + " (" + c.CurrencyName + "," + d.DestinationName + ")"
                                             }).ToList();
                    }

                    return currencyViewModel;
                }
                catch (Exception)
                {

                }
            }
            return null;
        }

        public SelectList GetDestinationSelectList()
        {
            SelectList destList = null;
            try
            {
                destList = new SelectList(destination_repo.GetAllQueryable().Where(x => x.IsActive == true).Select(x => x).ToList(), "Id", "DestinationName");
            }
            catch (Exception) { }
            return destList;

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