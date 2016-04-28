using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class Destination
    {
        public int Id { get; set; }

        public string DestinationName { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public string CountryCode { get; set; }

        //public int CurrencyId { get; set; }
    }
}