using eBanking.App_Code;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eBanking.Models
{
    public class SMSTransactionReport
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        [Display(Name = "Request Message")]
        public string ReqMessage { get; set; }
        public string ReqTime { get; set; }
         [Display(Name = "Response Message")]
        public string MessageBody { get; set; }
        public string ResTime { get; set; }
        public string OperationNumber { get; set; }
        [Display(Name = "Status")]
        public int ApiStatus { get; set; }
        public string StatusName { get; set; }

    }

  
}