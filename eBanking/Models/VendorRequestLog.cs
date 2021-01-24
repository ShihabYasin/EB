using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class VendorRequestLog
    {
        public VendorRequestLog()
        {
            CreatedOn = DateTime.Now;
            Details = new List<VendorRequestLogDetail>();
            Confirmed = false;
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MID { get; set; }
        [Required]
        public int RequestId { get; set; }
        [Required]
        public int VendorId { get; set; }
        [MaxLength(4)]
        public string Type { get; set; }
        //private DateTime _CreatedOn = DateTime.Now;
        public DateTime CreatedOn { get; set; }
        public bool Confirmed { get; set; }
        public List<VendorRequestLogDetail> Details { get; set; }

    }
}