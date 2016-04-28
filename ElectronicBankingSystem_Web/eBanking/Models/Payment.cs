using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class Payment
    {
       // public int DestinationId { get; set; }

        //FromCurrency
        [Required]
        [Display(Name="Country")]
        public int FromCurrencyId { get; set; }

        [Required]
        [Display(Name="Services")]
        //Bank Id 
        public int ServiceId { get; set; }

     //   public IEnumerable<Service> Services { get; set; }


        [Required]
        [Display(Name="Mobile No")]
        public string MobileNo { get; set; }

        [Required]
        //initially BDT
        [Display(Name="Amount")]
        public decimal Amount { get; set; }

        [Display(Name="Amount in USD")]
        [Required]
        public decimal AmountInUSD { get; set; }

        [Display(Name="Processing Fee")]
        public decimal ProcessingFee { get; set; }

        [Display(Name="Total Amount")]
        public decimal TotalAmount { get; set; }

        public string ClientId { get; set; }
        public int? PinId { get; set; }

    }
}