using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eBanking.Models;

namespace eBanking.Interface
{
    public interface IAuditRepository
    {
        IEnumerable<AuditViewModel> AuditSearch(string IPAddress, string UserName, DateTime? FromDate, DateTime? ToDate);

        bool Add(Audit model);
        void Save();
    }
}
