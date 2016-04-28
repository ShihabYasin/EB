using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eBanking.Models;
using eBanking.Interface;
using eBanking.App_Code;
using System.Data.Entity;
using System.Collections;


namespace eBanking.Abstract
{
    public class ServiceRepository:IServiceRepository
    {

        private eBankingDbContext db;
        private IEnumerable<Service> ServiceList;
        private Service service;

        //private DestinationRepository destination_repo;


        public ServiceRepository()
        {
            this.db = new eBankingDbContext();
            //destination_repo = new DestinationRepository(this.db);
        }
        public ServiceRepository(eBankingDbContext context)
        {
            this.db = context;
            //destination_repo = new DestinationRepository(this.db);
        }

        public List<ServiceViewModel> CreateTree(IEnumerable<Service> itemList, int? parentId)
        {
            List<ServiceViewModel> treeList = new List<ServiceViewModel>();
            var itemListWithParentOrChild = itemList;
            int count = 0;
            if(parentId == 0)
            {
                itemListWithParentOrChild = itemList.Where(i => i.ParentId == parentId).ToList();
            }
            else
            {
                itemListWithParentOrChild = itemList.Where(i => (i.Id == parentId) || (i.ParentId == parentId)).ToList().OrderBy( i => i.value);
                count = 1;
            }                       
            string parentName = "";
            try
            {               
                    parentName = itemList.Where(i => i.Id == parentId).SingleOrDefault().Name;                      
            }
            catch (Exception)
            {
            }
            foreach (var i in itemListWithParentOrChild)
            {
                if (i.IsGroup && count == 0)
                {
                    var attachment = CreateTree(itemList, i.Id);
                    treeList.AddRange(attachment);
                }
                else
                {
                    ServiceViewModel o = new ServiceViewModel();
                    o.Id = i.Id;
                    o.Name = i.Name;
                    o.ParentName = parentName;
                    treeList.Add(o);
                }
            }
            return treeList;
        }

        public IEnumerable<Service> GetAll(bool? isActive)
        {
            try
            {
                if (isActive != null)
                    ServiceList = db.Services.Where(x => x.IsActive == isActive).ToList();
                else
                    ServiceList = db.Services.ToList();
                return ServiceList;
            }
            catch (Exception ex)
            {
                string a = ex.Message;
                //To do log file why not 
            }

            return null;
        }
        public IEnumerable<ServiceViewModel> GetAllToServiceVM(bool? ActiveService, bool? ActiveDestination)
        {
            DestinationRepository destination_repo = new DestinationRepository(db);
            IEnumerable<ServiceViewModel> serviceVMList = null;

            IQueryable<Service> Services = GetAllQueryable();
            if (ActiveService != null)
                Services = Services.Where(s=>s.IsActive == ActiveService);
            IQueryable<Destination> Destinations = destination_repo.GetAllQueryable();
            if (ActiveDestination != null)
                Destinations = Destinations.Where(d=>d.IsActive == ActiveDestination);

            try
            {
                serviceVMList = (from s in Services
                                 join p in Services on s.ParentId equals p.Id into parentTable
                                 join d in Destinations on s.DestinationId equals d.Id
                                 from parent in parentTable.DefaultIfEmpty()
                                 //where s.IsActive == true //&& s.ParentId == p.Id

                                 select new ServiceViewModel
                                 {
                                     Id = s.Id,
                                     Name = s.Name,
                                     Destination = d.DestinationName,
                                     ParentId = s.ParentId,
                                     ParentName = (parent == null ? String.Empty : parent.Name),
                                     IsGroup = s.IsGroup,
                                     IsActive = s.IsActive
                                 }).ToList();
            }
            catch (Exception) { }

            return serviceVMList;
        }

        public IQueryable<Service> GetAllQueryable()
        {
            try
            {
                return db.Services;
            }
            catch (Exception) { }
            return null;
        }
        public Service FindById(int? Id)
        {
            try
            {
                service = db.Services.Where(x => x.Id == Id).SingleOrDefault();
                return service;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public ServiceViewModel GetServiceVMbyId(int? ServiceId)
        {
            DestinationRepository destination_repo = new DestinationRepository(db);
            ServiceViewModel serviceVM = null;
            IQueryable<Service> Services = GetAllQueryable().Where(s => s.IsActive == true && s.Id == ServiceId);
            IQueryable<Destination> Destinations = destination_repo.GetAllQueryable();
            try
            {
                var serviceViewModel = (from c in Services
                                        join d in Destinations on c.DestinationId equals d.Id
                                        join p in Services on c.ParentId equals p.Id
                                        select new ServiceViewModel
                                        {
                                            Id = c.Id,
                                            Name = c.Name,
                                            Destination = d.DestinationName,
                                            ParentName = p.Name,
                                            IsActive = c.IsActive,
                                            IsGroup = c.IsGroup
                                        }).SingleOrDefault();
                
            }
            catch (Exception) { }

            return serviceVM;
        }

        public Service FindByName(string Name)
        {
            try
            {
                service = db.Services.Where(x => x.Name == Name && x.IsActive==true).Select(x => x).SingleOrDefault();
                return service;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public Service FindByDestinationAndValue(int Destination, int Value)
        {
            try
            {
                service = db.Services.Where(x => x.DestinationId == Destination && x.value == Value).SingleOrDefault();
                return service;
            }
            catch(Exception)
            {

            }
            return null;
        }

        public IEnumerable<Service> GetRatePlanServices()
        {
            try
            {
                ServiceList = db.Services.Where(x => (new[] { ConstMessage.SERVICES_MONEYTRANSFER_ID, ConstMessage.SERVICES_TOPUP_ID }).Contains(x.ParentId)).Select(x=>x).ToList();
                
                return ServiceList;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public bool Add(Service entity)
        {
            try
            {
                db.Services.Add(entity);
                Save();
                return true;
            }
            catch (Exception)
            {

            }

            return false;
        }

        public Service Delete(int Id)
        {
            try
            {
                service = FindById(Id);
               
                if (service != null)
                {
                    db.Services.Remove(service);
                    Save();
                    return service;
                }
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }

            return null;

        }

        public bool Edit(Service entity)
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

        public IEnumerable<Service> Search(string Name, int? DestinationId, int? ParentId, bool? IsGroup, bool? IsActive)
        {
            try
            {
                IEnumerable<Service> result = null;
                IQueryable<Service> query = GetAllQueryable();

                if (!string.IsNullOrEmpty(Name))
                    query = query.Where(q => q.Name == Name);
                if (DestinationId != null)
                    query = query.Where(q => q.DestinationId == DestinationId);
                if (ParentId != null)
                    query = query.Where(q => q.ParentId == ParentId);
                if (IsGroup != null)
                    query = query.Where(q => q.IsGroup == IsGroup);
                if (IsActive != null)
                    query = query.Where(q => q.IsActive == IsActive);

                result = query.ToList();

                return result;
            }
            catch (Exception) { }
            return null;
            
        }

        public IEnumerable<ServiceCommonViewModel> GetServicesForApi(string DistributorCode)
        {
            IRatePlanRepository rateplan_repo = new RateplanRepository(db);
            IDestinationRepository destination_repo = new DestinationRepository(db);
            IDistributorRepository distributor_repo = new DistributorRepository(db);
            IEnumerable<ServiceCommonViewModel> services = null;
            try
            {
                
                var allServices = GetAllQueryable().Where(s => s.IsActive == true);
                var allRateplans = rateplan_repo.GetAllQueryable().Where(r=>r.IsActive == true);
                var allDestinations = destination_repo.GetAllQueryable();
                //var lastDistributorId = distributor_repo.GetDistributorIdFromDistributorCode(DistributorCode);
                //var tempAssignedServices = distributor_repo.DCRP_GetAssignedToDCRP(lastDistributorId).Select(d => d.ServiceId);
                services = (//from availableServices in tempAssignedServices
                            from svs in allServices// on availableServices equals svs.Id
                            join rps in allRateplans on svs.Id equals rps.ServiceId into svsWithRps
                            join dest in allDestinations on svs.DestinationId equals dest.Id
                            from swr in svsWithRps.DefaultIfEmpty()
                            //where svsWithRps != null
                            select new ServiceCommonViewModel 
                            {
                                ServiceId = svs.Id,
                                ServiceParentId = svs.ParentId,
                                ServiceName = svs.Name,
                                ServiceValue = svs.value,
                                IsGroup = svs.IsGroup,
                                
                                RatePlanId = swr.Id,
                                MRP = swr.MRP,
                                MRPIsPercentage = swr.MRPisPercentage,
                                OtherCharge = swr.OtherCharge,
                                DestinationId = svs.DestinationId,
                                DestinationName = dest.DestinationName,
                                ToUSD = swr.ConvertionRate,
                                
                                ServiceChargeDescrete = ((swr.ServiceChargeIsPercentage == null || swr.ServiceChargeIsPercentage == false) ? swr.ServiceCharge : 0),
                                ServiceChargePercentage = ((swr.ServiceChargeIsPercentage != null && swr.ServiceChargeIsPercentage == true) ? swr.ServiceCharge : 0)
                                

                            }).OrderBy(s=>s.ServiceId).ToList();
                foreach (var item in services)
                {
                    var temp = distributor_repo.DCRP_GetDistributorServiceCharge(item.ServiceId, DistributorCode);
                    if (temp != null)
                    {
                        if (temp.IsPercentage == true && temp.ServiceCharge != null)
                        {
                            if (item.ServiceChargePercentage == null)
                                item.ServiceChargePercentage = 0;
                            item.ServiceChargePercentage += temp.ServiceCharge;
                        }
                        else if (temp.ServiceCharge != null)
                        {
                            if (item.ServiceChargeDescrete == null)
                                item.ServiceChargeDescrete = 0;
                            item.ServiceChargeDescrete += temp.ServiceCharge;
                        }
                            
                    }

                }
            }
            catch (Exception) { }
            return services;
        }
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