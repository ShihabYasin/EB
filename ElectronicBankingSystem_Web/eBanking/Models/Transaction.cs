using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //foreign key of eBankingUser
        //hear userName will be assigned because GUID may not unique 
        [Required]
        [Display(Name="User")]
        public string UserId { get; set; }

        //foreign key of transection table
       // [Required]
        public string OperationNumber { get; set; }

        [Required(ErrorMessage="Please Select a Bank.")]
        public int ServiceId { get; set; }
              
        [Required]
        public int FromCurrencyId { get; set; }

        [NotMapped]
        public string CountryName { get; set; }

        [NotMapped]
        public string CurrencyISO { get; set; }

        //[Required]
        public int? PinId { get; set; }
   //     public int? PinCode { get; set; }

        //Local amount that is inserted to transfer or recharge or top up
        public decimal? InsertedAmount { get; set; }

        //USD
        public decimal? AmountIN { get; set; }

        //USD
        public decimal? AmountOut { get; set; }

        public decimal? ConversationRate  { get; set; }

        
        public int? RatePlanId { get; set; }

        public int? Status { get; set; }

        public DateTime? TransactionDate { get; set; }


        [Display(Name = "Recepient User")]
        public string ToUser { get; set; }

        public string FromUser { get; set; }

        public DateTime? UpdateDate { get; set; }

        public string Remarks { get; set; }

        [NotMapped]
        public string ServiceName { get; set; }

        [NotMapped]
        public string StatusName { get; set; }

        [Display(Name="Reference")]
        public string ReferenceId { get; set; }

        [NotMapped]
        public bool IsCheck { get; set; }
        public DateTime? TimeOut { get; set; }
        public bool IsTimeOut { get; set; }
        public int ClientId { get; set; }
        public int VendorId { get; set; }
        public decimal? UserBalance { get; set; }
        //public bool ReturnCost { get; set; }
        public decimal DistributorCommission { get; set; }
    }
    
     // TransactionHistory
    public class TransactionHistory
    {
        public int Id { get; set; }

        //foreign key of eBankingUser
        //hear userName will be assigned because GUID may not unique 
   
        [Display(Name = "User")]
        public string UserId { get; set; }

        //foreign key of transection table
        // [Required]
        public string OperationNumber { get; set; }

    //    [Required(ErrorMessage = "Please Select a Bank.")]
        public int? ServiceId { get; set; }

 //       [Required]
        public int FromCurrencyId { get; set; }

        //[NotMapped]
        public string CountryName { get; set; }

        //[NotMapped]
        public string CurrencyISO { get; set; }

        //[Required]
        public int? PinId { get; set; }
        //[NotMapped]
        public string PinCode { get; set; }

        //Local amount that is inserted to transfer or recharge or top up
        public decimal? InsertedAmount { get; set; }

        //USD
        public decimal? AmountIN { get; set; }

        //USD
        public decimal? AmountOut { get; set; }

        public decimal? ConversationRate { get; set; }


        public int? RatePlanId { get; set; }

        public int? Status { get; set; }

        public DateTime? TransactionDate { get; set; }


        [Display(Name = "Recepient User")]
        public string ToUser { get; set; }

        public string FromUser { get; set; }

        public DateTime? UpdateDate { get; set; }

        public string Remarks { get; set; }

        //[NotMapped]
        public string ServiceName { get; set; }

        //[NotMapped]
        public string StatusName { get; set; }

        [Display(Name = "Reference")]
        public string ReferenceId { get; set; }

        //[NotMapped]
        public bool IsCheck { get; set; }
        public DateTime? TimeOut { get; set; }
        public bool IsTimeOut { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public int VendorId { get; set; }
    }


    public class CreditTransfer
    {

        [Required(ErrorMessage = "Please Login")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Invalid User.")]
        [Display(Name = "To User")]
        public string ToUser { get; set; }

        [Required(ErrorMessage = "Please Enter Numeric number up to 6 decimal.")]
        [Display(Name = "Credit Transfer")]
        public decimal Balance { get; set; }
        public string ClientId { get; set; }

    }

    public class TransactionSearch 
    {

        public string UserNumber { get; set; }
        public string Status { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

       
    }

    //Action TransactionHistory
    public class MoneyTopUpHistory
    {
        public string TransactionDate { get; set; }
        public string OperationNumber { get; set; }
        public string Status { get; set; }

        [Display(Name="Receiver")]
        public string ToUser { get; set; }
        public decimal? InsertedAmount { get; set; }
        //USD
        public decimal? AmountOut { get; set; }

        public string TransactionType { get; set; }
    
    }

    public class TransferHistory
    {
        public int Id { get; set; }
        //Transaction ID(operation Number)
        [Display(Name = "Transaction Number")]
        public string OperationNumber { get; set; }
        
        [Display(Name = "Submission Time")]
        public DateTime? TransactionDate { get; set; }

        //get destination name from FromCurrencyId
        [Display(Name = "Country Name")]
        public string MBankDestination { get; set; }

        //Service Name
        [Display(Name="Bank Name")]
        public string BankName { get; set; }

        [Display(Name = "Recipient")]
        public string MBankNumber { get; set; }

        [Display(Name = "Amount in local currency")]
        public decimal? InsertedAmount { get; set; }

        //Rate Plan Service Charge
        [Display(Name = "Processing Fee")]
        public decimal ProcessingFee { get; set; }

        //USD amount in USD
        [Display(Name = "amount in USD")]
        public decimal? AmountOut { get; set; }
       // ExecutionTime(Update Time)

        [Display(Name = "Execution Time")]
        public DateTime? UpdateDate { get; set; }        

        public string Status { get; set; }
        public bool IsTimeOut { get; set; }
    }
}