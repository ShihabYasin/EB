using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Web.Mvc;


namespace eBanking.Controllers
{
    [CustomAuth]
    public class AuditController : Controller
    {
        private AuditRepository audit_repo;        
        
        public AuditController()
        {
            audit_repo = new AuditRepository();
        }
        
        //
        // GET: /Audit/
        public ActionResult Index(string IPAddress, string UserName, DateTime? FromDate, DateTime? ToDate, int? page)
        {
            int pageSize = ConstMessage.ITEMS_PER_PAGE;
            int pageNumber = (page ?? 1);
            
            try
            {
                IEnumerable<AuditViewModel> auditRecords = null;
                auditRecords = audit_repo.AuditSearch(IPAddress, UserName, FromDate, ToDate);
                return View(auditRecords.ToPagedList(pageNumber,pageSize));
            }
            catch (Exception)
            {
                return View("Error");
            }
        }
	}

    public class AuditAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            LoginViewModel model = new LoginViewModel();
            string UserName = request["UserName"];
            Audit audit = new Audit()
            {
                IPAddress = request.ServerVariables["HTTP_X_FORWORDED_FOR"] ?? request.UserHostAddress, //request.ServerVariables["HTTP_X_FORWORDED_FOR"],
                URLAccessed = request.RawUrl,
                UserName = UserName, //(request.IsAuthenticated) ? filterContext.HttpContext.User.Identity.Name : "Anonymous", //filterContext.HttpContext.User.Identity.Name,
                TimeAccessed = DateTime.Now
            };

            if (!string.IsNullOrEmpty(UserName))
            {
                AuditRepository audit_repo = new AuditRepository();
                audit_repo.Add(audit);
            }
            
            base.OnActionExecuting(filterContext);
        }
        
    }
}