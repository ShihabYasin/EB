using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class Distributor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DistributorId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public int ParentId { get; set; }
        //[Required]
        public string DistributorCode { get; set; }
        public bool IsActive { get; set; }
        public decimal? DistributorBalance { set; get; }
    }

    public class DistributorReturnCommission
    {
        public int DistributorId { get; set; }
        public string UserName { get; set; }
        public int ParentId { get; set; }
        public string DistributorCode { get; set; }
        [NotMapped]
        public decimal? AmountIn { get; set; }
        [NotMapped]
        public decimal? AmountOut { get; set; }
        [NotMapped]
        public string Remarks { get; set; }

    }

    public class DistributorViewModel
    {
        public int DistributorId { get; set; }
        public string UserName { get; set; }
        public string DistributorCode { get; set; }
        public decimal? DistributorBalance { get; set; }
        public string DistributorType { get; set; }
        public bool? IsActive { get; set; }
    }

    public class DistributorLastCommissionReturn
    {
        public decimal? Amount { get; set; }
        public DateTime? Time { get; set; }
    }

    public class RetailerRegisteredUserVM
    {
        public int Id { get; set; }
        public string UserNme { get; set; }
        public decimal Balance { get; set; }
    }
}