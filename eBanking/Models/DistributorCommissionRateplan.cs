using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class DistributorCommissionRateplan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DCRP_ID { get; set; }
        [Required]
        public int DistributorId { get; set; }
        [Required]
        [Display(Name = "Service")]
        public int ServiceId { get; set; }
        [Required]
        public decimal Commission { get; set; }
        [Required]
        [Display(Name = "Is Percentage")]
        public bool IsPercentage { get; set; }
        public decimal? Discount { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Decimal? ServiceCharge { get; set; }
        public string RateName { get; set; }
        public string Remarks { get; set; }
        //public int DistributorCode { get; set; }
    }

    public class DistributorCommissionRateplanViewModel
    {
        public int DCRP_ID { get; set; }
        public string DistributorUserName { get; set; }
        public int DistributorId { get; set; }
        public string ServiceName { get; set; }
        public int ServiceId { get; set; }
        public decimal Commission { get; set; }
        public bool IsPercentage { get; set; }
        public decimal? Discount { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ClientId { get; set; }
        public Decimal? ServiceCharge { get; set; }
        public string Remarks { get; set; }
        public string RateName { get; set; }
    }

    public class DistributorCommissionRateplanViewModel2
    {
        public int DCRP_ID { get; set; }
        public string DistributorUserName { get; set; }
        public List<int> DistributorId { get; set; }
        public string ServiceName { get; set; }
        public int ServiceId { get; set; }
        public decimal Commission { get; set; }
        public bool IsPercentage { get; set; }
        public decimal? Discount { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class AssignDistributorRateplan
    {
        [Key]
        public int Id { get; set; }
        public int RateplanId { get; set; }
        public int DistributorId { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class AssignedDCRPViewModel
    {
        public string RateName { get; set; }
        public int RatePlanId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal Commission { get; set; }
        public bool IsPercentage { get; set; }
        public int DistributorId { get; set; }
        public bool IsActive { get; set; }
        public string ClientId { get; set; }
        public decimal? ServiceCharge { get; set; }
    }

    //public class AssignDistributorRateplanViewModel
    //{
    //    [Key]
    //    public int Id { get; set; }
    //    public int RateplanId { get; set; }
    //    public List<int> DistributorId { get; set; }
    //    public bool IsActive { get; set; }
    //    public string CreatedBy { get; set; }
    //    public DateTime? CreatedOn { get; set; }
    //    public string UpdatedBy { get; set; }
    //    public DateTime? UpdatedOn { get; set; }
    //}

}