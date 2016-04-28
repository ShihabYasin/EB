using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace eBanking.App_Code
{
    public class Variable
    {
        public  string RoleId { get; set; }

        public string ActionName { get; set; }

        public string ControllerName { get; set; }

        public string Parameter { get; set; }

        public string UserName { get; set; }

        public string Message { get; set; }

        public int Flag { get; set; }
        public int count { get; set; }

        public string oldRoleId { get; set; }

        public string oldRoleName { get; set; }

        public decimal Balance { get; set; }
        //public decimal LocalBalance { get; set; }
        public decimal ConvertFromUsd { get; set; }

        public string ISO { get; set; }

        public int ApiStatus { get; set; }

        public string AllStringVar { get; set; }

        public bool ReturnsBooleanResult { get; set; }

        #region Global_GateWay_Uri_Variables

       public StringBuilder postData = null;
       public string responseMessage = string.Empty;
       public HttpWebRequest request = null;
       public HttpWebResponse response = null;
       public StreamReader respStreamReader = null;

        #endregion

    }
}