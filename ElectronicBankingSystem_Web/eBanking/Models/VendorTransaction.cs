using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class VendorTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get;set;}
        public int TransactionId { get; set; }
        [Display(Name = "Service")]
        public int ServiceId { get; set; }
        [Display(Name = "Vendor")]
        public int VendorId { get; set; }
        public int VendorTarrifId { get; set; }
        public decimal? AmountInLocal { get; set; }
        public decimal? AmountOutLocal { get; set; }
        [Display(Name = "Transaction In")]
        public int UsedCurrencyId { get; set; }
        public decimal ConversionRateUSD { get; set; }
        public decimal? AmountInUSD { get; set; }
        public decimal AmountOutUSD { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal? VendorBalance { get; set; }
    }
    public class VendorTransactionMV
    {
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int Id { get; set; }
        public int TransactionId { get; set; }
        [Display(Name = "Service")]
        public string ServiceName { get; set; }
        [Display(Name = "Vendor")]
        public string VendorName { get; set; }
        public int VendorTarrifId { get; set; }
        public decimal? AmountInLocal { get; set; }
        public decimal? AmountOutLocal { get; set; }
        [Display(Name = "Transaction In")]
        public string CountryName { get; set; }
        public decimal ConversionRateUSD { get; set; }
        public decimal? AmountInUSD { get; set; }
        public decimal AmountOutUSD { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal? VendorBalance { get; set; }
        [NotMapped]
        public int? ServiceId { get; set; }
        [NotMapped]
        public int VendorId { get; set; }
        [NotMapped]
        public int? CountryId { get; set; }

        //Vendor Transaction Summary
        public decimal? TotalBalanceIn { get; set; }
        public decimal? TotalBalanceOut { get; set; }
        public List<Service> ServiceList { get; set; }
        public decimal? BalanceIn { get; set; }
        public decimal? BalanceOut { get; set; }
        public IEnumerable<VendorGP> VendorGroup { get; set; }
        //public string VendorNameAll { get; set; }

    }
    public class VendorGP
    {
        public string VendorName { get; set; }
        public List<VendorTransactionMV> ServiceList { get; set; }
        public string ServiceName { get; set; }
        public decimal? EachServicelBalanceIn { get; set; }
        public decimal? EachServiceBalanceOut { get; set; }
        public decimal? BalanceIn { get; set; }
        public decimal? BalanceOut { get; set; }
    }
}