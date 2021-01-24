using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eBanking.Models;

namespace eBanking.Interface
{
    public interface IVendorRepository
    {
        #region Vendor
        List<VendorTransactionMV> VendorTransaction_Search(IEnumerable<VendorTransaction> vtr, int? TransactionId, int? ServiceId, int? VendorId, int? Country, DateTime? FromDate, DateTime? ToDate, string CreatedBy, IEnumerable<Service> ServiceList, IEnumerable<Vendor> VendorList, IEnumerable<Destination> CountryList);
        IEnumerable<Vendor> Search(IEnumerable<Vendor> vendor, int? Name, bool? IsActive, DateTime? FromDate, DateTime? ToDate);
        IQueryable<Vendor> Vendor_GetAll();
        IQueryable<Vendor> Get_ActiveVendor();
        bool Vendor_Create(Vendor vendor);
        Vendor Vendor_Delete(int id);
        bool Vendor_Edit(Vendor entity);
        Vendor FindVendorById(int? id);
        bool FindVendorByName(string Name);
        decimal? VendorBalanceUpdate(int VendorId, decimal Amount);
        bool VendorBalanceReturn(int TransactionId);
        #endregion

        #region Vendor Tarrif
        IQueryable<VendorTarrif> VendorTarrif_GetAll();
        bool VendorTarrif_Create(VendorTarrif vendorTarrif);
        bool VendorTarrif_Delete(int id);
        bool VendorTarrif_Edit(VendorTarrif vendorTarrif);
        VendorTarrif FindVendorTarrifById(int? id);
        bool FindVendorTarrifByName(string Name);
        void VendorTarrif_DeactivatePrevious(int serviceId, int VendorId);
        VendorTransaction VendorTransaction_FindById(int Id);
        #endregion

        #region Vendor Route
        IEnumerable<VendorRoute> VendorRoute_getAll();
        IEnumerable<VendorRoute> VendorRoute_GetAllByService(int ServiceId);
        IQueryable<VendorRoute> VendorRoute_GetAllQueryable();
        bool VendorRoute_Add(VendorRoute model);
        bool VendorRoute_UpdatePendingQty(int Id, bool IsIncrement);
        VendorRoute VendorRoute_GetByServiceAndVendor(int ServiceId, int VendorId);
        #endregion

        #region Vendor Transaction
        IQueryable<VendorTransaction> VendorTransaction_GetAllQueryable();
        bool VendorTransaction_Add(Transaction newTransaction, int BalUpdate);
        bool VendorTransaction_Add(VendorTransaction vendortransaction);
        bool VendorTransaction_Edit(VendorTransaction vendorTransaction);
        bool VendorRechage(VendorTransaction vendortransaction);
        #endregion

        #region Vendor Log
        IQueryable<VendorRequestLog> VendorReqLog_GetAll();
        IQueryable<VendorRequestLog> GetActiveVendorReqWithDetailsByVendorId(int VendorId);
        IQueryable<VendorRequestLog> GetVendorReqWithReqId(int ReqId);
        bool VendorRequestLog_Add(VendorRequestLog vrl);
        bool VendorRequestLog_Edit(VendorRequestLog vendorRequestLog);
        #endregion

        void Save();

    }
}
