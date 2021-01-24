using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eBanking.Models;
using eBanking.Interface;
using System.Data.Entity;
using PagedList;
using System.Web.Mvc;
using eBanking.App_Code;

using eBanking.Abstract;


using System.Net;


namespace eBanking.Abstract
{
    public class PinRepository : IPinRepository
    {
        private eBankingDbContext db;
        private IEnumerable<Pin> pinList;
        //private IQueryable<Pin> pinListQueryable;
        private IPagedList<Pin> pagedPinList;
        private Pin pin;

        private ICurrencyRepository currency_repo;
        private IStatusRepository status_repo;
        private IUserRoleRepository user_role_repo;
        private ITransactionRepository transactin_repo;
        private IDistributorRepository distributor_repo;
        private IServiceRepository service_repo;
        public PinRepository()
        {
            this.db = new eBankingDbContext();
            currency_repo = new CurrencyRepository(db);
            user_role_repo = new UserRoleRepository(db);
            status_repo = new StatusRepository(db);
            transactin_repo = new TransactionRepository(db);
            distributor_repo = new DistributorRepository(db);
            service_repo = new ServiceRepository(db);
        }
        public PinRepository(eBankingDbContext context)
        {
            this.db = context;
            currency_repo = new CurrencyRepository(db);
            user_role_repo = new UserRoleRepository(db);
            status_repo = new StatusRepository(db);
            transactin_repo = new TransactionRepository(db);
            distributor_repo = new DistributorRepository(db);
            service_repo = new ServiceRepository(db);
        }

        public IEnumerable<Pin> GetAll()
        {
            try
            {
                pinList = db.Pins.ToList();

                //(x=>x.IsActive==true)
                return pinList;

            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public IQueryable<Pin> GetAllQueryable()
        {
            try
            {
                return db.Pins;
            }
            catch (Exception) { }
            return null;
        }

        //Created on 8th september, 2015 by Siddique to implement pagedList
        public IPagedList<Pin> GetPagedList(int pageNumber, int pageSize)
        {
            try
            {
                pagedPinList = db.Pins.OrderBy(p => p.Id).ToPagedList(pageNumber, pageSize);
            }
            catch (Exception) { }
            return pagedPinList;
        }
        public Pin FindById(int? Id)
        {
            try
            {
                pin = db.Pins.Where(x => x.Id == Id).Select(x => x).SingleOrDefault();
                return pin;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public Pin FindByPinCode(string pinCode)
        {
            try
            {
                pin = GetAll().Where(x => x.PinCode == pinCode).Select(x => x).SingleOrDefault();
                return pin;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public Pin GetActivePinByPinCode(string pinCode)
        {
            try
            {
                return db.Pins.Where(p => p.PinCode == pinCode && p.IsActive == true).SingleOrDefault();
            }
            catch (Exception) { }
            return null;
        }

        //public string GetTopUpValueFromPinTopUP(string PinValue, string CountryCode)
        //{
        //    try
        //    {
        //        return db.PinTopUps.Where(p => p.PinValue.Equals(PinValue) && p.CountryCode.Equals(CountryCode)).FirstOrDefault().TopUpValue;
        //    }
        //    catch (Exception)
        //    {

        //    }
        //    return null;
        //}

        public Pin FindBySerialNo(string serialNo)
        {
            try
            {
                //pin = GetAll().Where(x => x.SerialNo == serialNo).Select(x => x).SingleOrDefault();
                string[] split = serialNo.Split('-');
                pin = GetAllQueryable().Where(p => p.PinPrefix.Contains(split[0]) && p.SerialNo == Convert.ToInt64(split[1])).FirstOrDefault();

                return pin;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }

        public IEnumerable<eBankingUser> GetAllUserOfaRole(string RoleName)
        {
            IEnumerable<eBankingUser> userRoles = null;
            try
            {
                userRoles = user_role_repo.GetAllUserOfaRole(RoleName);
            }
            catch (Exception) { }
            return userRoles;
        }

        public bool Add(Pin entity)
        {
            try
            {
                db.Pins.Add(entity);

                Save();
                return true;
            }
            catch (Exception)
            {

            }

            return false;
        }


        /*---------------------------------------------------------
        *  get the pin single entity of given pin Id 
        *  if pin has value then delete this entity and return deleted entity 
        *  if got exception or no pin entity found by given pin Id then return null
        *---------------------------------------------------------*/
        public Pin Delete(int Id)
        {
            try
            {
                pin = FindById(Id);

                if (pin != null)
                {
                    db.Pins.Remove(pin);

                    Save();
                    return pin;
                }
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }

            return null;

        }

        public bool Edit(Pin entity)
        {
            try
            {
                db.Entry(entity).State = EntityState.Modified;
                Save();
                return true;
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }

            return false;
        }

        public void Save()
        {
            db.SaveChanges();
        }

        public IPagedList<PinViewModel> GetAllPinAsPinVM(int pageNumber, int pageSize)
        {
            IPagedList<PinViewModel> pinVM = null;
            IQueryable<Pin> tPins = GetAllQueryable();
            IQueryable<Currency> tCurr = currency_repo.GetAllQueryable();
            IQueryable<StatusMsg> tStat = status_repo.GetAllQueryable();
            IQueryable<Service> tServices = service_repo.GetAllQueryable();
            try
            {
                pinVM = (from p in tPins
                         join c in tCurr on p.CurrencyID equals c.Id into currencyJoin
                         join s in tStat on p.Status equals s.Id into statusJoin
                         join sv in tServices on p.ServiceId equals sv.Id into serviceJoined
                         from svj in serviceJoined.DefaultIfEmpty()
                         from cj in currencyJoin.DefaultIfEmpty()
                         from sj in statusJoin.DefaultIfEmpty()
                         //from rj in resellerJoin.DefaultIfEmpty()
                         select new PinViewModel
                         {
                             Id = p.Id,
                             BatchNo = p.BatchNumber,
                             SerialNo = p.PinPrefix + "-"+ p.SerialNo,
                             PinCode = p.PinCode,
                             CurrencyName = (cj == null ? String.Empty : cj.CurrencyName),
                             Value = p.Value,
                             CreationDate = p.CreationDate,
                             Status = (sj == null ? String.Empty : sj.Name),
                             DistributorCode = p.DistributorCode,//(rj == null ? String.Empty : rj.UserName),
                             IsActive = p.IsActive,
                             ServiceId = p.ServiceId,
                             ServiceName = (svj == null ? "" : svj.Name)
                         }).OrderBy(p => p.Id).ToPagedList(pageNumber, pageSize);
            }
            catch (Exception) { }

            return pinVM;
        }
        public IPagedList<PinViewModel> PinSearch(string Prefix, int? SerialNoFrom, int? SerialNoTo, string batchNumber, string PinCode, decimal? Value, string AssignedTo, string UsedBy, int? Status, bool? IsActive, DateTime? FromDate, DateTime? ToDate, int pageNumber, int pageSize)
        {

            IPagedList<PinViewModel> pinVM = null;
            IQueryable<Pin> tPins = db.Pins;
            IQueryable<Currency> tCurr = currency_repo.GetAllQueryable();
            IQueryable<StatusMsg> tStat = status_repo.GetAllQueryable();
            IQueryable<Distributor> dUser = distributor_repo.Distributor_GetAllQueryable();
            IQueryable<Service> tServices = service_repo.GetAllQueryable();
            //   IQueryable<Transaction> tr = transactin_repo.GetAllQueryable();

            if (!string.IsNullOrEmpty(batchNumber) && batchNumber != null)
            {
                tPins = GetAllQueryable().Where(p => p.BatchNumber.Contains(batchNumber));
            }
            if (!string.IsNullOrEmpty(Prefix) && SerialNoFrom > 0 && SerialNoTo > 0)
            {
                tPins = tPins.Where(p => p.PinPrefix.Contains(Prefix) && p.SerialNo >= SerialNoFrom && p.SerialNo <= SerialNoTo && p.DistributorCode != null);
            }
            if (!string.IsNullOrEmpty(PinCode) && PinCode != null)
            {
                tPins = tPins.Where(p => p.PinCode.Contains(PinCode));
            }
            if (Value > 0)
            {
                tPins = tPins.Where(p => p.Value == Value);
            }
            if (!string.IsNullOrEmpty(AssignedTo) && AssignedTo != null)
            {
                tPins = tPins.Where(p => p.DistributorCode.Contains(AssignedTo));
            }

            if (!string.IsNullOrEmpty(UsedBy) && UsedBy != null)
            {
                tPins = tPins.Where(a => a.UsedBy.Contains(UsedBy));
            }
            if (Status != null && Status > 0)
                tPins = tPins.Where(p => p.Status == Status);
            if (IsActive != null)
                tPins = tPins.Where(p => p.IsActive == IsActive);

            if (FromDate != null && FromDate > DateTime.MinValue)
                tPins = tPins.Where(p => p.CreationDate >= FromDate);
            if (ToDate != null && ToDate > DateTime.MinValue)
                tPins = tPins.Where(p => p.CreationDate <= ToDate);

            try
            {
                pinVM = (from p in tPins
                         join c in tCurr on p.CurrencyID equals c.Id into currencyJoin
                         join s in tStat on p.Status equals s.Id into statusJoin

                         join u in dUser on p.DistributorCode equals u.DistributorCode into distributorJoin
                         //   join t in tr on p.Id equals t.PinId into transactionJoin
                         join sv in tServices on p.ServiceId equals sv.Id into serviceJoined
                         from svj in serviceJoined.DefaultIfEmpty()
                         from cj in currencyJoin.DefaultIfEmpty()
                         from sj in statusJoin.DefaultIfEmpty()
                         from dj in distributorJoin.DefaultIfEmpty()
                         select new
                         {   
                             p.Id,
                             p.BatchNumber,
                             p.PinPrefix,
                             p.SerialNo,
                             p.PinCode,
                             cj.CurrencyName,
                             p.Value,
                             p.CreationDate,
                             sj.Name,
                             DestributorUserName = dj.UserName,
                             p.DistributorCode,
                             p.IsActive,
                             p.UsedBy,
                             p.ExecutionDate,
                             p.ServiceId,
                             ServiceName = (svj == null ? "" : svj.Name)
                         }).AsEnumerable().Select(pins => new PinViewModel
                         {
                             Id = pins.Id,
                             BatchNo = pins.BatchNumber,
                             SerialNo = pins.PinPrefix + "-" + pins.SerialNo.ToString("D11"),
                             PinCode = pins.PinCode,
                             CurrencyName = pins.CurrencyName,
                             Value = pins.Value,
                             CreationDate = pins.CreationDate,
                             Status = pins.Name,
                             DistributorCode = pins.DistributorCode,
                             DistributorUserName = pins.DestributorUserName,
                             IsActive = pins.IsActive,
                             UsedBy = pins.UsedBy,
                             ExecutionDate = pins.ExecutionDate,
                             ServiceId = pins.ServiceId,
                             ServiceName = pins.ServiceName
                         }).OrderBy(p => p.Id).ToPagedList(pageNumber, pageSize);
            }
            catch (Exception) { }         

            return pinVM;
        }
        public IEnumerable<PinViewModel> ExportPins(string Prefix, int? SerialNoFrom, int? SerialNoTo, string batchNumber, string PinCode, decimal? Value, string AssignedTo, string UsedBy, int? Status, bool? IsActive, DateTime? FromDate, DateTime? ToDate, int pageNumber, int pageSize)
        {
                       
            IQueryable<Pin> tPins = db.Pins;
            IEnumerable<PinViewModel> pinVM = null;
            IQueryable<Currency> tCurr = currency_repo.GetAllQueryable();
            IQueryable<StatusMsg> tStat = status_repo.GetAllQueryable();
            IQueryable<eBankingUser> tUser = user_role_repo.GetAllUserQueryable();
            IQueryable<Service> tServices = service_repo.GetAllQueryable();

            if (!string.IsNullOrEmpty(batchNumber) && batchNumber != null)
            {
                tPins = GetAllQueryable().Where(p => p.BatchNumber.Contains(batchNumber));
            }
            if (!string.IsNullOrEmpty(Prefix) && SerialNoFrom > 0 && SerialNoTo > 0)
            {
                tPins = tPins.Where(p => p.PinPrefix.Contains(Prefix) && p.SerialNo >= SerialNoFrom && p.SerialNo <= SerialNoTo && p.DistributorCode != null);
            }
            if (!string.IsNullOrEmpty(PinCode) && PinCode != null)
            {
                tPins = tPins.Where(p => p.PinCode.Contains(PinCode));
            }
            if (Value > 0)
            {
                tPins = tPins.Where(p => p.Value == Value);
            }
            if (!string.IsNullOrEmpty(AssignedTo) && AssignedTo != null)
            {
                tPins = tPins.Where(p => p.DistributorCode.Contains(AssignedTo));
            }

            if (!string.IsNullOrEmpty(UsedBy) && UsedBy != null)
            {
                tPins = tPins.Where(a => a.UsedBy.Contains(UsedBy));
            }
            if (Status != null && Status > 0)
                tPins = tPins.Where(p => p.Status == Status);
            if (IsActive != null)
                tPins = tPins.Where(p => p.IsActive == IsActive);

            if (FromDate != null && FromDate > DateTime.MinValue)
                tPins = tPins.Where(p => p.CreationDate >= FromDate);
            if (ToDate != null && ToDate > DateTime.MinValue)
                tPins = tPins.Where(p => p.CreationDate <= ToDate);

            //try
            //{
            //    pinVM = (from p in tPins                         
            //            ).OrderBy(p => p.Id);
            //}
            //catch (Exception) { }         
            try
            {
                pinVM = (from p in tPins
                         join c in tCurr on p.CurrencyID equals c.Id into currencyJoin
                         join s in tStat on p.Status equals s.Id into statusJoin

                         join u in tUser on p.DistributorCode equals u.Id into resellerJoin
                         //   join t in tr on p.Id equals t.PinId into transactionJoin
                         join sv in tServices on p.ServiceId equals sv.Id into serviceJoined
                         from svj in serviceJoined.DefaultIfEmpty()
                         //ServiceId = p.ServiceId,
                         //                                ServiceName = (sj == null ? "" : sj.Name)

                         from cj in currencyJoin.DefaultIfEmpty()
                         from sj in statusJoin.DefaultIfEmpty()
                         select new
                         {
                             p.Id,
                             p.BatchNumber,
                             p.PinPrefix,
                             p.SerialNo,
                             p.PinCode,
                             cj.CurrencyName,
                             p.Value,
                             p.CreationDate,
                             sj.Name,
                             DestributorUserName = p.DistributorCode,
                             p.IsActive,
                             p.UsedBy,
                             p.ServiceId,
                             ServiceName = (svj == null ? "" : svj.Name)
                         }).AsEnumerable().Select(pins => new PinViewModel
                         {
                             Id = pins.Id,
                             BatchNo = pins.BatchNumber,
                             SerialNo = pins.PinPrefix + "-" + pins.SerialNo.ToString("D11"),
                             PinCode = pins.PinCode,
                             CurrencyName = pins.CurrencyName,
                             Value = pins.Value,
                             CreationDate = pins.CreationDate,
                             Status = pins.Name,
                             DistributorCode = pins.DestributorUserName,
                             IsActive = pins.IsActive,
                             UsedBy = pins.UsedBy,
                             ServiceId = pins.ServiceId,
                             ServiceName = pins.ServiceName
                         });
            }
            catch (Exception) { }
            return pinVM;
        }
        public PinViewModelTotal Search(int pageNumber, int pageSize, string Prefix, long? SerialNo, long? SerialNoTo, string PinCode, string BatchNo, int? Status, DateTime? FromDate, DateTime? ToDate, string DistributorCode, bool? PinsInHand)
        {
            PinViewModelTotal result = new PinViewModelTotal();

            IQueryable<Pin> tPins = GetAllQueryable();
            IQueryable<Pin> tPinsOwn = GetAllQueryable();
            IQueryable<Currency> tCurr = currency_repo.GetAllQueryable();
            IQueryable<StatusMsg> tStat = status_repo.GetAllQueryable();
            IQueryable<eBankingUser> tUser = user_role_repo.GetAllUserQueryable();
            IQueryable<Service> tServices = service_repo.GetAllQueryable();


            if (!string.IsNullOrEmpty(Prefix))
                tPins = tPins.Where(p => p.PinPrefix == Prefix);

            if (SerialNoTo != null && SerialNo != null)
                tPins = tPins.Where(p => p.SerialNo >= SerialNo && p.SerialNo <= SerialNoTo);
            else if (SerialNo != null)
                tPins = tPins.Where(p => p.SerialNo == SerialNo);
            
            
            if (!string.IsNullOrEmpty(BatchNo))
                tPins = tPins.Where(p => p.BatchNumber == BatchNo);

            if (!string.IsNullOrEmpty(PinCode))
                tPins = tPins.Where(p => p.PinCode.Contains(PinCode));
            if (FromDate != null && FromDate > DateTime.MinValue)
                tPins = tPins.Where(p => p.ExecutionDate >= FromDate);
            if (ToDate != null && ToDate > DateTime.MinValue)
                tPins = tPins.Where(p => p.ExecutionDate <= ToDate);

            if (Status != null && Status > 0)
                tPins = tPins.Where(p => p.Status == Status);
            
            if (!string.IsNullOrEmpty(DistributorCode))
            {
                tPinsOwn = tPins.Where(p => p.DistributorCode == DistributorCode);
                if (PinsInHand != null && PinsInHand == true)
                    tPins = tPinsOwn;
                else
                    //tPins = tPins.Where(p => p.DistributorCode.StartsWith(DistributorCode));
                    tPins = tPins.Where(p => p.DistributorCode.Equals(DistributorCode));
                 
            }
            
            
            try
            {
                result.PinViewModel = (from p in tPins 
                                      join c in tCurr on p.CurrencyID equals c.Id
                                      join s in tStat on p.Status equals s.Id
                                      //join u in tUser on p.DestributorUserName equals u.Id
                                      join sv in tServices on p.ServiceId equals sv.Id into serviceJoined
                                      from sj in serviceJoined.DefaultIfEmpty()

                                      select new PinViewModel
                                      {
                                          Id = p.Id,
                                          BatchNo = p.BatchNumber,
                                          SerialNo = p.PinPrefix + "-" + p.SerialNo,
                                          //p.SerialNo,
                                          PinCode = p.PinCode,
                                          CurrencyName = c.CurrencyName,
                                          Value = p.Value,
                                          CreationDate = p.CreationDate,
                                          ExecutionDate = p.ExecutionDate,
                                          Status = s.Name,
                                          DistributorCode = p.DistributorCode,//u.UserName,
                                          IsActive = p.IsActive,
                                          ServiceId = p.ServiceId,
                                          ServiceName = (sj == null ? "" : sj.Name)
                                      }).OrderBy(p => p.Id).ToPagedList(pageNumber, pageSize);
                tPinsOwn = tPinsOwn.Where(tp => tp.IsActive == true && tp.Status == ConstMessage.PIN_UN_USED_ID);
                //result.TotalPins = tPinsOwn.Count();
                ////result.TotalPinValue = 0;
                //result.TotalPinValue = (from p in tPinsOwn
                //                        select p.Value).Sum();
                //result.PinIdList = (from p in tPinsOwn
                //                    select p.Id).ToList();
                
            }
            catch (Exception) { }

            return result;
        }
        public PinViewModel GetPinVMById(int? pinId)
        {
            IQueryable<Pin> tPin = db.Pins.Where(p => p.Id == pinId);
            IQueryable<Currency> tCurr = currency_repo.GetAllQueryable();
            IQueryable<StatusMsg> tStat = status_repo.GetAllQueryable();
            IQueryable<Service> tServices = service_repo.GetAllQueryable();
            if (tPin != null && tCurr != null && pinId != null)
            {
                try
                {
                    var PinViewModel = (from p in tPin
                                        join s in tStat on p.Status equals s.Id
                                        join c in tCurr on p.CurrencyID equals c.Id
                                        join sv in tServices on p.ServiceId equals sv.Id into serviceJoined
                                        from sj in serviceJoined.DefaultIfEmpty()
                                        select new PinViewModel
                                        {
                                            Id = p.Id,
                                            SerialNo = p.PinPrefix + "-" + p.SerialNo,
                                            PinCode = p.PinCode,
                                            CurrencyName = c.CurrencyName,
                                            Value = p.Value,
                                            CreationDate = p.CreationDate,
                                            Status = s.Name,
                                            ServiceId = p.ServiceId,
                                            ServiceName = (sj == null ? "" : sj.Name)

                                        }).SingleOrDefault();


                    return PinViewModel;
                }
                catch (Exception)
                {

                }
            }
            return null;
        }

        //public SelectList CurrencySelectList(int? selected)
        //{
        //    SelectList currencySelectList = null;

        //    try
        //    {
        //        if(selected == null)
        //            currencySelectList = new SelectList(currency_repo.GetAllQueryable().Where(c => c.DestinationId == ConstMessage.SELECTED_USD_DESTINATION_ID).ToList(), "Id", "CurrencyName", ConstMessage.SELECTED_USD_DESTINATION_ID);
        //        else
        //            currencySelectList = new SelectList(currency_repo.GetAllQueryable().ToList(), "Id", "CurrencyName", selected);
        //    }
        //    catch (Exception) { }

        //    return currencySelectList;
        //}

        //public SelectList ResellerSelectList()
        //{
        //    SelectList resellerSelectList = null;

        //    try
        //    {
        //        resellerSelectList = new SelectList(user_role_repo.GetAllUserOfaRole(ConstMessage.RESELLER_USER_ROLE_NAME).ToList(), "Id", "UserName");
        //    }
        //    catch (Exception) { }

        //    return resellerSelectList;
        //}

        public bool PinReactivate(int PinId)
        {
            try
            {
                var pin = FindById(PinId);

                pin.Status = ConstMessage.PIN_UN_USED_ID;
                pin.ExecutionDate = DateTime.Now;

                return Edit(pin);
            }
            catch (Exception) { }
            return false;
        }

        public bool PinChangeStatus(int PinId, string UserName, int Status) 
        {
            try
            {
                var pin = FindById(PinId);
                pin.Status = Status;//ConstMessage.STATUS_PROCESSING_ID
                pin.UsedBy = UserName;
                pin.ExecutionDate = DateTime.Now;

                return Edit(pin);
            }
            catch (Exception) { }
            return false;
        }
        public int? PinService(string PinCode)
        {
            try
            {
                var validPin = GetAllQueryable().Where(p => p.PinCode.Contains(PinCode) && p.IsActive == true && p.Status == ConstMessage.PIN_UN_USED_ID).SingleOrDefault();
                if (validPin != null)
                    return validPin.ServiceId;
            }
            catch (Exception) { }
            return null;
        }

        public bool PinHistory_Add(PinHistory entity)
        {
            try
            {
                db.PinHistories.Add(entity);
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public IEnumerable<PinAssignViewModel> AssignPinSummery(string DistributorCode)//, decimal? Value, int? Qty
        {
            IEnumerable<PinAssignViewModel> results = null;
            try
            {
                ServiceRepository service_repo = new ServiceRepository(db);
                var Pins = db.Pins.Where(p => p.DistributorCode.Equals(DistributorCode) && p.IsActive == ConstMessage.PIN_ISACTIVE_STATUS_ID && p.Status == ConstMessage.PIN_UN_USED_ID);
                //if (Value != null)
                //    Pins = Pins.Where(p=>p.Value == Value);
                //if (Qty != null)
                //    Pins = Pins.Take(Qty);
                results = (from r in Pins
                           group r by new { r.ServiceId, r.Value} into groupedPins
                           join services in service_repo.GetAllQueryable() on groupedPins.Key.ServiceId equals services.Id
                           select new PinAssignViewModel
                           {
                               AvailableQuantity = groupedPins.Count(),
                               ServiceName = services.Name,
                               TotalValue = groupedPins.Sum(s => s.Value),
                               ServiceId = (int)(groupedPins.FirstOrDefault().ServiceId ?? 0),
                               UnitValue = groupedPins.FirstOrDefault().Value
                           }).ToList();
            }
            catch (Exception) { }
            return results;
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