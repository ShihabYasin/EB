using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class Link
    {
      
            public int Id { get; set; }
            public int? AId { get; set; }
            public string Name { get; set; }
            public int Parent { get; set; }
            public string Url { get; set; }
            public bool IsParent { get; set; }
            public bool IsActive { get; set; }
            public string ControllerName { get; set; }
            public string ActionName { get; set; }
            public int SequenceId { get; set; }
            public string GroupName { get; set; }
        
    }
}