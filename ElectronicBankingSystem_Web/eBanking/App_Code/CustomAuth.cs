
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using eBanking.Models;

namespace eBanking.App_Code
{
    public class CustomAuth : AuthorizeAttribute
    {
        private Variable _variable = new Variable();
        private eBankingDbContext db = new eBankingDbContext();

        private UserMenuGenarator UserRole = new UserMenuGenarator();

        private int customSupport = 0;
        private bool Access { get; set; }
        private bool privilegeLevels { get; set; }
        // Custom property
        public string AccessLevel { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var isAuthorized = base.AuthorizeCore(httpContext);


            if (!isAuthorized)
            {
                Access = false;
                return false;
            }

            else
            {
                Access = true;
                return true;
            }
            // return base.AuthorizeCore(httpContext);
        }

        private bool GetUserRights(string UserName, string ControllerName, string ActionName)
        {


            //var query = db.Users.Where(x => x.UserName == UserName).Select(x=>x).SingleOrDefault();

            //if (query!=null) //query.Id != null && !string.IsNullOrEmpty(query.Id)
            //{
            var isInRole = UserRole.GetUserRole(UserName);


            ControllerName = ControllerName + "Controller";
            //if has a role
            if (isInRole != null)  //isInRole.RoleId != null && !string.IsNullOrEmpty(isInRole.RoleId)
            {
                try
                {
                    var IsAccess = db.RoleDetails.Where(x => x.RoleId == isInRole.RoleId && x.ControllerName == ControllerName && x.ActionName == ActionName && x.IsAccessible == true).SingleOrDefault();
                    if (IsAccess != null)
                        return true;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    //To do genarate a log file
                    string ms = ex.Message;
                }

            }

            //}
            return false;
        }



        public override void OnAuthorization(AuthorizationContext filterContext)
        {

            base.OnAuthorization(filterContext);

            _variable.Message = null;

            if (Access == true)
            {
                var rd = filterContext.HttpContext.Request.RequestContext.RouteData;

                _variable.ActionName = rd.GetRequiredString("action");
                _variable.ControllerName = rd.GetRequiredString("controller");
                _variable.Parameter = filterContext.HttpContext.Request["IsDevice"] as string;
                _variable.AllStringVar = filterContext.HttpContext.Request["CustomerSupport"] as string; 
                //Message

               string ManageTransferMessage = filterContext.HttpContext.Request["Message"] as string; 

                _variable.UserName = filterContext.HttpContext.User.Identity.Name.ToString();

                if (_variable.UserName != null && _variable.ActionName != null && _variable.ControllerName != null)
                {
                    //if (_variable.UserName == ConstMessage.StaticUserName && _variable.AllStringVar !="1")
                     
                    //{

                    //    filterContext.Result = new RedirectToRouteResult(
                    //          new RouteValueDictionary(
                    //            new
                    //            {
                    //                controller = "Transaction",
                    //                action = "ManageTransfer",
                    //                CustomerSupport = 1,
                    //                Message = ManageTransferMessage,
                    //            })
                    //        );
                    //}


                    //if (_variable.UserName != ConstMessage.StaticUserName)
                    //{
                        privilegeLevels = GetUserRights(_variable.UserName, _variable.ControllerName, _variable.ActionName); // Call another method to get rights of the user from DB

                        //allow access
                        if (privilegeLevels != true)
                        {
                            _variable.Message = ConstMessage.CustomAuthErrorMessage;
                        }
                    //}
                }
                else
                {
                    _variable.Message = ConstMessage.CustomAuthErrorMessage;
                }
            }
            //Access false auto redirect Login page

            if (_variable.Message != null)
            {
                filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                controller = "Account",
                                action = "Unauthorised",
                                Message = _variable.Message
                            })
                        );
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new
                        {
                            controller = "Account",
                            action = "Unauthorised"

                        })
                    );
            base.HandleUnauthorizedRequest(filterContext);
        }
                       
    }
}