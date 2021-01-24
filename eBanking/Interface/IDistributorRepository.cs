using eBanking.Models;
using System.Collections.Generic;
using System.Linq;
using PagedList;
using System;

namespace eBanking.Interface
{
    interface IDistributorRepository
    {
        IQueryable<DistributorTransaction> DistributorTransaction_GetAllQueryable();
        IPagedList<DistributorTransactionVM> DistributorTransactionVM(IQueryable<DistributorTransaction> dt, int pageNumber, int pageSize);
        IQueryable<DistributorTransaction> DistributorTransaction_Search(int? ServiceId, int? DistributorId, DateTime? FromDate, DateTime? ToDate);//IQueryable<DistributorTransaction> dt, 
        IEnumerable<DistributorTransaction> GetAllDistributorTransaction();
        IQueryable<Distributor> Distributor_GetAllQueryable();
        Distributor Distributor_FindById(int DistributorId);
        Distributor Distributor_FindByUserName(string UserName);
        bool Distributor_Add(Distributor distributor);
        IEnumerable<Distributor> Distributor_GetAll();
        IEnumerable<DistributorCommissionRateplan> DCRP_GetAll();
        IQueryable<DistributorCommissionRateplan> DCRP_GetAllQueryable();
        DistributorCommissionRateplan DCRP_GetById(int DCRP_ID);
        IQueryable<DistributorCommissionRateplanViewModel> DCRP_GetAllVMByDistributorId(int? DistributorId);
        bool DCRP_Create(DistributorCommissionRateplan Entity);
        DistributorServiceChargeVM DCRP_GetDistributorServiceCharge(int ServiceId, string DistributorCode);
        IEnumerable<DistributorServiceChargeVM> DCRP_GetDSCforEachDistributor(int serviceId, string distributorCode);
        IQueryable<DistributorCommissionRateplanViewModel> DCRP_GetAssignedToDCRP(int distributorId);
        decimal? DistributorBalanceUpdate(int DistributorId, decimal Amount);
        int? DistributorTransaction_Add(DistributorTransaction transaction);
        int? DistributorTransaction_Update(DistributorTransaction transaction, decimal? PreviousAmountIn, decimal? PreviousAmountOut);
        bool DistributorTransactionDetails_Add(DistributorTransactionDetail model);
        IQueryable<DistributorTransactionDetail> DistributorTransactionDetails_GetAllQueryable();
        decimal? GetDistributorBalanceByDistCode(string DistributorCode);
        int GetDistributorIdFromDistributorCode(string DistributorCode);
        void Save();
        bool DCRP_DeactivatePrevious(int DistributorId, int ServiceId);
        bool ADR_Create(AssignDistributorRateplan model);
        bool ADR_Edit(AssignDistributorRateplan model);
        IQueryable<AssignDistributorRateplan> ADR_GetAllQueryable();
    }
}
