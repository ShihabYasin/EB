using System;
using System.Collections.Generic;
using System.Linq;
using eBanking.Models;
using eBanking.Interface;
using System.Data.Entity;
using eBanking.App_Code;
using PagedList;

namespace eBanking.Abstract
{
    public class TransactionRepository : ITransactionRepository
    {
        private eBankingDbContext db;

        private IEnumerable<Transaction> TransactionList; //, Tran_temp2;
        private IPagedList<Transaction> TransactionPagedList;
        private Transaction transaction;
        private Sequencer sequencer;

        private Variable _variable = new Variable();

        public TransactionRepository()
        {
            this.db = new eBankingDbContext();
        }
        public TransactionRepository(eBankingDbContext context)
        {
            this.db = context;
        }

        public IEnumerable<Transaction> GetAll()
        {
            try
            {
                TransactionList = db.Transactions.ToList();
                return TransactionList;
            }
            catch (Exception)
            {
                //To do log file why not 
            }

            return null;
        }
        public IEnumerable<Client> GetClient()
        {
            try
            {
                var ClientList = db.Clients.Where(x => x.IsActive.Equals(true));
                return ClientList;
            }
            catch (Exception)
            {

            }
            return null;
        }
        public IQueryable<Client> GetClientsQueryable()
        {
            try
            {
                return db.Clients;
            }
            catch (Exception) { }
            return null;
        }

        public IQueryable<Transaction> GetAllQueryable()
        {
            try
            {
                return db.Transactions;
            }
            catch (Exception) { }
            return null;
        }

        //Created on 8th september, 2015 by Siddique to implement pagedList
        public IPagedList<Transaction> GetPagedList(int pageNumber, int pageSize)
        {
            try
            {
                TransactionPagedList = db.Transactions.ToPagedList(pageNumber, pageSize);
            }
            catch (Exception) { }
            return TransactionPagedList;
        }

        //public IEnumerable<Transaction> Search(IEnumerable<Service> ServiceList, string User, string Recipient, DateTime? FromDate, DateTime? ToDate, int? Destination, int? Status, int? _ServiceId, IEnumerable<Pin> pinList, int? ClientId)
        //{
        //    IPinRepository pin_repo = new PinRepository();
        //    IEnumerable<Pin> pins = pin_repo.GetAll();
        //    IEnumerable<Transaction> Tran_temp = null;
        //    IEnumerable<Transaction> Tran_temp2 = null; // new List<Transaction>();
        //    TransactionList = GetAll();

        //    DateTime today = DateTime.Today;
        //    try
        //    {
        //        if (FromDate == null && ToDate == null && string.IsNullOrEmpty(User) && string.IsNullOrEmpty(Recipient) && Destination == null && Status == null && _ServiceId == null && ClientId == null)
        //        {
        //            TransactionList = TransactionList.Where(x => x.TransactionDate >= today);
        //        }
        //        else
        //        {
        //            if (FromDate != null && FromDate != DateTime.MinValue)
        //            {
        //                TransactionList = TransactionList.Where(x => x.TransactionDate.Value.Date >= FromDate.Value.Date);
        //            }
        //            if (ToDate != null && ToDate != DateTime.MinValue)
        //            {
        //                TransactionList = TransactionList.Where(x => x.TransactionDate.Value.Date <= ToDate.Value.Date);
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(User) && User != null)
        //        {
        //            TransactionList = TransactionList.Where(x => x.UserId.Contains(User));
        //        }
        //        if (!string.IsNullOrEmpty(Recipient) && Recipient != null)
        //        {
        //            TransactionList = TransactionList.Where(x => x.ToUser.Contains(Recipient));
        //        }

        //        if (Destination > 0)
        //        {
        //            TransactionList = TransactionList.Where(x => x.FromCurrencyId.Equals(Destination));
        //        }
        //        if (Status > 0)
        //        {
        //            TransactionList = TransactionList.Where(x => x.Status.Equals(Status));
        //        }
        //        if (ClientId > 0)
        //            TransactionList = TransactionList.Where(x => x.ClientId.Equals(ClientId));

        //        if (_ServiceId > 0)
        //        {
        //            Tran_temp2 = TransactionList.Where(x => x.ServiceId.Equals(_ServiceId)).ToList();                    

        //            if(ServiceList.Where(x=>x.Id.Equals(_ServiceId)).SingleOrDefault().IsGroup)
        //            {
        //                var GroupServices = ServiceList.Where(x => x.ParentId.Equals(_ServiceId));
        //                var t = new Transaction();
        //              //  int i = 0;
        //                foreach( var item in GroupServices)
        //                {

        //                    Tran_temp = TransactionList.Where(x => x.ServiceId == item.Id);
        //                    Tran_temp2 = Tran_temp2.Concat(Tran_temp);
        //                    //foreach(var tran in Tran_temp)
        //                    //{
        //                    //    t.AmountIN = Tran_temp.SingleOrDefault().Id;
        //                    //    t.UserId = Tran_temp.SingleOrDefault().UserId;
        //                    //    t.OperationNumber = Tran_temp.SingleOrDefault().OperationNumber;
        //                    //    t.ServiceId = Tran_temp.SingleOrDefault().ServiceId;
        //                    //    t.FromCurrencyId =Tran_temp.SingleOrDefault().FromCurrencyId;
        //                    //    t.AmountIN = Tran_temp.SingleOrDefault().AmountIN;
        //                    //    t.AmountOut = Tran_temp.SingleOrDefault().AmountOut;
        //                    //    t.UserBalance = Tran_temp.SingleOrDefault().UserBalance;
        //                    //    t.ConversationRate =Tran_temp.SingleOrDefault().ConversationRate;
        //                    //    t.Status = Tran_temp.SingleOrDefault().Status;
        //                    //    t.PinId = Tran_temp.SingleOrDefault().PinId;
        //                    //    t.TransactionDate = Tran_temp.SingleOrDefault().TransactionDate;
        //                    //    t.RatePlanId = Tran_temp.SingleOrDefault().RatePlanId;
        //                    //    t.ToUser =Tran_temp.SingleOrDefault().ToUser;
        //                    //    t.InsertedAmount = Tran_temp.SingleOrDefault().InsertedAmount;
        //                    //    t.FromUser = Tran_temp.SingleOrDefault().FromUser;
        //                    //    t.UpdateDate = Tran_temp.SingleOrDefault().UpdateDate;
        //                    //    t.Remarks = Tran_temp.SingleOrDefault().Remarks;
        //                    //    t.ReferenceId = Tran_temp.SingleOrDefault().ReferenceId;
        //                    //    t.TimeOut = Tran_temp.SingleOrDefault().TimeOut;
        //                    //    t.IsTimeOut = Tran_temp.SingleOrDefault().IsTimeOut;
        //                    //    t.ClientId = Tran_temp.SingleOrDefault().ClientId;
        //                    //    t.VendorId = Tran_temp.SingleOrDefault().VendorId;
        //                    //    t.ReturnCost = Tran_temp.SingleOrDefault().ReturnCost;
        //                    //}

        //                    //if (Tran_temp != null)
        //                    //{
        //                    //    if(i==0)
        //                    //    {
        //                    //        Tran_temp2.Add(Tran_temp.ToList());
        //                    //        i = 1;
        //                    //    }                                
        //                    //}
        //                    //else
        //                    //    Tran_temp2.Add(Tran_temp);
        //                    //Tran_temp2.Add(t);
        //                }
        //                //TransactionList = Tran_temp2;
        //                //TransactionList = TransactionList.Where(x => x.ServiceId == _ServiceId || x.ServiceId.gra);
        //            }
        //            TransactionList = Tran_temp2;
        //            //else
        //            //    TransactionList = TransactionList.Where(x => x.ServiceId == _ServiceId);
        //        }


        //        var temp = TransactionList.OrderBy(t => t.Id).ToList();
        //        return temp;
        //    }
        //    catch (Exception)
        //    {

        //    }
        //    //count = TransactionList.Count();
        //    return TransactionList;
        //}

        public IPagedList<TransactionHistory> TransactionHistory(string User, string Recipient, DateTime? FromDate, DateTime? ToDate, int? Destination, int? Status, int? Service, string PinCode, int? ClientId, int pageNumber, int pageSize)
        {
            //#region db instances and query filters

            PinRepository pin_repo = new PinRepository(db);
            ServiceRepository service_repo = new ServiceRepository(db);
            StatusRepository stat_repo = new StatusRepository(db);
            DestinationRepository dest_repo = new DestinationRepository(db);
            IQueryable<Transaction> transactions = GetAllQueryable();
            IQueryable<Pin> pinList = pin_repo.GetAllQueryable();
            IQueryable<Service> service = service_repo.GetAllQueryable();
            IQueryable<StatusMsg> statusmsg = stat_repo.GetAllQueryable();
            IQueryable<Client> ClientList = GetClientsQueryable();
            IQueryable<Destination> destination = dest_repo.GetAllQueryable();

            IPagedList<TransactionHistory> transactionHistory = null;
            IQueryable<TransactionHistory> tempTranHistory = null;

            if (string.IsNullOrEmpty(PinCode))
            {

                if (!string.IsNullOrEmpty(User))
                {
                    transactions = transactions.Where(t => t.UserId.Contains(User));
                }
                if (!string.IsNullOrEmpty(Recipient))
                {
                    transactions = transactions.Where(t => t.ToUser.Contains(Recipient));
                }
                if (FromDate != null && FromDate > DateTime.MinValue)
                {
                    transactions = transactions.Where(t => t.TransactionDate >= FromDate);
                }
                if (ToDate != null && ToDate > DateTime.MinValue)
                {
                    TimeSpan ts = new TimeSpan(23, 59, 59);
                    ToDate += ts;
                    transactions = transactions.Where(t => t.TransactionDate <= ToDate);
                }
                if (Destination != null)
                {
                    transactions = transactions.Where(t => t.FromCurrencyId == Destination);
                }
                if (Status != null)
                {
                    transactions = transactions.Where(t => t.Status == Status);
                }
                if (Service != null)
                {
                    transactions = transactions.Where(t => t.ServiceId == Service);
                }
                if (ClientId != null)
                {
                    transactions = transactions.Where(t => t.ClientId == ClientId);
                }





                //Pin dummyPin = new Pin();
                try
                {
                    tempTranHistory = (from tran in transactions
                                       join serv in service on tran.ServiceId equals serv.Id// into servicejoin
                                       join status in statusmsg on tran.Status equals status.Id// into statusjoin
                                       join pin in pinList on tran.PinId equals pin.Id into pinjoin
                                       join client in ClientList on tran.ClientId equals client.ClientId
                                       join dest in destination on tran.FromCurrencyId equals dest.Id into destJoin

                                       //from statusj in servicejoin.DefaultIfEmpty()
                                       from pinj in pinjoin.DefaultIfEmpty()
                                       from destj in destJoin.DefaultIfEmpty()
                                       select new
                                       {
                                           tran.Id,
                                           tran.UserId,
                                           tran.OperationNumber,
                                           tran.ServiceId,
                                           PinCode = (pinj.PinCode ?? ""),
                                           //pinj.Id,
                                           serviceName = serv.Name,
                                           tran.FromCurrencyId,
                                           tran.AmountIN,
                                           tran.AmountOut,
                                           tran.ConversationRate,
                                           tran.Status,
                                           statusName = status.Name,
                                           //tran.PinId,
                                           tran.TransactionDate,
                                           tran.RatePlanId,
                                           tran.FromUser,
                                           tran.ToUser,
                                           tran.InsertedAmount,
                                           tran.ReferenceId,
                                           tran.Remarks,
                                           tran.UpdateDate,
                                           tran.TimeOut,
                                           tran.IsTimeOut,
                                           tran.ClientId,
                                           client.Name,
                                           destj.DestinationName
                                       }).Select(Transactions => new TransactionHistory
                                       {
                                           Id = Transactions.Id,
                                           UserId = Transactions.UserId,
                                           OperationNumber = Transactions.OperationNumber,
                                           ServiceId = Transactions.ServiceId,
                                           PinCode = Transactions.PinCode,
                                           ServiceName = Transactions.serviceName,
                                           FromCurrencyId = Transactions.FromCurrencyId,

                                           CountryName = Transactions.DestinationName,//((Transactions.FromCurrencyId != null && Transactions.FromCurrencyId > 0) ? (from dest in destination where dest.Id == Transactions.FromCurrencyId select dest.DestinationName).FirstOrDefault() : ""),

                                           AmountIN = Transactions.AmountIN,
                                           AmountOut = Transactions.AmountOut,
                                           ConversationRate = Transactions.ConversationRate,
                                           Status = Transactions.Status,
                                           StatusName = Transactions.statusName,
                                           //PinId = Transactions.PinId,
                                           TransactionDate = Transactions.TransactionDate,
                                           RatePlanId = Transactions.RatePlanId,
                                           FromUser = Transactions.FromUser,
                                           ToUser = Transactions.ToUser,
                                           InsertedAmount = Transactions.InsertedAmount,
                                           ReferenceId = Transactions.ReferenceId,
                                           Remarks = Transactions.Remarks,
                                           UpdateDate = Transactions.UpdateDate,
                                           TimeOut = Transactions.TimeOut,
                                           IsTimeOut = Transactions.IsTimeOut,
                                           ClientId = Transactions.ClientId,
                                           ClientName = Transactions.Name
                                       });
                }
                catch (Exception)
                { }
            }
            else
            {
                #region pin search only
                try
                {
                    pinList = pinList.Where(p => p.PinCode.Contains(PinCode));
                    tempTranHistory = (
                                       from pin in pinList// on tran.PinId equals pin.Id into pinjoin
                                       join trans in transactions on pin.Id equals trans.PinId into tranJoin

                                       from tran in tranJoin.DefaultIfEmpty()

                                       join serv in service on tran.ServiceId equals serv.Id// into servicejoin
                                       join status in statusmsg on tran.Status equals status.Id// into statusjoin
                                       join client in ClientList on tran.ClientId equals client.ClientId
                                       join dest in destination on tran.FromCurrencyId equals dest.Id into destJoin
                                       //from statusj in servicejoin.DefaultIfEmpty()
                                       //from pinj in pinjoin.DefaultIfEmpty()
                                       from destj in destJoin.DefaultIfEmpty()
                                       select new
                                       {
                                           tran.Id,
                                           tran.UserId,
                                           tran.OperationNumber,
                                           tran.ServiceId,
                                           PinCode = (pin.PinCode ?? ""),
                                           //pinj.Id,
                                           serviceName = serv.Name,
                                           tran.FromCurrencyId,
                                           tran.AmountIN,
                                           tran.AmountOut,
                                           tran.ConversationRate,
                                           tran.Status,
                                           statusName = status.Name,
                                           //tran.PinId,
                                           tran.TransactionDate,
                                           tran.RatePlanId,
                                           tran.FromUser,
                                           tran.ToUser,
                                           tran.InsertedAmount,
                                           tran.ReferenceId,
                                           tran.Remarks,
                                           tran.UpdateDate,
                                           tran.TimeOut,
                                           tran.IsTimeOut,
                                           tran.ClientId,
                                           client.Name,
                                           destj.DestinationName
                                       }).Select(Transactions => new TransactionHistory
                                       {
                                           Id = Transactions.Id,
                                           UserId = Transactions.UserId,
                                           OperationNumber = Transactions.OperationNumber,
                                           ServiceId = Transactions.ServiceId,
                                           PinCode = Transactions.PinCode,
                                           ServiceName = Transactions.serviceName,
                                           FromCurrencyId = Transactions.FromCurrencyId,

                                           CountryName = Transactions.DestinationName,//((Transactions.FromCurrencyId != null && Transactions.FromCurrencyId > 0) ? (from dest in destination where dest.Id == Transactions.FromCurrencyId select dest.DestinationName).FirstOrDefault() : ""),

                                           AmountIN = Transactions.AmountIN,
                                           AmountOut = Transactions.AmountOut,
                                           ConversationRate = Transactions.ConversationRate,
                                           Status = Transactions.Status,
                                           StatusName = Transactions.statusName,
                                           //PinId = Transactions.PinId,
                                           TransactionDate = Transactions.TransactionDate,
                                           RatePlanId = Transactions.RatePlanId,
                                           FromUser = Transactions.FromUser,
                                           ToUser = Transactions.ToUser,
                                           InsertedAmount = Transactions.InsertedAmount,
                                           ReferenceId = Transactions.ReferenceId,
                                           Remarks = Transactions.Remarks,
                                           UpdateDate = Transactions.UpdateDate,
                                           TimeOut = Transactions.TimeOut,
                                           IsTimeOut = Transactions.IsTimeOut,
                                           ClientId = Transactions.ClientId,
                                           ClientName = Transactions.Name
                                       });
                }
                catch (Exception)
                { }
                #endregion

            }

            //var tempTest1 = tempTranHistory.ToList();
            //if (!string.IsNullOrEmpty(PinCode))
            //{
            //    tempTranHistory = tempTranHistory.Where(p => p.PinCode.Contains(PinCode));
            //}
            //var tempTest2 = tempTranHistory.ToList();
            transactionHistory = tempTranHistory.OrderByDescending(t => t.Id).ToPagedList(pageNumber, pageSize);


            return transactionHistory;
        }

        public IPagedList<Transaction> SearchPaged(int? page_Number, int? page_Size, string UserNumber, int? Status, int? _ServiceId, int? ParentServiceId, DateTime? FromDate, DateTime? ToDate)
        {
            int pageSize = (page_Size ?? 20);
            int pageNumber = (page_Number ?? 1);
            IServiceRepository service_repo = new ServiceRepository(db);
            try
            {
                //eBankingDbContext db = new eBankingDbContext();
                IQueryable<Transaction> queryableList = GetAllQueryable();
                IQueryable<Service> serviceList = service_repo.GetAllQueryable();
                //TransactionList = from t in db.Transactions
                //select t;                              //db.Transactions.ToList();

                if (!string.IsNullOrEmpty(UserNumber) && UserNumber != null)
                {
                    queryableList = queryableList.Where(x => x.UserId.Contains(UserNumber));
                }

                if (Status == null)
                    queryableList = queryableList.Where(x => x.Status == 1 || x.Status == 20);
                else if (Status != null && Status > 0)
                    queryableList = queryableList.Where(x => x.Status == Status);

                if (_ServiceId != null && _ServiceId > 0)
                {
                    queryableList = queryableList.Where(x => x.ServiceId == _ServiceId);
                }
                if (ParentServiceId != null)
                {
                    serviceList = serviceList.Where(s => s.ParentId == ParentServiceId);
                    queryableList = (from q in queryableList
                                     join s in serviceList on q.ServiceId equals s.Id
                                     select q);
                }
                if (FromDate != null && FromDate != DateTime.MinValue)
                {
                    queryableList = queryableList.Where(x => x.TransactionDate >= FromDate);
                }
                if (ToDate != null && ToDate != DateTime.MinValue)
                {
                    queryableList = queryableList.Where(x => x.TransactionDate <= ToDate);
                }
                var temp = queryableList.OrderBy(t => t.Id).ToPagedList(pageNumber, pageSize);
                return queryableList.OrderBy(t => t.Id).ToPagedList(pageNumber, pageSize);
            }
            catch (Exception)
            {

            }
            return null;
        }


        //public IEnumerable<SMSResponse> GetTopUpStatusForSMS(IEnumerable<SMSResponse> models)
        //{
        //    Transaction tempResponse = new Transaction();
        //    List<SMSResponse> result = new List<SMSResponse>();
        //    SMSResponse temp;
        //    foreach(var item in models)
        //    {
        //        temp = new SMSResponse();
        //        try
        //        {
        //            tempResponse = db.Transactions.Where(t => t.OperationNumber == item.OperationNumber).SingleOrDefault();
        //            temp.ApiStatus = (int)tempResponse.Status;
        //            temp.UserName = tempResponse.UserId;
        //            temp.OperationNumber = item.OperationNumber;
        //            switch (temp.ApiStatus)
        //            {
        //                case 1:
        //                    temp.MessageBody = "Top up request is pending. Please wait.";
        //                    break;
        //                case 20:
        //                case 25:
        //                    temp.MessageBody = "Top up request is processing. Please wait.";
        //                    break;
        //                case 30:
        //                    temp.MessageBody = "Tou up is successfull. User";
        //                    break;
        //                case 50:
        //                    temp.MessageBody = "Top up failed. Please try again later.";
        //                    break;
        //                default:
        //                    temp.MessageBody = "Record not found";
        //                    break;
        //            }
        //            result.Add(temp);
        //        }
        //        catch (Exception)
        //        {
        //            //temp.MessageBody = "Record not found";
        //        }
        //    }
        //    return result;
        //}

        public IEnumerable<Transaction> GetTransactionStatusByOrderNumberList(List<string> orderNumbers)
        {
            List<Transaction> transactionList = new List<Transaction>();
            foreach (var i in orderNumbers)
            {
                try
                {
                    transaction = db.Transactions.Where(t => t.OperationNumber == i).SingleOrDefault();
                }
                catch (Exception) { }
                transactionList.Add(transaction);
            }
            return transactionList;
        }
        //hear prefix is Prefix+Only Date
        public string OperationNumberGenarator(string prefix, string date)
        {
            sequencer = new Sequencer();

            _variable.AllStringVar = prefix + "-" + date;

            //check is this sequencer already exits if exists then update SqlNumber
            //or if not exists then create new one and return the SqlNumber

            try
            {

                var check = db.Sequencers.Where(x => x.Prefix == _variable.AllStringVar).SingleOrDefault();

                //already exists so Update the SqlNumber
                if (check != null)
                {
                    check.SqlNumber = check.SqlNumber + 1;
                    db.Entry(check).State = EntityState.Modified;
                    _variable.AllStringVar = _variable.AllStringVar + "-" + check.SqlNumber;
                }

                    //does not exists so create new one
                else
                {
                    sequencer.SqlNumber = 1;
                    sequencer.Prefix = _variable.AllStringVar;
                    db.Sequencers.Add(sequencer);

                    _variable.AllStringVar = _variable.AllStringVar + "-" + sequencer.SqlNumber;
                }

                Save();



            }
            catch (Exception ex)
            {
                string m = ex.Message;
                _variable.AllStringVar = null;
            }

            return _variable.AllStringVar;
        }

        public string VendorUniqueIdGenerator(string VendorName)
        {
            sequencer = new Sequencer();

            _variable.AllStringVar = VendorName;
            try
            {

                var check = db.Sequencers.Where(x => x.Prefix == _variable.AllStringVar).SingleOrDefault();

                //already exists so Update the SqlNumber
                if (check != null)
                {
                    check.SqlNumber = ++check.SqlNumber;
                    db.Entry(check).State = EntityState.Modified;
                    _variable.AllStringVar = check.SqlNumber.ToString();
                }

                    //does not exists so create new one
                else
                {
                    sequencer.SqlNumber = 1;
                    sequencer.Prefix = _variable.AllStringVar;
                    db.Sequencers.Add(sequencer);

                    _variable.AllStringVar = sequencer.SqlNumber.ToString();
                }

                Save();

            }
            catch (Exception ex)
            {
                string m = ex.Message;
                _variable.AllStringVar = null;
            }

            return _variable.AllStringVar;
        }

        public Transaction FindById(int? Id)
        {
            try
            {
                return db.Transactions.Where(t => t.Id == Id).SingleOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Transaction FindByName(string Name)
        {
            throw new NotImplementedException();
        }

        public bool Add(Transaction entity)
        {
            try
            {
                entity.TransactionDate = DateTime.Now;
                db.Transactions.Add(entity);
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public bool Return_User_Balance(Transaction entity)
        {
            //IAdminRepository admin_repo = new AdminRepository(db);
            try
            {
                transaction = new Transaction();

                //transaction = entity;
                transaction.UserBalance = UserBalanceUpdate(entity.UserId, (decimal)entity.AmountOut);
                transaction.AmountIN = entity.AmountOut;
                transaction.AmountOut = 0;
                transaction.ClientId = entity.ClientId;
                transaction.ConversationRate = entity.ConversationRate;
                transaction.FromCurrencyId = entity.FromCurrencyId;
                transaction.FromUser = entity.FromUser;
                transaction.InsertedAmount = entity.InsertedAmount;
                transaction.IsTimeOut = true;
                transaction.UserId = entity.UserId;
                transaction.VendorId = entity.VendorId;
                transaction.Remarks = "Return balance of - " + entity.OperationNumber;
                transaction.OperationNumber = OperationNumberGenarator(ConstMessage.PREFIX_USER_BALANCE_RETURN, DateTime.Now.ToString("ddMMyyyy"));
                transaction.Status = ConstMessage.STATUS_COMPLETE_ID;
                transaction.ServiceId = ConstMessage.UserBalanceReturn;
                transaction.DistributorCommission = 0;
                return Add(transaction);
            }
            catch (Exception) { }
            return false;
        }

        public decimal? UserBalanceUpdate(string UserName, decimal Amount)
        {
            try
            {
                var user = db.Users.Where(u => u.UserName == UserName).FirstOrDefault();
                user.CurrentBalance += Amount;
                db.Entry(user).State = EntityState.Modified;
                Save();
                return user.CurrentBalance;
            }
            catch (Exception) { }
            return null;
        }

        public Transaction Delete(int Id)
        {
            throw new NotImplementedException();
        }

        public bool Edit(Transaction entity)
        {
            try
            {
                entity.UpdateDate = DateTime.Now;
                db.Entry(entity).State = EntityState.Modified;
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }
        public bool DistributorCommisionReturned(int transactionId, decimal commission)
        {
            try
            {
                Transaction model = FindById(transactionId);
                model.DistributorCommission = commission;
                //model.ReturnCost = true;
                db.Entry(model).State = EntityState.Modified;
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public void Save()
        {
            try
            {
                db.SaveChanges();
            }
            catch (Exception) { }
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