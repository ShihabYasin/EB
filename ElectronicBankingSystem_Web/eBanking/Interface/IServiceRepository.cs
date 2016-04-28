using eBanking.Models;
using System.Collections.Generic;
using System.Linq;

namespace eBanking.Interface
{

    public interface IServiceRepository
    {
        IEnumerable<Service> GetAll(bool? isActive); 
        List<ServiceViewModel> CreateTree(IEnumerable<Service> itemList, int? parentId);
        IEnumerable<ServiceViewModel> GetAllToServiceVM(bool? ActiveService, bool? ActiveDestination);
        IQueryable<Service> GetAllQueryable();
        IEnumerable<Service> GetRatePlanServices();

        Service FindById(int? Id);
        ServiceViewModel GetServiceVMbyId(int? ServiceId);

        Service FindByName(string Name);
        Service FindByDestinationAndValue(int Destination, int Value);
        bool Add(Service entity);
        Service Delete(int Id);
        bool Edit(Service entity);
               
        void Save();
        IEnumerable<Service> Search(string Name, int? DestinationId, int? ParentId, bool? IsGroup, bool? IsActive);
        IEnumerable<ServiceCommonViewModel> GetServicesForApi(string DistributorCode);
    }
}
