using eBanking.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;


namespace eBanking.Interface
{
  public  interface IPinRepository
    {
        IEnumerable<Pin> GetAll();
        IQueryable<Pin> GetAllQueryable();

        //Created on 8th september, 2015 by Siddique to implement pagedList
        IPagedList<Pin> GetPagedList(int pageNumber, int pageSize);

        Pin FindById(int? Id);
        PinViewModel GetPinVMById(int? pinId);
        Pin GetActivePinByPinCode(string pinCode);
        //string GetTopUpValueFromPinTopUP(string PinValue, string CountryCode);
        Pin FindBySerialNo(string Name);
        Pin FindByPinCode(string pinCode);
        IEnumerable<eBankingUser> GetAllUserOfaRole(string RoleName);
        IPagedList<PinViewModel> GetAllPinAsPinVM(int pageNumber, int pageSize);
        IPagedList<PinViewModel> PinSearch(string Prefix, int? SerialNoFrom, int? SerialNoTo, string batchNumber, string PinCode, decimal? Value, string AssignedTo, string UsedBy, int? Status, bool? IsActive, DateTime? FromDate, DateTime? ToDate, int pageNumber, int pageSize);
        IEnumerable<PinViewModel> ExportPins(string Prefix, int? SerialNoFrom, int? SerialNoTo, string batchNumber, string PinCode, decimal? Value, string AssignedTo, string UsedBy, int? Status, bool? IsActive, DateTime? FromDate, DateTime? ToDate, int pageNumber, int pageSize);

        PinViewModelTotal Search(int pageNumber, int pageSize, string Prefix, long? SerialNo, long? SerialNoTo, string PinCode, string BatchNo, int? Status, DateTime? FromDate, DateTime? ToDate, string DistributorCode, bool? PinsInHand);

        bool Add(Pin entity);
        int? PinService(string PinCode);
        Pin Delete(int Id);
        bool Edit(Pin entity);
        bool PinReactivate(int PinId);
        bool PinChangeStatus(int PinId, string UserName, int Status);
        IEnumerable<PinAssignViewModel> AssignPinSummery(string DistributorCode);
        bool PinHistory_Add(PinHistory entity);
        //SelectList CurrencySelectList(int? selected);
        //SelectList ResellerSelectList();
        void Save();
    }
}
