using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class VendorTarrif
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TarrifId { get; set; }
        [Display(Name="Vendor Id")]
        public int VendorId { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        [Display(Name="Service Name")]
        public int ServiceId { get; set; }
        [Display(Name = "Conversion Rate")]
        public decimal ConversionRate { get; set; }
        public decimal Cost { get; set; }
        public string Refference { get; set; }
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; }
        [Display(Name = "Is Percentage")]
        public bool IsPercentage { get; set; }
    }
}