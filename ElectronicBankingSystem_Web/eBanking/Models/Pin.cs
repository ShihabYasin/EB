using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using PagedList;

namespace eBanking.Models
{

    //In Pin model add a column ResellerUserID
    //
    public class Pin
    {
        public int Id { get; set; }

        //every PinCode has a Money value
        //using this PinCode user can recharge this value to his/her Account
        public string PinCode { get; set; }

        //this is every PinCode SerialNumber
        [Display(Name = "Pin Prefix")]
        public string PinPrefix { get; set; }

        [Display(Name = "Serial No")]
        public long SerialNo { get; set; }
        //public string SerialNo { get; set; }

        //how many pin created in a day is BatchNumber store this Info
        public string BatchNumber { get; set; }
        [Display(Name = "Value(USD)")]
        public decimal Value { get; set; }

        //as like USD 
        [Display(Name="Currency")]
        public int CurrencyID { get; set; }

        public int Status { get; set; }

        public DateTime? CreationDate { get; set; }
        
        public bool IsActive { get; set; }

        //this user ID comes from AspNetUser table Id(PK)
        public string DistributorCode { get; set; }
        public DateTime? ExecutionDate { get; set; }
        public string UsedBy { get; set; }
        public int? ServiceId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ExpireDate { get; set; }
      
    }

    public class PinViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Batch No")]
        public string BatchNo { get; set; }

        [Display(Name="Serial No")]
        public string SerialNo { get; set; }

        [Display(Name="Pin Code")]
        public string PinCode { get; set; }

        //as like 100 dolar
        [Display(Name = "Pin Value")]
        public decimal Value { get; set; }

        //as like USD 
        [Display(Name = "Currency")]
        public string CurrencyName { get; set; }

        public string Status { get; set; }

        [Display(Name="Date")]
        public DateTime? CreationDate { get; set; }
        public DateTime? ExecutionDate { get; set; }

        [Display(Name="Active")]
        public bool IsActive { get; set; }


        //ReselerUserId is eBanking user Id but in pin index it show eBanking user UserName 
        [Display(Name = "Assign To")]
        public string UsedBy { get; set; }
        public string DistributorCode { get; set; }
        public string DistributorUserName { get; set; }
        public int? ServiceId { get; set; }
        public string ServiceName { get; set; }
    }

    public class PinTopUp
    {
        public string PinValue { get; set; }
        public string CountryCode { get; set; }
        public string TopUpValue { get; set; }
    }

    public class PinHistory
    {
        public PinHistory()
        {
            AssignedOn = DateTime.Now;
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SerialNo { get; set; }
        public int EntryType { get; set; }
        [Display(Name = "Pin Prefix")]
        [MaxLength(20)]
        public string PinPrefix { get; set; }
        [Display(Name = "Pin Serial (starts from) ")]
        public long PinSerialFrom { get; set; }
        [Display(Name = "Pin Serial (ends at) ")]
        public long PinSerialTo { get; set; }
        [Display(Name = "Assigned By")]
        public string AssignedBy { get; set; }
        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; }
        [Display(Name = "Assigned On")]
        public DateTime AssignedOn { get; set; }
        public int? DTId { get; set; }
    }
    public class PinViewModelTotal
    {
        //public PinViewModelTotal()
        //{
        //    PinViewModel = null;
        //    TotalPins = 0;
        //    TotalPinValue = 0;
        //}
        public IPagedList<PinViewModel> PinViewModel { get; set; }
        public int TotalPins { get; set; }
        public decimal TotalPinValue { get; set; }
        public List<int> PinIdList { get; set; }
    }
    public class PinAssignViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal UnitValue { get; set; }
        public decimal TotalValue { get; set; }
        public int TotalQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int AssignedQuantity { get; set; }
        public int DistributorId { get; set; }
    }
}