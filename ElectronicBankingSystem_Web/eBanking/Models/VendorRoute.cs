using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eBanking.Models
{
    public class VendorRoute
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int VendorId { get; set; }
        public int Priority { get; set; }
        public int PendingRequest { get; set; }
    }
}