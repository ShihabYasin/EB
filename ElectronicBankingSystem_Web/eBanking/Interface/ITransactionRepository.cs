using eBanking.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eBanking.Interface
{

  public interface ITransactionRepository
    {
        IEnumerable<Client> GetClient();
        IQueryable<Client> GetClientsQueryable();
        IEnumerable<Transaction> GetAll();
        IQueryable<Transaction> GetAllQueryable();

        //Created on 8th september, 2015 by Siddique to implement pagedList
        IPagedList<Transaction> GetPagedList(int pageNumber, int pageSize);

        //IEnumerable<Transaction> Search(IEnumerable<Service> ServiceList, string User, string Recipient, DateTime? FromDate, DateTime? ToDate, int? Destination, int? Status, int? _ServiceId, IEnumerable<Pin> pinList, int? ClientId);
        IPagedList<TransactionHistory> TransactionHistory(string User, string Recipient, DateTime? FromDate, DateTime? ToDate, int? Destination, int? Status, int? Service, string PinCode, int? ClientId, int pageNumber, int pageSize);
        IPagedList<Transaction> SearchPaged(int? page_Number, int? page_Size, string UserNumber, int? Status, int? _ServiceId, int? ParentServiceId, DateTime? FromDate, DateTime? ToDate);
        string VendorUniqueIdGenerator(string VendorName);
        Transaction FindById(int? Id);

        Transaction FindByName(string Name);
        //IEnumerable<SMSResponse> GetTopUpStatusForSMS(IEnumerable<SMSResponse> models);
        IEnumerable<Transaction> GetTransactionStatusByOrderNumberList(List<string> orderNumbers);
        bool Add(Transaction entity);
        bool Return_User_Balance(Transaction entity);
        Transaction Delete(int Id);
        bool Edit(Transaction entity);

        string OperationNumberGenarator(string prefix,string date);
        decimal? UserBalanceUpdate(string UserName, decimal Balance);

        bool DistributorCommisionReturned(int model, decimal commission);
        void Save();

    }
}
