using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using eBanking.Interface;

namespace eBanking.Models
{
    public class DistributorTransaction
    {
        public DistributorTransaction()
        {
            AmountIn = 0;
            AmountOut = 0;
            CurrentBalance = 0;
            CreatedOn = DateTime.Now;
            AmountInLocal = 0;
            AmountOutLocal = 0;
            HasDetails = false;

        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DTId { get; set; }
        public int ServiceId { get; set; }
        public int DistributorId { get; set; }
        public decimal? AmountIn { get; set; }
        public decimal? AmountOut { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal? AmountInLocal { get; set; }
        public decimal? AmountOutLocal { get; set; }
        public int? CurrencyId { get; set; }
        public decimal? ConvertToUsd { get; set; }
        [MaxLength(1000)]
        public string Remarks { get; set; }
        public bool? HasDetails { get; set; }
    }

    public class DistributorTransactionDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int DTId { get; set; }

        public string UserName { get; set; }
        public int UserTransactionNo { get; set; }
        public decimal DistributorCommission { get; set; }
        public int DistributorCommissionRateId { get; set; }
    }

    public class DistributorTransactionDetailVM
    {
        public int Id { get; set; }
        public DateTime ? Date { get; set; }
        public string UserName { get; set; }
        public string RecipentNo { get; set; }
        public string ServiceName { get; set; }
        public decimal ? Amount { get; set; }
        public string UserTransactionNo { get; set; }
        public decimal DistributorCommission { get; set; }
 
    
    }


    public class DistributorTransactionVM
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public string DistributorName { get; set; }
        public decimal? AmountIn { get; set; }
        public decimal? AmountOut { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal? AmountInLocal { get; set; }
        public decimal? AmountOutLocal { get; set; }
        public int? CurrencyId { get; set; }
        public decimal? ConvertToUsd { get; set; }
        public bool? HasDetails { get; set; }
    }

    

}