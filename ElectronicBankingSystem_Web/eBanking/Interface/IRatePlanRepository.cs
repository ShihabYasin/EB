using eBanking.Models;
using System.Collections.Generic;
using System.Linq;

namespace eBanking.Interface
{
   public interface IRatePlanRepository
    {
        IEnumerable<RatePlan> GetAll();
        IQueryable<RatePlan> GetAllQueryable();
        IEnumerable<RatePlanViewModel> GetAlltoRatePlanVM();
        RatePlan FindById(int? Id);
        RatePlanViewModel GetRatePlanVMById(int? Id);

        RatePlan FindByName(string Name);
        RatePlan FindByService(int serviceId);
        bool Add(RatePlan entity);
        RatePlan Delete(int Id);
        bool Edit(RatePlan entity);

        void InActiveRatePlan(int serviceId);
        void Save();
    }
}
