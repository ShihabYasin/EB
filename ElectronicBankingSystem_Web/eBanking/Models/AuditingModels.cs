using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class Audit
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditID { get; set; }
        [Required]
        public string IPAddress { get; set; }
        [Required]
        public string UserName { get; set; }
        public string URLAccessed { get; set; }
        [Required]
        public DateTime TimeAccessed { get; set; }
        public Audit()
        {
        }
    }
/*    public class AuditingContext : DbContext
    {
       public DbSet<Audit> AuditRecords { get; set; }
    }
  */
  public class AuditingModels
    {
    }
}