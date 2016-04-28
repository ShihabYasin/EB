using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class VendorRequestLogDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public int Status { get; set; }
        public int MID { get; set; }

        [ForeignKey("MID")]
        public VendorRequestLog VendorRequestLog { get; set; }
    }
}