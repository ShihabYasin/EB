using eBanking.Models;
using System.Collections.Generic;
using System.Linq;

namespace eBanking.Interface
{
   public interface IStatusRepository
    {
        IEnumerable<StatusMsg> GetAll();
        IQueryable<StatusMsg> GetAllQueryable();

        StatusMsg FindById(int? Id);

        StatusMsg FindByName(string Name);
        bool Add(StatusMsg entity);
        StatusMsg Delete(int Id);
        bool Edit(StatusMsg entity);
        void Save();
    }
}
