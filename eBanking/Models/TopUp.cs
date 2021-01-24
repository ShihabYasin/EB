using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class TopUp
    {
       
        [Display(Name = "User")]
        public string UserId { get; set; }

        [Required(ErrorMessage="Please Select a destination.")]
        [Display(Name = "Destination")]
        public int FromCurrencyId { get; set; }

        [Required(ErrorMessage="Please Ensert a mobile number for top up.")]
        [Display(Name = "Mobile No")]
        public string ToUser { get; set; }

        //By Default all statusId is panding 
        public int? StatusId { get; set; }


        //[Required(ErrorMessage = "Please Select a amount.")]
        //[Display(Name="Select Amount")]
        //Local amount that is inserted to transfer or recharge or top up
        public int ValueId { get; set; }

        [Display(Name = "Amount in USD")]
        [Required]
        public decimal AmountInUSD { get; set; }

        [Required]
        [Display(Name = "Processing Fee")]
        public decimal ProcessingFee { get; set; }

        [Required]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        //only fromcurrency is BDT
        [Display(Name="Type")]
        public int ServiceId { get; set; }

        [Required]
        public int RatePlanId { get;set; }
        [Required]
        public decimal? Value { get; set; }

        public int Type { get; set; }

        public int? PinId { get; set; }

        public string OperatorPrefix { get; set; }

        public string ClientId { get; set; }

    }
}