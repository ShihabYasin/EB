using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Linq;
//using System.Web;

namespace eBanking.Models
{
    public class TransactionRecordsVM
    {
        public decimal? userBalance { get; set; }
        public decimal? vendorBalance { get; set; }
        public decimal? distributorBalance { get; set; }

        public decimal? totalTransaction { get; set; }
        public decimal? topupTransaction { get; set; }
        public decimal? moneyTransferTransaction { get; set; }
        public decimal? returnBalance { get; set; }

        public int? pinNumber { get; set; }
        public int? activePinNumber { get; set; }
        public decimal? activePinBalance { get; set; }
        public int? usedPinNumber { get; set; }
        public decimal? usedPinBalance { get; set; }


        public decimal? PurchaseByCustomerToday { get; set; }
        public decimal? PurchaseFromVendorToday { get; set; }
        public int? TotalTransactionCountToday { get; set; }
        //should be in usd rate
        public decimal? VendorsBalance { get; set; }
        public string VendorName { get; set; }
        public decimal? TotalCustomerTransaction { get; set; }
        public decimal? CustomerTransactionForEachService { get; set; }
        public decimal? Deducted { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Revenue { get; set; }
        public int? TopServiceId { get; set; }
        public string TopServiceName { get; set; }
        public int TopServiceTotalCount { get; set; }
        public decimal? TopServiceProfit { get; set; }
        public int? PinCount { get; set; }        
        public decimal? PinAmount { get; set; }
        public string TopServices { get; set; }

        //public DateTime TransactionTime { get; set; }
        //public int? Status { get; set; }
    }
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Phone No")]
        [StringLength(14,ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string UserName { get; set; }

        //[Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        //[DataType(DataType.Password)]
        //[Display(Name = "Password")]
        public string Password { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Confirm password")]
        //[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Name { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string DateofBirth { get; set; }
        public string Email { get; set; }
        public string PresentAddress { get; set; }
        public string ParmanentAddress { get; set; }
        public string OtherConductInfo { get; set; }
        public bool IsActive { get; set; }
        public string NomineePhoneNumber { get; set; }
    }

    public class UserCreateViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        //[EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        public string Email { get; set; }


        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Add a Role")]
        [Required(ErrorMessage = "Select a Role Name.")]
        public string RoleName { get; set; }
        [Display(Name = "Nominee Phone Number")]
        [Required(ErrorMessage = "Insert Nominee Phone Number.")]
        public string NomineePhoneNumber { get; set; }


    }

    public class UserEditViewModel
    {

        public string UserId { get; set; }

        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        //[EmailAddress(ErrorMessage = "Invalid Email Address")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
        public string Email { get; set; }

        public bool IsActive { get; set; }

        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} characters long.")]
        public string Address { get; set; }

        [Display(Name = "Add a Role")]
        [Required(ErrorMessage = "Select a Role Name.")]
        public string RoleName { get; set; }
               
        public decimal? CurrentBalance { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? Country { get; set; }
        public string CountryName { get; set; }
        public string DistributorCode { get; set; }
    }

    public class AuditViewModel
    {
        [Required]
        public int AuditID { get; set; }
        public string UserName {get; set; }
        public string URLAccessed { get; set; }
        public string IPAddress { get; set; }
        public DateTime TimeAccessed { get; set; }

        public object CurrentBalance { get; set; }

        public decimal Balance { get; set; }
    }

    public class UserCashInViewModel
    {
        //[Required]
        //[Display(Name = "Destributor")]
        //public string UserName { get; set; }
        [Required]
        [Display(Name = "Destributor")]
        public int DistributorId { get; set; }
        [Display(Name = "eBanking Credit")]
        public decimal eBankingCredit { get; set; }
        [Required]
        [Display(Name = "Price")]
        public decimal Price { get; set; }
        [Required]
        [Display(Name = "Currency of Payment")]
        public int LocalCurrencyId { get; set; }
        [Display(Name = "Conversion Rate")]
        public decimal ConversionRate { get; set; }
        public string ClientId { get; set; }
    }

    
}
