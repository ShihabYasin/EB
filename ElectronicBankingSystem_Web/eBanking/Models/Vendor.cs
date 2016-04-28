using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class Vendor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [MaxLength(20)]
        [Required]
        [Display(Name = "Vendor Name")]
        public string Name { get; set; }
        
        public string CreatedBy { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name="Created Date")]
        public DateTime CreatedOn { get; set; }
        public bool IsActive { get; set; }
         [Required]
         [Display(Name = "Local Currency")]
         public int LocalCurrencyId { get; set; }
        [Display(Name = "Curreant Balance")]
        public decimal CurrentBalance { get; set; }
    }
}