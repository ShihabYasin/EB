using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class RequestResponse
    {
        public RequestResponse()
        {
            TransactionId = "";
            RESPONSE_MSG = "";
        }

        public string TransactionId { get; set; }
        public int? Status { get; set; }
        public string RESPONSE_MSG { get; set; }
        public DateTime? RefilSuccessDate { get; set; }

    }

    public class SMSDR
    {
        public int Id { get; set;}

        // to receiver
        public string PHN_NO {get;set;}

        //post prepaid
        public string PHN_NO_TYPE {get;set;}

        //local or inserted amount
        public decimal? TRANS_AMOUNT {get;set;}

        // USD of inserted TRANS_AMOUNT with service charge
        public decimal? COST_AMOUNT { get; set; }

        // rate plan Id
        public int RATE_PLANE_ID {get;set;}

        // request get from api
        public string REQUEST_ID {get;set;}

        //first requested time 
        public DateTime? REQUEST_TIME {get;set;}

        //status changed time
        public DateTime? REFILL_SUCCESS_TIME {get;set;}
        //Status ID
        public int STATUS {get;set;} 
        public int TRANSACTION_ID  {get;set;}
        public string RESPONSE_MSG {get;set;}
        public string GW_ID {get;set;} //gate way Id
       // public int RESLLER_ID {get;set;} 
        public string CREATED_BY {get;set;}
       // public DateTime CREATED_TIME { get; set; }  
        public int VendorId { get; set; }
        [NotMapped]
        public int? ServiceId { get; set; }
    }
}