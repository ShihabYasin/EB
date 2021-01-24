using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class Sequencer
    {
        [Key]
        public int Id { get; set; }
        public int SqlNumber { get; set; }

        public string Prefix { get; set; }
        public int PrefixType { get; set; }
            
    }
}