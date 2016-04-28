using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class Currency
    {
        [Key]
        public int Id { get; set; }

        [Display(Name="Currency Name")]
        public string CurrencyName { get; set; }
        
        [Display(Name="Currency Code")]
        public string ISO { get; set; }

        [Display(Name="Destination")]
        public int DestinationId { get; set; }

        [Display(Name="Symbol")]
        public string Sign { get; set; }
        public decimal ConvertFromUsd { get; set; }
    }

    //used in both Currency and RatePlan Controller
    public class CurrencyViewModel {

        public int Id { get; set; }

        [Display(Name="Currency Name")]
        public string CurrencyName { get; set; }
     
        [Display(Name="Currency Code")]
        public string ISO { get; set; }

        [Display(Name="Country Name")]
        public string DestinationName { get; set; }

        [Display(Name = "Symbol")]
        public string Sign { get; set; }
       
    }
}