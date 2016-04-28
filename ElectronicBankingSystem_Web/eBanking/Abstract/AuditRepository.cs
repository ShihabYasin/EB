using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using eBanking.Models;
using eBanking.Interface;

namespace eBanking.Abstract
{
    public class AuditRepository : IAuditRepository
    {
        private eBankingDbContext db = new eBankingDbContext();
        public IEnumerable<AuditViewModel> AuditSearch(string IPAddress, string UserName, DateTime? FromDate, DateTime? ToDate)
        {
            IEnumerable<AuditViewModel> auditRrecords = null;
                IQueryable<Audit> audits = db.Audits;
                if (!string.IsNullOrEmpty(IPAddress))
                {
                    audits = audits.Where(x => x.IPAddress.Contains(IPAddress));
                }

                if (!string.IsNullOrEmpty(UserName) && UserName != null)
                {
                    audits = audits.Where(x => x.UserName.Contains(UserName));
                }
                if (FromDate != null)
                {
                    audits = audits.Where(x => x.TimeAccessed >= FromDate.Value.Date);
                }
                if (ToDate != null)
                {
                    audits = audits.Where(x => x.TimeAccessed <= ToDate.Value.Date);
                }

                auditRrecords = (from audit in audits
                                 select new AuditViewModel
                                 {
                                     AuditID = audit.AuditID,
                                     UserName = audit.UserName,
                                     URLAccessed = audit.URLAccessed,
                                     IPAddress = audit.IPAddress,
                                     TimeAccessed = audit.TimeAccessed
                                 }).OrderByDescending( x => x.TimeAccessed).ToList();
                return auditRrecords;            
        }

        public bool Add(Audit model)
        {
            try
            {
                db.Audits.Add(model);

                Save();
                return true;
            }
            catch (Exception)
            {

            }

            return false;
        }
        public void Save()
        {
            db.SaveChanges();
        }
    }
}