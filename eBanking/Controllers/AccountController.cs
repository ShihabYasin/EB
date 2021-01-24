using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;

//for json data parse
using eBanking.Models;
using eBanking.Interface;
using eBanking.Abstract;
using eBanking.App_Code;
using System.Net;

namespace eBanking.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
       // private IdentityConfig ObjIdentity;
        
        private UserMenuGenarator user_menu = new UserMenuGenarator();
        private smsMessageHelper message = new smsMessageHelper();
        private Variable _variable = new Variable();

      
        public AccountController()
            : this(new UserManager<eBankingUser>(new UserStore<eBankingUser>(new eBankingDbContext())),new RoleManager<eBankingRole>(new RoleStore<eBankingRole>(new eBankingDbContext())))
        {
        }

        public AccountController(UserManager<eBankingUser> userManager,RoleManager<eBankingRole> roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        public UserManager<eBankingUser> UserManager { get; private set; }

        public RoleManager<eBankingRole> RoleManager { get; private set; }

        [AllowAnonymous]
        public ActionResult Unauthorised(string Message)
        {
            ViewBag.Message = Message;
            return View();
        }

        //ApiLogin called when third party(from mobile) wants to login with a version

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ApiLogin(string UserName, string Password, string ApkVersionName, string ClientId)
        {
            LoginViewModel ApiLogin = new LoginViewModel();
            IAdminRepository admin_repo = new AdminRepository();

            if (!admin_repo.ClientIsActive(ApkVersionName)) //"1.0" //ApkVersionName != ConstMessage.APK_VERSION
            {
                ViewBag.ApiStatus = ConstMessage.STATUS_LOGIN_APK_VERSION_FAILED; //3
            }
            else
            {

                ViewBag.ApiStatus = ConstMessage.STATUS_LOGIN_FAILED_ID; //0
                ViewBag.Balance = ConstMessage.STATUS_SET_USER_BALANCE;  //-1
                ViewBag.ISO = "";
                ViewBag.RemainingTime = "";
                ViewBag.LoginRemainingAttempt = "";
                ViewBag.LockOut = false;
                ViewBag.SMSTransactionReportTab_1 = "";
                ViewBag.NomineeExists = false;


                // when UserName,Password and ApkVersionName are given and not nullable then allow for login
                //else ApiStatus=0 means failed 1 means login success

                if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(ApkVersionName))
                {

                    ApiLogin.UserName = UserName;
                    ApiLogin.Password = Password;
                    ApiLogin.RememberMe = false;

                    var login = Login(ApiLogin, null, ConstMessage.API_FROM);


                }
            }
            return Json(new { ViewBag.ApiStatus, ViewBag.Balance, ViewBag.ConvertFromUsd, ViewBag.ISO, ViewBag.LockOut, ViewBag.LoginRemainingAttempt, ViewBag.RemainingTime, ViewBag.SMSTransactionReportTab_1, ViewBag.NomineeExists }, JsonRequestBehavior.AllowGet);      //ViewBag.LocalBalance
        }

        //ApiLogOff called when third party(from mobile) wants to LogOff 
        //[HttpPost]
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ApiLogOff()
        {
           
            ViewBag.ApiStatus = ConstMessage.STATUS_LOGG_OFF_FAILED_ID;
          
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var logoff = LogOff();
            }
            else
                ViewBag.ApiStatus = ConstMessage.STATUS_LOGG_OFF_SUCCESS_ID;

            return Json(new { ViewBag.ApiStatus }, JsonRequestBehavior.AllowGet);
        }


        //[AllowAnonymous]
        //public ActionResult Message()
        //{
        //    smsMessageHelper message = new smsMessageHelper();
        //    _variable.Message = message.SendMessage("8801723674115", ConstMessage.SMS_SENDER_SendingMessage);
        //    //country code(as pattern 88) must added otherwise message does not send
        //    return Content("ok");
        //}


        //ApiRegister called when third party(from mobile) wants to Registered

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ApiRegister(string UserName, string NomineePhoneNumber, string ClientId)
        {
            ViewBag.RegisterPassword = ConstMessage.STATUS_REGISTER_FAILED_ID;
            ViewBag.ApiStatus = ConstMessage.STATUS_REGISTER_FAILED_ID;

            if (!string.IsNullOrEmpty(UserName))
            {
                RegisterViewModel ApiRegister = new RegisterViewModel();
                ApiRegister.UserName = UserName;
                ApiRegister.NomineePhoneNumber = NomineePhoneNumber;

               var reg=  Register(ApiRegister, true);
            }


            return Json(new {ViewBag.ApiStatus,ViewBag.RegisterPassword }, JsonRequestBehavior.AllowGet);
           
        }

        // ApiPasswordChange called when third party(from mobile) wants to Changed his Password

        [HttpPost]
        public ActionResult ApiPasswordChange(ManageUserViewModel model, string ClientId)
        {
            ViewBag.ApiStatus = ConstMessage.STATUS_PASSWORD_CHANGE_FAILED_ID; //0

            if (model.OldPassword != null && model.NewPassword != null && model.ConfirmPassword != null)
            {
                ManageUserViewModel newModel = model;
                var result = Manage(newModel);
            }
            return Json(new { ViewBag.ApiStatus }, JsonRequestBehavior.AllowGet);
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //login  with lockout option
        //user account will be locked some times after failed 3 times to logIn
        //after this time user can try again to login
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [Audit]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl, int? FromApi)
        {
            //string hostIp = HttpContext.Request.UserHostAddress; //GetUserHostInfo.GetIp();  //base.Request;
            //return hostIp;
            ViewBag.ForgotPassword = "";
            int LockOutTime = 0;
            int Access = 1;

            try
            {
                if (ModelState.IsValid)
                {
                    //step 1- find user by username first
                    var User = await UserManager.FindByNameAsync(model.UserName);

                    if (User != null)
                    {
                        //step 2-check if User isLockOut   
                        if (await UserManager.IsLockedOutAsync(User.Id))
                        {
                            //get present time and lock out time difference                        
                            LockOutTime = GetLockOutTimeDifference(User.Id);

                            if (LockOutTime > 0)
                            {
                                Access = 0;
                                ModelState.AddModelError("", string.Format("Your account has been locked out for {0} minutes due to multiple failed login attempts.Remaining Time {1} minutes.", ConstMessage.DEFAULT_ACCOUNT_LOCKOUT_TIME_SPAN_MINUTES, LockOutTime));
                                //ConfigurationManager.AppSettings["DefaultAccountLockoutTimeSpan"].ToString()

                                // ViewBag.ApiStatus = ConstMessage.STATUS_LOGIN_FAILED_ID; //0
                                //ViewBag.Balance = ConstMessage.STATUS_SET_USER_BALANCE;  //-1
                                ViewBag.RemainingTime = LockOutTime;
                                ViewBag.LoginRemainingAttempt = "";
                                ViewBag.LockOut = true;
                            }

                        }

                        if (Access == 1)
                        {
                            //step 3 if LockOutTime<=0 then try to credential by user name and password
                            var validCredentials = await UserManager.FindAsync(model.UserName, model.Password);

                            if (validCredentials == null)
                            {
                                // int count = UserManager.GetAccessFailedCount(User.Id);
                                //To Do check again if user already lockout previous and new attempt 
                                //to try  with invalid password then check duration time 
                                //if time is access then User.AccessFailedCount==0


                                if (User.AccessFailedCount == 0)
                                {
                                    IdentityResult result = await UserManager.SetLockoutEndDateAsync(User.Id, DateTimeOffset.UtcNow.AddMinutes(4));
                                    // User.LockoutEndDateUtc = DateTime.Now.AddMinutes(10);
                                }

                                User.AccessFailedCount += 1;
                                IdentityResult updateUser = await UserManager.UpdateAsync(User);

                                // ViewBag.ApiStatus = ConstMessage.STATUS_LOGIN_FAILED_ID; //0
                                //ViewBag.Balance = ConstMessage.STATUS_SET_USER_BALANCE;  //-1
                                //ViewBag.RemainingTime = LockOutTime;
                                ViewBag.LoginRemainingAttempt = User.AccessFailedCount;
                                //ViewBag.LockOut = true;

                                if (updateUser.Succeeded && User.AccessFailedCount >= 3)
                                {

                                    LockOutTime = GetLockOutTimeDifference(User.Id);

                                    if (LockOutTime <= 0)
                                    {
                                        IdentityResult resetdate = UserManager.ResetAccessFailedCount(User.Id);
                                        IdentityResult lockuser = await UserManager.SetLockoutEnabledAsync(User.Id, true);
                                        IdentityResult result = await UserManager.SetLockoutEndDateAsync(User.Id, DateTimeOffset.UtcNow.AddMinutes(ConstMessage.DEFAULT_ACCOUNT_LOCKOUT_TIME_SPAN_MINUTES));
                                        ModelState.AddModelError("", string.Format("Your account has been locked out for {0} minutes due to multiple failed login attempts.", ConstMessage.DEFAULT_ACCOUNT_LOCKOUT_TIME_SPAN_MINUTES));
                                        // ConfigurationManager.AppSettings["DefaultAccountLockoutTimeSpan"].ToString()

                                        ViewBag.RemainingTime = ConstMessage.DEFAULT_ACCOUNT_LOCKOUT_TIME_SPAN_MINUTES;
                                        ViewBag.LoginRemainingAttempt = "";
                                        ViewBag.LockOut = true;
                                    }
                                    else
                                    {
                                        int reset = ResetUserLockOutInfo(User.Id);
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError("", string.Format("Invalid User Id or Password. You have {0} more attempt(s) before your account gets locked out.", ConstMessage.MAX_FAILED_ACCESS_ATTEMPTS_BEFORE_LOCKOUT - User.AccessFailedCount));
                                    //ViewBag.LockOut_After_Multiple_LogInFailed_In_Time ="Your acount has been locked out after failed "+3 +"Times in "+ ConstMessage.LOCKOUT_AFTER_MULTIPLE_LOGINFAILED_IN_TIME+ " minutes.";
                                    ModelState.AddModelError("", string.Format("Your account has been locked out after failed {0} Times in {1} minutes", 3, ConstMessage.LOCKOUT_AFTER_MULTIPLE_LOGINFAILED_IN_TIME));


                                    ViewBag.LoginRemainingAttempt = ConstMessage.MAX_FAILED_ACCESS_ATTEMPTS_BEFORE_LOCKOUT - User.AccessFailedCount;
                                }
                            }
                            else
                            {
                                Access = 0;
                                if (await UserManager.IsLockedOutAsync(User.Id) || UserManager.GetAccessFailedCount(User.Id) > 0)
                                {
                                    Access = ResetUserLockOutInfo(User.Id);
                                    if (Access == 1)
                                    {
                                        User.LockoutEndDateUtc = null;
                                        IdentityResult result = await UserManager.UpdateAsync(User);
                                    }

                                    // ViewBag.ApiStatus = ConstMessage.STATUS_LOGIN_FAILED_ID; //0
                                    //ViewBag.Balance = ConstMessage.STATUS_SET_USER_BALANCE;  //-1
                                    //ViewBag.RemainingTime = LockOutTime;
                                    ViewBag.LoginRemainingAttempt = User.AccessFailedCount;
                                    //ViewBag.LockOut = true;

                                }
                                else
                                    Access = 1;

                                if (Access == 1)
                                {
                                    await SignInAsync(User, model.RememberMe);
                                    AdminRepository admin_repo = new AdminRepository();
                                    CurrencyRepository currency_repo = new CurrencyRepository();
                                    ViewBag.ApiStatus = ConstMessage.STATUS_LOGIN_SUCCESS_ID;
                                    if (ConstMessage.SMSTransactionReportUsers.Contains(User.UserName))
                                        ViewBag.SMSTransactionReportTab_1 = "visible";
                                    else
                                        ViewBag.SMSTransactionReportTab_1 = "invisible";
                                    //ViewBag.LocalBalance = currency_repo.ConvertToLocal(User.CurrentBalance, User.LocalCurrencyId);
                                    ViewBag.ConvertFromUsd = currency_repo.GetConversionRate(User.LocalCurrencyId);
                                    ViewBag.Balance = User.CurrentBalance;
                                    ViewBag.ISO = admin_repo.GetUserCurrencyISOByUserName(User.UserName);

                                    ViewBag.LockOut = false;
                                    ViewBag.LoginRemainingAttempt = "";
                                    ViewBag.RemainingTime = "";

                                    if (!string.IsNullOrEmpty(User.NomineePhoneNumber))
                                    {
                                        ViewBag.NomineeExists = true;
                                    }
                                    else
                                    {
                                        ViewBag.NomineeExists = false;
                                    }

                                    return RedirectToLocal(returnUrl);
                                }
                                else
                                    ModelState.AddModelError("", "Please try again !");
                            }
                        }

                    } //
                    else
                    {
                        ModelState.AddModelError("", "Invalid User Id or Password!");
                        ViewBag.ForgotPassword = ConstMessage.ForgotPassword;
                    }
                }

                // If we got this far, something failed, redisplay form
                return View(model);
                //
            }
            catch (Exception)
            { 
            
            }
            return View("Error");       

           
        }

        [NonAction]
        public int GetLockOutTimeDifference(string UserID)
        {
            // int different = 0;

            DateTimeOffset getLockOutDate = UserManager.GetLockoutEndDate(UserID);
            DateTimeOffset currentDateTime = DateTimeOffset.UtcNow;
            int compare = DateTimeOffset.Compare(getLockOutDate, currentDateTime);

            if (compare > 0)
            {
                compare = Convert.ToInt32(getLockOutDate.Subtract(currentDateTime).TotalMinutes);

                //Formula biggerTime.Subtract(SmallerTime)
            }
            return compare;
        }

        public int ResetUserLockOutInfo(string UserId)
        {
            int result = 0;

            IdentityResult resetFailedCount = UserManager.ResetAccessFailedCount(UserId);

            if (resetFailedCount.Succeeded)
            { 
                IdentityResult lockuser = UserManager.SetLockoutEnabled(UserId, false);
                if (lockuser.Succeeded)
                {  
                    
                    result = 1;
                }
            }
            return result;
        }

        
        public ActionResult PasswordNotification(string Pass,string Message)
        {
            ViewBag.Pass = Pass;
            ViewBag.Message = Message;

            return View();
        }
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model,bool? FromApi)
        {
            if (ModelState.IsValid)
            {
                var user = new eBankingUser() { UserName = model.UserName };

                user.Email = user.Name = user.OtherConductInfo = user.MotherName = user.FatherName = user.DateofBirth = user.PresentAddress = user.ParmanentAddress = null;
                user.IsActive = true;
                user.CreatedDate = DateTime.Now;
                user.LocalCurrencyId = ConstMessage.SELECTED_BAN_DESTINATION_ID;
                user.NomineePhoneNumber = model.NomineePhoneNumber;

                //Create a Random Password for this User within 8 to 10 charecter

                model.Password = GenaratePassword.NumericPasswordCreator();
                    
                try
                {
                    //initial 
                    ViewBag.ApiStatus = ConstMessage.STATUS_REGISTER_FAILED_ID; //0

                    var result = await UserManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        #region create registered role if not exixts
                        //ViewBag.ApiStatus = ConstMessage.STATUS_REGISTER_SUCCESS_ID; //2

                        //check is any role has of the name ConstMessage.ROLE_REGISTERED_NAME
                        var IsRoleExists = await RoleManager.FindByNameAsync(ConstMessage.ROLE_REGISTERED_NAME);
                        
                        //if not create a role
                        if (IsRoleExists == null)
                        {
                            eBankingRole newRole = new eBankingRole();
                            newRole.Name = ConstMessage.ROLE_REGISTERED_NAME;
                            var createRole = await RoleManager.CreateAsync(newRole);
                        }
                        #endregion
                        //assign role to user
                        var u = await UserManager.AddToRoleAsync(user.Id, ConstMessage.ROLE_REGISTERED_NAME);

                        //if success
                        if (u.Succeeded)
                        {
                            //sending user a notification message using smsMessageHelper
                            //this is global vendor for sending message to a user mobile number for any country
                            
                            //comment-New user Password Sending By SMS
                           /*
                            _variable.Message = message.SendMessage(model.UserName, ConstMessage.SMS_SENDER_SendingMessage + model.Password, ConstMessage.SMS_SELECTED_VENDOR);
        
                            


                            if ((ConstMessage.SMS_SELECTED_VENDOR == ConstMessage.SMS_VENDOR_ID_SINFINI && _variable.Message == ConstMessage.SMS_SENDER_SUCCESS_STATUS) ||
                                (ConstMessage.SMS_SELECTED_VENDOR == ConstMessage.SMS_VENDOR_ID_EZZE && _variable.Message == ConstMessage.SMS_SENDER_SUCCESS_STATUS_EZZE))
                            {
                               ViewBag.RegisterPassword = ConstMessage.SMS_SENDER_SendingMessage_For_APK;
                                
                            }
                            else
                            {
                                //todo log file 
                            }
                             */

                            ViewBag.ApiStatus = ConstMessage.STATUS_REGISTER_ROLE_SUCCESS_ID; //1    
                            //ViewBag.ApiStatus = ConstMessage.STATUS_REGISTER_SUCCESS_ID;
                            ViewBag.RegisterPassword = model.Password;

                            //sign In for genarate cookies
                            await SignInAsync(user, isPersistent: false);
                        }

                        if (FromApi == true)
                            return Json(new { Pass = model.Password, message = ConstMessage.Registration_successfulMessage }, JsonRequestBehavior.AllowGet);

                        return RedirectToAction("PasswordNotification", "Account", new { Pass = model.Password, message = ConstMessage.Registration_successfulMessage });
                       
                    }
                    else if(result.Errors != null)
                    {
                        foreach(var error in result.Errors)
                        {
                            if (error.Contains("taken"))
                                ViewBag.ApiStatus = ConstMessage.STATUS_REGISTER_USERNAME_USED;
                        }
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
                catch (Exception)
                {

                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [NonAction]
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
        {
            ManageMessageId? message = null;
            IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("Manage", new { Message = message });
        }

       
        // GET: /Account/Manage
      
        public ActionResult Manage(ManageMessageId? message,string NewPass,int? Reset)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed.Your new Password "+NewPass
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";

            if(Reset==null)
            ViewBag.HasLocalPassword = HasPassword();

            ViewBag.ReturnUrl = Url.Action("Manage");

            //ViewBag.Menu = user_menu.GenarateMenu(HttpContext.User.Identity.Name);
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            bool hasPassword = HasPassword();
            ViewBag.HasLocalPassword = hasPassword;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasPassword)
            {
                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        ViewBag.ApiStatus = ConstMessage.STATUS_PASSWORD_CHANGE_SUCCESS_ID; //1
             

                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess, NewPass=model.NewPassword });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }
            else
            {
                // User does not have a password so remove any validation errors caused by a missing OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    else
                    {
                        AddErrors(result);
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [NonAction]
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        [NonAction]
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // If the user does not have an account, then prompt the user to create an account
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }

        [NonAction]
        // POST: /Account/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
        }

        [NonAction]
        // GET: /Account/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            if (result.Succeeded)
            {
                return RedirectToAction("Manage");
            }
            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
        }

        [NonAction]
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new eBankingUser() { UserName = model.UserName };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();

            ViewBag.ApiStatus = ConstMessage.STATUS_LOGG_OFF_SUCCESS_ID;
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ApiForgotPassword(string UserName, string NomineePhoneNumber, string ClientId)
        {

            var User = UserManager.FindByNameAsync(UserName);
            AdminRepository admin_repo = new AdminRepository();
            var SelectedUser = admin_repo.GetUserByUserName(UserName);
            if (SelectedUser != null && !string.IsNullOrEmpty(SelectedUser.NomineePhoneNumber))
            {
                AdminController admin = new AdminController();
                ChangeUserPasswordVM pvm = new ChangeUserPasswordVM();
                pvm.AutoGenerated = true;
                pvm.UserName = UserName;
                pvm.SendSms = true;
                string newPass = GenaratePassword.NumericPasswordCreator();
                pvm.NewPassword = newPass;
                var changePassResult = admin.ChangeUserPassword(pvm,"").Result;

                System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                var temp = (serializer.Serialize(changePassResult));

                var obj = Newtonsoft.Json.Linq.JObject.Parse(temp);
                var ApiStatus = (int)obj["Data"]["Status"];

                if (ApiStatus == ConstMessage.STATUS_COMPLETE_ID)
                {
                    _variable.Message = message.SendMessage(UserName, ConstMessage.SMS_SENDER_SendingMessage + newPass, ConstMessage.SMS_SELECTED_VENDOR);

                    if ((ConstMessage.SMS_SELECTED_VENDOR == ConstMessage.SMS_VENDOR_ID_SINFINI && _variable.Message == ConstMessage.SMS_SENDER_SUCCESS_STATUS) ||
                                (ConstMessage.SMS_SELECTED_VENDOR == ConstMessage.SMS_VENDOR_ID_EZZE && _variable.Message == ConstMessage.SMS_SENDER_SUCCESS_STATUS_EZZE))
                    {
                        ViewBag.RegisterPassword = ConstMessage.SMS_SENDER_SendingMessage_For_APK;
                        ViewBag.Message = "Password has been changed. Your new password has been sent to the User Phone Number through SMS.";
                        ViewBag.ApiStatus = ConstMessage.STATUS_COMPLETE_ID;
                    }
                    else
                    {
                        ViewBag.Message = "Password has been changed.";
                        ViewBag.ApiStatus = ConstMessage.STATUS_COMPLETE_ID;
                    }
                }

            }
            else
            {
                ViewBag.Message = "Unable to resend password. Please try again or contact administrator.";
                ViewBag.ApiStatus = ConstMessage.STATUS_FAILED_ID;
            }


            return Json(new { ViewBag.ApiStatus, ViewBag.Message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SetNomineeNumber(string NomineeNumber)
        {
            AdminRepository admin_repo = new AdminRepository();
            var SelectedUser = admin_repo.GetUserByUserName(User.Identity.Name);//admin_repo.GetUserByUserName(UserName);
            ViewBag.ApiStatus = ConstMessage.STATUS_FAILED_ID;
            if (string.IsNullOrEmpty(SelectedUser.NomineePhoneNumber))
            {
                SelectedUser.NomineePhoneNumber = NomineeNumber;
                if (admin_repo.UserUpdate(SelectedUser))
                {
                    ViewBag.ApiStatus = ConstMessage.STATUS_COMPLETE_ID;
                    ViewBag.Message = "Nominee number has been set";
                }
                else
                {
                    ViewBag.Message = "Nominee number has not been set";
                }
            }
            ViewBag.Message = "Nominee number exists.";

            return Json(new { ViewBag.ApiStatus, ViewBag.Message }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SetLocalCurrency(int CurrencyId)
        {
            AdminRepository admin_repo = new AdminRepository();
            CurrencyRepository currency_repo = new CurrencyRepository();
            var SelectedUser = admin_repo.GetUserByUserName(User.Identity.Name);//admin_repo.GetUserByUserName(UserName);
            ViewBag.ApiStatus = ConstMessage.STATUS_FAILED_ID;

            SelectedUser.LocalCurrencyId = CurrencyId;
            if (admin_repo.UserUpdate(SelectedUser))
            {
                ViewBag.ApiStatus = ConstMessage.STATUS_COMPLETE_ID;
                ViewBag.Message = "Local Currency has been set";
            }
            else
            {
                ViewBag.Message = "Local Currency has not been set";
            }
            var reselectUser = admin_repo.GetUserByUserName(User.Identity.Name);
            ViewBag.ConvertFromUsd = currency_repo.GetConversionRate(reselectUser.LocalCurrencyId);
            ViewBag.Balance = reselectUser.CurrentBalance;
            ViewBag.ISO = admin_repo.GetUserCurrencyISOByUserName(reselectUser.UserName);
            return Json(new { ViewBag.ApiStatus, ViewBag.Message, ViewBag.ConvertFromUsd, ViewBag.Balance, ViewBag.ISO }, JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveAccountList()
        {
            var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
            ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
            return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
        }

    

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
              UserManager.Dispose();
              UserManager = null;
            }
            base.Dispose(disposing);
        }


        #region Helpers


        [NonAction]
        public bool UserIsInRole(string RoleName)
        {
            //if user has this role then returns true else false 
            _variable.ReturnsBooleanResult= RoleManager.RoleExists(RoleName);

            return _variable.ReturnsBooleanResult;
        
        }




        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

   

        private async Task SignInAsync(eBankingUser user, bool isPersistent)
        {
            try
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
            }
            catch (Exception) { }
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            //var user = UserManager.FindById(User.Identity.GetUserId());

            var user =UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        
        #endregion
    }
}
//public static class GetUserHostInfo
//{
//    public static IPAddress GetIp(this HttpRequest request)
//    {
//        string ipString;
//        if (string.IsNullOrEmpty(request.ServerVariables["HTTP_X_FORWARDED_FOR"]))
//        {
//            ipString = request.ServerVariables["REMOTE_ADDR"];
//        }
//        else
//        {
//            ipString = request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
//                .FirstOrDefault();
//        }

//        IPAddress result;
//        if (!IPAddress.TryParse(ipString, out result))
//        {
//            result = IPAddress.None;
//        }

//        return result;
//    }
//}