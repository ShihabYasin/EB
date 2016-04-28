using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eBanking.Models;
using eBanking.Interface;
using System.Data.Entity;

namespace eBanking.Abstract
{
    public class DestinationRepository:IDestinationRepository
    {


        private eBankingDbContext db;
        private IEnumerable<Destination> Destinationlist;
        //private IServiceRepository service_repo;
        //private IQueryable<Destination> DestinationListQueryable;
        private Destination destination;

        public DestinationRepository()
        {
            this.db = new eBankingDbContext();
            //service_repo = new ServiceRepository(this.db);
        }
        public DestinationRepository(eBankingDbContext context)
        {
            this.db = context;
            //service_repo = new ServiceRepository(this.db);

           
        }
        
        public IEnumerable<Destination> GetAll()
        {
            try
            {
                Destinationlist = db.Destinations.ToList().Where(x=>x.IsActive.Equals(true));   //.Where(x=>x.IsActive==true)
                return Destinationlist;
            }
            catch (Exception)
            { 
              //To do log file why not 
            }

            return null;
        }
        
        public IQueryable<Destination> GetAllQueryable()
        {
            try
            {
                return db.Destinations;
            }
            catch (Exception) { }
            return null;
        }

        public Destination FindById(int? Id)
        {
            try
            {
                destination = db.Destinations.Where(x=>x.Id==Id).Select(x=>x).SingleOrDefault();
                return destination;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public Destination FindByName(string Name)
        {
            try
            {
                destination = db.Destinations.Where(x => x.DestinationName == Name).Select(x => x).SingleOrDefault();
                return destination;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public Destination FindByCountryCode(String CountryCode)
        {
            try
            {
                destination = db.Destinations.Where(x=>x.CountryCode == CountryCode).SingleOrDefault();
                return destination;
            }
            catch (Exception) { }
            return null;
        }
        /*
         * implementation of this created an infinite loop between destinationRepo and serviceRepo because of the constructors
         */

        //public IEnumerable<Destination> GetDestinationForTopUp()
        //{
        //    Destinationlist = GetAllQueryable();
        //    try
        //    {
        //        IQueryable<Service> activeService = service_repo.GetAllQueryable().Where(s => s.IsActive == true);
        //        Destinationlist = (from serv in activeService
        //                           where serv.IsActive == true
        //                           from dest in Destinationlist
        //                           where dest.IsActive == true && serv.DestinationId == dest.Id
        //                           select dest
        //                         ).Distinct().ToList();
        //    }
        //    catch (Exception) { }
        //    return Destinationlist;
        //}
        public bool Add(Destination entity)
        {
            try
            {
                db.Destinations.Add(entity);
                Save();
                return true;
            }
            catch (Exception)
            { 

            }

            return false;
        }

        public Destination Delete(int Id)
        {
            try
            {
                 destination =FindById(Id);
              
                if (destination != null)
                {
                    db.Destinations.Remove(destination);
                    Save();
                    return destination;
                }
            }
            catch (Exception ex) 
            {
                string a = ex.Message;
            }

            return null;
          
        }

        public bool Edit(Destination entity)
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