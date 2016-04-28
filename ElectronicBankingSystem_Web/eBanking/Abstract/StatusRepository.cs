using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eBanking.Models;
using eBanking.Interface;
using System.Data.Entity;

namespace eBanking.Abstract
{
    public class StatusRepository:IStatusRepository
    {

        private eBankingDbContext db;
        private IEnumerable<StatusMsg> StatusList;
        private StatusMsg status;

        public StatusRepository()
        {
            this.db = new eBankingDbContext();
        }
        public StatusRepository(eBankingDbContext context)
        {
            this.db = context;
        }
        public IEnumerable<StatusMsg> GetAll()
        {
            try
            {
                StatusList = db.StatusMsgs.ToList();
                return StatusList;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public IQueryable<StatusMsg> GetAllQueryable()
        {
            try
            {
                return db.StatusMsgs;
            }
            catch (Exception) { }
            return null;
        }

        public StatusMsg FindById(int? Id)
        {
            try
            {
                status = db.StatusMsgs.Where(x => x.Id == Id).Select(x => x).SingleOrDefault();
                return status;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public StatusMsg FindByName(string Name)
        {
            try
            {
                status = db.StatusMsgs.Where(x => x.Name == Name).Select(x => x).SingleOrDefault();
                return status;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public bool Add(StatusMsg entity)
        {
            try
            {
                db.StatusMsgs.Add(entity);
                Save();
                return true;
            }
            catch (Exception)
            {

            }

            return false;
        }

        public StatusMsg Delete(int Id)
        {
            try
            {
                status = FindById(Id);
               
                if (status != null)
                {
                    db.StatusMsgs.Remove(status);
                    Save();
                    return status;
                }
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }

            return null;

        }

        public bool Edit(StatusMsg entity)
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