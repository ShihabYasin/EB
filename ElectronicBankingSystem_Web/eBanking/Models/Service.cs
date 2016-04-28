using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        
        [Required]
        [Display(Name = "Service Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name="Distination")]
        public int DestinationId { get; set; }

        [Required]
        [Display(Name = "Service Type")]
        public int ParentId {get; set;}

        [Display(Name="Group")]
        public bool IsGroup { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        //[RegularExpression("[0-9]{1,7}")]

        [Range(1,1000)]
        public int? value { get; set; }
        //public string ParentName { get; set; }
    }

    public class TeamNode
    {
        public Service ParentId { get; set; }  
        public IEnumerable<TeamNode> Children { get; set; }
    }
    public class ServiceViewModel
    {
        public int Id { get; set; }

        [Display(Name="Service Name")]
        public string Name { get; set; }

        [Display(Name = "Distination")]
        public string Destination { get; set; }

        [Display(Name = "Service Type")]
        public int? ParentId { get; set; }

        [Display(Name = "Service Type")]
        public string ParentName { get; set; }

        [Display(Name = "Group")]
        public bool IsGroup { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public List<ServiceViewModel> Children { get; set; }
    }
}