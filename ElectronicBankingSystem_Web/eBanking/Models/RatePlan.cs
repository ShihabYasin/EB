using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class RatePlan
    {
        public int Id { get; set; }

      
        [Required]
        [Display(Name="Service Option")]
        public int ServiceId { get; set; }

        [Display(Name="From Currency")]
        public int FromCurrencyId { get; set; }

       [Display(Name = "To Currency")]
        public int ToCurrencyId { get; set; }

         [Display(Name = "Convartion Rate")]
        [RegularExpression(@"\A\d+(\.\d{1,6})?\Z", ErrorMessage = "Please enter a numeric value with up to six decimal places.")]
      
        //[Range(1.00m, 100.00m)]
        public decimal ConvertionRate { get; set; }

        [Display(Name = "Service Charge($)")]
        [RegularExpression(@"\A\d+(\.\d{1,6})?\Z", ErrorMessage = "Please enter a numeric value with up to six decimal places.")]
        public decimal ServiceCharge { get; set; }
        [Display(Name = "As Percentage")]
        public bool ServiceChargeIsPercentage { get; set; }

        [Display(Name = "Other Charge($)")]
        [RegularExpression(@"\A\d+(\.\d{1,6})?\Z", ErrorMessage = "Please enter a numeric value with up to six decimal places.")]
        public decimal? OtherCharge { get; set; }
        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        [Display(Name="Active")]
        public bool IsActive { get; set; }
        public int? PendingTime { get; set; }
        [Required]
        [Display(Name = "MRP($)")]
        public decimal MRP { get; set; }

        [Display(Name = "MRP as Percentage")]

        [Required]
        public bool MRPisPercentage { get; set; }

        [Required]
        [Display(Name = "Cost($)")]
        public decimal Cost { get; set; }
    }

    public class RatePlanViewModel
    {
        public int Id { get; set; }


        [Required]
        [Display(Name = "Service Option")]
        public string ServiceName { get; set; }

        [Display(Name = "From Currency")]
        public string FromCurrencyName { get; set; }

        [Display(Name = "To Currency")]
        public string ToCurrencyName { get; set; }

        [Display(Name = "Convartion Rate")]
        [RegularExpression(@"\A\d+(\.\d{1,6})?\Z", ErrorMessage = "Please enter a numeric value with up to six decimal places.")]

        //[Range(1.00m, 100.00m)]
        public decimal ConvertionRate { get; set; }

        [Display(Name = "Service Charge($)")]
        [RegularExpression(@"\A\d+(\.\d{1,6})?\Z", ErrorMessage = "Please enter a numeric value with up to six decimal places.")]
        public decimal ServiceCharge { get; set; }

        [Display(Name = "As Percentage")]
        public bool ServiceChargeIsPercentage { get; set; }

        [Display(Name = "Other Charge($)")]
        [RegularExpression(@"\A\d+(\.\d{1,6})?\Z", ErrorMessage = "Please enter a numeric value with up to six decimal places.")]
        public decimal? OtherCharge { get; set; }

        [Display(Name="Creation Date")]
        public DateTime CreatedDate { get; set; }


        //user who create rate paln
        [Display(Name="Created BY")]
        public string CreatedBy { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        [Display(Name="MRP($)")]
        public decimal MRP { get; set; }
        [Display(Name = "MRP as Rate")]
        public bool MRPisPercentage { get; set; }

       [Display(Name = "Cost($)")]
        public decimal? Cost { get; set; }

    }
}