using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eBanking.Models;
using eBanking.Abstract;

namespace eBanking.App_Code
{
    public static class eBankingTask
    {
        #region Private_Variable
        private static IEnumerable<PinViewModel> PinViewList = null;
        #endregion

        #region CurrencyControllerTask

        //
        //public static IEnumerable<CurrencyViewModel> detailsCurrency(IEnumerable<Currency> currency,IEnumerable<Destination> destination,string Request)
        //{
        //   IEnumerable<CurrencyViewModel> currencyViewModel = null;

        //    if (currency != null && destination != null)
        //    {
        //        try
        //        {
        //            if (Request == null)
        //            {
        //                currencyViewModel = (from c in currency
        //                                     from d in destination
        //                                     where c.DestinationId == d.Id
        //                                     select new CurrencyViewModel
        //                                     {
        //                                         Id = c.Id,
        //                                         CurrencyName = c.CurrencyName,
        //                                         DestinationName = d.DestinationName,
        //                                         ISO = c.ISO,
        //                                         Sign = c.Sign
        //                                     }).ToList();
        //            }
        //            else if(Request==ConstMessage.RequestFrmRatePlan)
        //            {
        //                currencyViewModel = (from c in currency
        //                                     from d in destination
        //                                     where c.DestinationId == d.Id
        //                                     select new CurrencyViewModel
        //                                     {
        //                                         Id = c.Id,
        //                                         DestinationName = c.ISO + " (" + c.CurrencyName + "," + d.DestinationName + ")"
        //                                     }).ToList();
        //            }

        //            return currencyViewModel;
        //        }
        //        catch (Exception)
        //        { 
                
        //        }
        //    }
        //    return null;
        //}

        //public static CurrencyViewModel SingleCurrency(IEnumerable<Currency> currency, IEnumerable<Destination> destination,int? CurrencyId)
        //{
           

        //    if (currency != null && destination != null && CurrencyId != null)
        //    {
        //        try
        //        {
                  
        //              var  currencyViewModel = (from c in currency
        //                                     from d in destination
        //                                     where c.Id == CurrencyId && c.DestinationId == d.Id
        //                                     select new CurrencyViewModel
        //                                     {

        //                                         Id = c.Id,
        //                                         DestinationName = d.DestinationName,
        //                                         CurrencyName = c.CurrencyName,
        //                                         ISO = c.ISO,
        //                                         Sign = c.Sign

        //                                     }).SingleOrDefault();

                  
        //               return currencyViewModel;
        //        }
        //        catch (Exception)
        //        {

        //        }
        //    }
        //    return null;
        //}
        #endregion

        #region ServiceControllerTask

        //public static List<ServiceViewModel> ServiceIndex(IEnumerable<Service> Services,IEnumerable<Destination> Destinations)
        //{
        //    if (Services != null && Destinations !=null)
        //    {
        //        try
        //        {                   
                 
        //            var serviceViewList = (from s in Services
        //                                  from d in Destinations
        //                                  where s.IsActive == true && s.DestinationId == d.Id
        //                                  select new ServiceViewModel { 
        //                                   Id=s.Id,
        //                                   Name=s.Name,
        //                                   Destination=d.DestinationName,
        //                                   ParentId=s.ParentId,
        //                                   ParentName=(from p in Services where  s.ParentId!=0  && p.Id==s.ParentId select p.Name).SingleOrDefault(),
        //                                   IsGroup=s.IsGroup,
        //                                   IsActive=s.IsActive                                          
        //                                  }).ToList();

        //            return serviceViewList;
        //        }
        //        catch (Exception ex)
        //        {
        //            string a = ex.Message;
        //        }
        //    }
        //    return null;
        //}

        //public static List<ServiceViewModel> ServiceIndexView(List<ServiceViewModel> items)
        //{
        //    if (items != null)
        //    {
        //        try
        //        {
        //            items.ForEach(i => i.Children = items.Where(ch => ch.ParentId == i.Id).ToList());

        //            return items.Where(i => i.ParentId == 0).ToList();
        //        }
        //        catch (Exception) 
        //        { 
                
        //        }
        //    }

        //    return null;
        //}

        //public static ServiceViewModel SingleService(IEnumerable<Service> service, IEnumerable<Destination> destination, int? serviceId)
        //{


        //    if (service != null && destination != null && serviceId != null)
        //    {
        //        try
        //        {
        //            var serviceViewModel = (from c in service
        //                                     from d in destination
        //                                     where c.Id == serviceId && c.DestinationId == d.Id
        //                                     select new ServiceViewModel
        //                                     {

        //                                         Id = c.Id,
        //                                         Name=c.Name,
        //                                         Destination=d.DestinationName,
        //                                         ParentName=(from p in service where p.Id==c.ParentId select p.Name).SingleOrDefault(),
        //                                         IsActive=c.IsActive,
        //                                         IsGroup=c.IsGroup
                                                   

        //                                     }).SingleOrDefault();


        //            return serviceViewModel;
        //        }
        //        catch (Exception)
        //        {

        //        }
        //    }
        //    return null;
        //}
        #endregion

        #region PinControllerTask
        //public static IEnumerable<PinViewModel> PinIndex(IEnumerable<Pin> Pins, IEnumerable<Currency> currencys,IEnumerable<StatusMsg> StatusMessages,IEnumerable<eBankingUser> UserList)
        //{
        //     PinViewList=null;
        //    if (Pins != null && currencys != null)
        //    {
        //        try
        //        {
                  
                
        //                PinViewList = (from p in Pins
        //                               join C in currencys on p.CurrencyID equals C.Id
        //                               join s in StatusMessages on p.Status equals s.Id
        //                             //  join user in UserList on p.ResellerUserID  equals user.Id
                                    
        //                               select new PinViewModel
        //                               {
        //                                   Id = p.Id,
        //                                   BatchNo = p.BatchNumber,
        //                                   SerialNo = p.SerialNo,
        //                                   PinCode = p.PinCode,
        //                                   CurrencyName = C.CurrencyName,
        //                                   Value = p.Value,
        //                                   CreationDate = p.CreationDate,
        //                                   Status = s.Name,
        //                                   ReselerUserId = UserList!=null?(from u in UserList where u.Id==p.ResellerUserID select u.UserName).SingleOrDefault() :"None",
        //                                   //user.UserName,  //p.ResellerUserID
        //                                   IsActive = p.IsActive
        //                               }).ToList();
                    
                    
        //        }
        //        catch (Exception)
        //        {
        //           PinViewList=null;
        //        }
        //    }
        //    return PinViewList;
        //}


        //public static PinViewModel SinglePin(IEnumerable<Pin> pins, IEnumerable<Currency> currencies,IEnumerable<StatusMsg> StatusMessages, int? pinId)
        //{


        //    if (pins != null && currencies != null && pinId != null)
        //    {
        //        try
        //        {
        //            var PinViewModel = (from p in pins
        //                                join s in StatusMessages on p.Status equals s.Id
        //                                    from c in currencies
        //                                    where p.Id == pinId && p.CurrencyID == c.Id
        //                                    select new PinViewModel
        //                                    {
        //                                        Id = p.Id,
        //                                        SerialNo=p.SerialNo,
        //                                       PinCode = p.PinCode,
        //                                       CurrencyName =c.CurrencyName,
        //                                       Value = p.Value,
        //                                       CreationDate = p.CreationDate,
        //                                       Status = s.Name
                                                
        //                                    }).SingleOrDefault();


        //            return PinViewModel;
        //        }
        //        catch (Exception)
        //        {

        //        }
        //    }
        //    return null;
        //}
     

        #endregion

        #region RatePlanController
        //public static IEnumerable<RatePlanViewModel> RatePlanIndex(IEnumerable<RatePlan> ratePlans,IEnumerable<Service> services, IEnumerable<Currency> currencys)
        //{
        //    if (ratePlans != null&& services!=null && currencys != null)
        //    {
        //        try
        //        {
        //            var RatePlanViewList = (from r in ratePlans
                                                                         
        //                               select new RatePlanViewModel
        //                               {
        //                                   Id = r.Id,
        //                                   ServiceName = (from s in services where r.ServiceId==s.Id select s.Name).SingleOrDefault(),
        //                                   FromCurrencyName =(from c in currencys where r.FromCurrencyId==c.Id select c.CurrencyName).SingleOrDefault(),
        //                                   ToCurrencyName = (from c in currencys where r.ToCurrencyId == c.Id select c.CurrencyName).SingleOrDefault(),
        //                                   ServiceCharge = r.ServiceCharge,
        //                                   ConvertionRate = r.ConvertionRate,
        //                                   CreatedDate = r.CreatedDate,
        //                                   CreatedBy = r.CreatedBy,
        //                                   IsActive = r.IsActive

        //                               }).ToList();

        //            return RatePlanViewList;
        //        }
        //        catch (Exception)
        //        {

        //        }
        //    }
        //    return null;
        //}


        //public static RatePlanViewModel SingleRatePlan(IEnumerable<RatePlan> ratePlans,IEnumerable<Service> services, IEnumerable<Currency> currencys, int? RatePlanId)
        //{


        //    if (ratePlans != null && services != null && currencys != null)
        //    {
        //        try
        //        {

        //            var RatePlanView = (from r in ratePlans
                                          
        //                                    where r.Id==RatePlanId 
        //                                    select new RatePlanViewModel
        //                                    {
        //                                        Id = r.Id,
        //                                        ServiceName = (from s in services where r.ServiceId == s.Id select s.Name).SingleOrDefault(),
        //                                        FromCurrencyName = (from c in currencys where r.FromCurrencyId == c.Id select c.CurrencyName).SingleOrDefault(),
        //                                        ToCurrencyName = (from c in currencys where r.ToCurrencyId == c.Id select c.CurrencyName).SingleOrDefault(),
        //                                        ServiceCharge = r.ServiceCharge,
        //                                        ConvertionRate = r.ConvertionRate,
        //                                        CreatedDate = r.CreatedDate,
        //                                        CreatedBy = r.CreatedBy,
        //                                        IsActive = r.IsActive

        //                                    }).SingleOrDefault();

        //            return RatePlanView;
                   
        //        }
        //        catch (Exception)
        //        {

        //        }
        //    }
        //    return null;
        //}

        //public static void InActive_RatePlan_OfThis_Service(int serviceId,IEnumerable<RatePlan> ratePlanes)
        //{
          
        
        //}

        #endregion

        #region TransactionController
        //private static IEnumerable<Transaction> transactionHistory=null;
        public static List<Transaction> TransactionReportsGenerator(IEnumerable<Transaction> transaction, IEnumerable<Service> service,IEnumerable<StatusMsg> statusmsg,IEnumerable<Destination> destination, int? FromTranHistory)
        {
            List<Transaction> transactionHistory = new List<Transaction>();            
            
            try
            {
                if (transaction != null && service != null && statusmsg != null && destination != null)
                {
                    //&& tran.FromCurrencyId == dest.Id
                    if (FromTranHistory == 1)
                    {
                        transactionHistory = (from tran in transaction
                                              join serv in service on tran.ServiceId equals serv.Id
                                              join status in statusmsg on tran.Status equals status.Id
                                              select new Transaction
                                              {
                                                  Id = tran.Id,
                                                  UserId = tran.UserId,
                                                  OperationNumber = tran.OperationNumber,
                                                  ServiceId = tran.ServiceId,
                                                  //PinCode = tran.PinCode,
                                                  ServiceName = serv.Name,
                                                  FromCurrencyId = tran.FromCurrencyId,

                                                  CountryName = tran.FromCurrencyId > 0 ? (from dest in destination where dest.Id == tran.FromCurrencyId select dest.DestinationName).SingleOrDefault() : "",

                                                  AmountIN = tran.AmountIN,
                                                  AmountOut = tran.AmountOut,
                                                  ConversationRate = tran.ConversationRate,
                                                  Status = tran.Status,
                                                  StatusName = status.Name,
                                                  PinId = tran.PinId,
                                                  TransactionDate = tran.TransactionDate,
                                                  RatePlanId = tran.RatePlanId,
                                                  FromUser = tran.FromUser,
                                                  ToUser = tran.ToUser,
                                                  InsertedAmount = tran.InsertedAmount,
                                                  ReferenceId = tran.ReferenceId,
                                                  Remarks = tran.Remarks,
                                                  UpdateDate = tran.UpdateDate,
                                                  TimeOut = tran.TimeOut,
                                                  IsTimeOut = tran.IsTimeOut

                                              }).ToList();

                    }
                    else
                    {
                        transactionHistory = (from tran in transaction
                                              join serv in service on tran.ServiceId equals serv.Id
                                              join status in statusmsg on tran.Status equals status.Id
                                              join dest in destination on tran.FromCurrencyId equals dest.Id into dest_tbl
                                              from d in dest_tbl.DefaultIfEmpty()
                                              select new Transaction
                                              {
                                                  Id = tran.Id,
                                                  UserId = tran.UserId,
                                                  OperationNumber = tran.OperationNumber,
                                                  ServiceId = tran.ServiceId,
                                                  ServiceName = serv.Name,
                                                  FromCurrencyId = tran.FromCurrencyId,
                                                  CountryName = (dest_tbl == null ? string.Empty : d.DestinationName),//tran.FromCurrencyId == 0 ? string.Empty : dest.DestinationName,//(dest_tbl == null ? string.Empty : d.DestinationName),//dest.DestinationName,
                                                  AmountIN = tran.AmountIN,
                                                  AmountOut = tran.AmountOut,
                                                  ConversationRate = tran.ConversationRate,
                                                  Status = tran.Status,
                                                  StatusName = status.Name,
                                                  PinId = tran.PinId,
                                                  TransactionDate = tran.TransactionDate,
                                                  RatePlanId = tran.RatePlanId,
                                                  FromUser = tran.FromUser,
                                                  ToUser = tran.ToUser,
                                                  InsertedAmount = serv.ParentId == ConstMessage.SERVICES_CREDIT_MANAGEMENT_ID ? serv.value : tran.InsertedAmount,
                                                  ReferenceId = tran.ReferenceId,
                                                  Remarks = tran.Remarks,
                                                  UpdateDate = tran.UpdateDate,
                                                  TimeOut = tran.TimeOut,
                                                  IsTimeOut = tran.IsTimeOut

                                              }).ToList();
                        //return tList;
                    }

                  
                }
            }
            catch (Exception) {

                transactionHistory = null;
            }

            return transactionHistory;
        }  

        // transactionHistory
        //public static IQueryable<TransactionHistory> TransactionHistory(string User, string Recipient, DateTime? FromDate, DateTime? ToDate, int? Destination, int? Status, int? Service, string PinCode, int? ClientId)
        //    //(IQueryable<Transaction> transaction, IQueryable<Service> service, IQueryable<StatusMsg> statusmsg, IQueryable<Destination> destination, int? FromTranHistory, IQueryable<Pin> pinList, string PinCode, IQueryable<Client> ClientList)
        //{
        //    #region db instances and query filters 

        //        eBankingDbContext db = new eBankingDbContext();
        //        TransactionRepository tran_repo = new TransactionRepository(db);
        //        PinRepository pin_repo = new PinRepository(db);
        //        IQueryable<Transaction> transactions = tran_repo.GetAllQueryable();
            
        //        if (!string.IsNullOrEmpty(User))
        //        {
        //            transactions = transactions.Where(t=>t.UserId.Contains(User));
        //        }
        //        if (!string.IsNullOrEmpty(Recipient))
        //        {
        //            transactions = transactions.Where(t=>t.ToUser.Contains(Recipient));
        //        }
        //        if (FromDate != null && FromDate > DateTime.MinValue)
        //        {
        //            transactions = transactions.Where(t=>t.TransactionDate.Value.Date >= FromDate.Value.Date);
        //        }
        //        if (ToDate != null && ToDate > DateTime.MinValue)
        //        {
        //            transactions = transactions.Where(t => t.TransactionDate.Value.Date <= ToDate.Value.Date);
        //        }
        //        if (Destination != null)
        //        {
        //            transactions = transactions.Where(t=>t.FromCurrencyId == Destination);
        //        }
        //        if (Status != null)
        //        {
        //            transactions = transactions.Where(t => t.Status == Status);
        //        }
        //        if (Service != null)
        //        {
        //            transactions = transactions.Where(t => t.ServiceId == Service);
        //        }
        //        if (ClientId != null)
        //        {
        //            transactions = transactions.Where(t => t.ClientId == ClientId);
        //        }


        //        IQueryable<Pin> pinList = pin_repo.GetAllQueryable();
        //        if (!string.IsNullOrEmpty(PinCode))
        //        {
        //            pinList = pinList.Where(p => p.PinCode.Contains(PinCode));
        //        }

        //        ServiceRepository service_repo = new ServiceRepository();
        //        IQueryable<Service> service = service_repo.GetAllQueryable();

        //        StatusRepository stat_repo = new StatusRepository();
        //        IQueryable<StatusMsg> statusmsg = stat_repo.GetAllQueryable();

        //        var ClientList = tran_repo.GetClientsQueryable();

        //        DestinationRepository dest_repo = new DestinationRepository();
        //        var destination = dest_repo.GetAllQueryable();
        //    #endregion


        //    IQueryable<TransactionHistory> transactionHistory = null;
        //    Pin dummyPin = new Pin();
        //    try
        //    {
        //        transactionHistory = (from tran in transactions
        //                                join serv in service on tran.ServiceId equals serv.Id// into servicejoin
        //                                join status in statusmsg on tran.Status equals status.Id// into statusjoin
        //                                join pin in pinList on tran.PinId equals pin.Id into pinjoin
        //                                join client in ClientList on tran.ClientId equals client.ClientId

        //                                //from statusj in servicejoin.DefaultIfEmpty()
        //                                from pinj in pinjoin.DefaultIfEmpty(dummyPin)
        //                                select new
        //                                {
        //                                    tran.Id,
        //                                    tran.UserId,
        //                                    tran.OperationNumber,
        //                                    tran.ServiceId,
        //                                    pinj.PinCode,
        //                                    //pinj.Id,
        //                                    serviceName = serv.Name,
        //                                    tran.FromCurrencyId,
        //                                    tran.AmountIN,
        //                                    tran.AmountOut,
        //                                    tran.ConversationRate,
        //                                    tran.Status,
        //                                    statusName = status.Name,
        //                                    //tran.PinId,
        //                                    tran.TransactionDate,
        //                                    tran.RatePlanId,
        //                                    tran.FromUser,
        //                                    tran.ToUser,
        //                                    tran.InsertedAmount,
        //                                    tran.ReferenceId,
        //                                    tran.Remarks,
        //                                    tran.UpdateDate,
        //                                    tran.TimeOut,
        //                                    tran.IsTimeOut,
        //                                    tran.ClientId,
        //                                    client.Name
        //                                }).AsQueryable().Select(Transactions => new TransactionHistory
        //                                {
        //                                    Id = Transactions.Id,
        //                                    UserId = Transactions.UserId,
        //                                    OperationNumber = Transactions.OperationNumber,
        //                                    ServiceId = Transactions.ServiceId,
        //                                    PinCode = Transactions.PinCode,
        //                                    ServiceName = Transactions.serviceName,
        //                                    FromCurrencyId = Transactions.FromCurrencyId,

        //                                    CountryName = Transactions.FromCurrencyId > 0 ? (from dest in destination where dest.Id == Transactions.FromCurrencyId select dest.DestinationName).SingleOrDefault() : "",

        //                                    AmountIN = Transactions.AmountIN,
        //                                    AmountOut = Transactions.AmountOut,
        //                                    ConversationRate = Transactions.ConversationRate,
        //                                    Status = Transactions.Status,
        //                                    StatusName = Transactions.statusName,
        //                                    //PinId = Transactions.PinId,
        //                                    TransactionDate = Transactions.TransactionDate,
        //                                    RatePlanId = Transactions.RatePlanId,
        //                                    FromUser = Transactions.FromUser,
        //                                    ToUser = Transactions.ToUser,
        //                                    InsertedAmount = Transactions.InsertedAmount,
        //                                    ReferenceId = Transactions.ReferenceId,
        //                                    Remarks = Transactions.Remarks,
        //                                    UpdateDate = Transactions.UpdateDate,
        //                                    TimeOut = Transactions.TimeOut,
        //                                    IsTimeOut = Transactions.IsTimeOut,
        //                                    ClientId = Transactions.ClientId,
        //                                    ClientName = Transactions.Name
        //                                });

        //    }
        //    catch (Exception)
        //    {

        //        transactionHistory = null;
        //    }

            
            
        //    return transactionHistory; //transactionHistory;
        //}  
        public static IEnumerable<MoneyTopUpHistory> Money_TopUp_History(IEnumerable<Transaction> transaction, IEnumerable<Service> service,IEnumerable<StatusMsg> status,string UserName)
        {
            try
            {
                if (UserName != null)
                {
                   IEnumerable<Service> services = service.Where(x => x.ParentId == ConstMessage.SERVICES_MONEYTRANSFER_ID || x.ParentId == ConstMessage.SERVICES_TOPUP_ID).Select(x => x).ToList();

                    var history = (from tran in transaction
                                   from serv in services
                                   from statusmsg in status
                                   where tran.UserId == UserName && tran.ServiceId == serv.Id && tran.Status == statusmsg.Id 
                                   select new MoneyTopUpHistory
                                   {
                                       OperationNumber = tran.OperationNumber,
                                       InsertedAmount = tran.InsertedAmount,
                                       ToUser=tran.ToUser,
                                       AmountOut = tran.AmountOut,
                                       Status = statusmsg.Name + ((tran.OperationNumber.Contains("MN-") && tran.ReferenceId != null && tran.ReferenceId.Split().Length > 2 && tran.ReferenceId.Split()[3].Length > 5) ? ", bKash Pin - " + tran.ReferenceId.Split()[3].Substring(tran.ReferenceId.Split()[3].Length - 6, 6) : ""), 
                                       TransactionDate = tran.TransactionDate.ToString(),
                                       TransactionType = (serv.ParentId == ConstMessage.SERVICES_MONEYTRANSFER_ID ? ConstMessage.SERVICES_MONEYTRANSFER_MSG:ConstMessage.SERVICES_TOPUP_MSG)

                                   }).ToList();
                    return history;

                    //The operator
                    //result= Condition?TrueResult:FalseResult
                    //TransactionType=serv.ParentId == ConstMessage.SERVICES_MONEYTRANSFER_ID? ConstMessage.SERVICES_MONEYTRANSFER_MSG:ConstMessage.SERVICES_TOPUP_MSG

                }
            }
            catch(Exception)
            {
            
            }
                return null;
        }

        
      public static IEnumerable<TransferHistory> TransferHistory_Creation(IEnumerable<Transaction> transaction, IEnumerable<Service> service, IEnumerable<Destination> destination, IEnumerable<RatePlan> ratePlan,IEnumerable<StatusMsg> status, string UserName, int? month, int? year)
        {
            try
            {
                IEnumerable<Service> services = service.Where(x => x.ParentId == ConstMessage.SERVICES_MONEYTRANSFER_ID).Select(x => x).ToList();

                //var q = (from tran in transaction
                //        where tran.TransactionDate.Value.Month.Equals(month)
                //        select tran).ToList();

                //foreach(var item in transaction)
                //{
                //    int m = item.TransactionDate.Value.Month;
                //}

                if (month >=1 && month<=12 && year>2014)
                {
                    var history = (from tran in transaction
                                   where tran.UserId == UserName && tran.TransactionDate.Value.Month.Equals(month) && tran.TransactionDate.Value.Year.Equals(year)
                                   join serv in services on tran.ServiceId equals serv.Id
                                   join dest in destination on tran.FromCurrencyId equals dest.Id
                                   join rate_plan in ratePlan on tran.RatePlanId equals rate_plan.Id
                                   join statusmsg in status on tran.Status equals statusmsg.Id

                                   select new TransferHistory
                                   {
                                       Id = tran.Id,
                                       OperationNumber = tran.OperationNumber,
                                       TransactionDate = tran.TransactionDate,
                                       MBankDestination = dest.DestinationName,
                                       BankName = serv.Name,
                                       MBankNumber = tran.ToUser,
                                       InsertedAmount = tran.InsertedAmount,
                                       ProcessingFee = rate_plan.ServiceCharge,
                                       AmountOut = tran.AmountOut,
                                       UpdateDate = tran.UpdateDate,
                                       Status =statusmsg.Name,
                                       IsTimeOut = tran.IsTimeOut

                                   }).ToList();

                    return history;

                }
                else
                {
                    var history = (from tran in transaction
                                   where tran.UserId == UserName
                                   join serv in services on tran.ServiceId equals serv.Id
                                   join dest in destination on tran.FromCurrencyId equals dest.Id
                                   join rate_plan in ratePlan on tran.RatePlanId equals rate_plan.Id
                                   join statusmsg in status on tran.Status equals statusmsg.Id
                                   select new TransferHistory
                                   {
                                       Id = tran.Id,
                                       OperationNumber = tran.OperationNumber,
                                       TransactionDate = tran.TransactionDate,
                                       MBankDestination = dest.DestinationName,
                                       BankName = serv.Name,
                                       MBankNumber = tran.ToUser,
                                       InsertedAmount = tran.InsertedAmount,
                                       ProcessingFee = rate_plan.ServiceCharge,
                                       AmountOut = tran.AmountOut,
                                       UpdateDate = tran.UpdateDate,
                                       Status = statusmsg.Name,
                                       IsTimeOut = tran.IsTimeOut

                                   }).ToList();

                    return history;
                }
            }
            catch (Exception) { }
            return null;                
        }

        #endregion

        #region TopUp

      public static IEnumerable<Destination> GetDestinationForTopUp(IEnumerable<Destination> destinations, IEnumerable<Service> services)
      {
          try
          {
              var destination = (from serv in services
                                 from dest in destinations
                                 where dest.IsActive == true && serv.DestinationId == dest.Id

                                 select dest
                                 ).Distinct().ToList();
              return destination;
          }
          catch (Exception) { }

          return null;

      }

    

        #endregion
    }
}