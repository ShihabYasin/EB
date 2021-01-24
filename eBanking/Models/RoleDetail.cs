using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class RoleDetail
    {
        [Key]
        public int Id { get; set; }      
        public string RoleId { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public bool IsAccessible { get; set; }

        
    }
}