using eBanking.Abstract;
using eBanking.App_Code;
using eBanking.App_Start;
using eBanking.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using eBanking.Interface;

namespace eBanking.Controllers
{
    [CustomAuth]
    public class AdminController : Controller
    {
        private AdminRepository adminReposiotry;
        private TransactionRepository transaction_repo;
     //   private VendorTransactionRepository vendorTransaction_repo;
        private VendorRepository vendor_repo;
        private ServiceRepository service_repo;
        private eBankingDbContext db;
        private PinRepository pin_repo;

        private Variable _variable;

        private UserMenuGenarator adminUserRole;

        private IdentityConfig ObjIdentity;
        public AdminController()
        {
         //   vendorTransaction_repo = new VendorTransactionRepository();
            vendor_repo = new VendorRepository();
            transaction_repo = new TransactionRepository();
            adminReposiotry = new AdminRepository();
            service_repo = new ServiceRepository();
            ObjIdentity = new IdentityConfig();
            db = new eBankingDbContext();
            _variable = new Variable();
            adminUserRole = new UserMenuGenarator();
            pin_repo = new PinRepository();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (ObjIdentity.UserManager != null || ObjIdentity.RoleManager != null))
            {
                if (ObjIdentity.UserManager != null)
                {
                    ObjIdentity.UserManager.Dispose();
                    ObjIdentity.UserManager = null;
                }
                if (ObjIdentity.RoleManager != null)
                {
                    ObjIdentity.RoleManager.Dispose();
                    ObjIdentity.RoleManager = null;
                }

            }

            base.Dispose(disposing);
        }



        protected override void HandleUnknownAction(string actionName)
        {
            ViewData["name"] = actionName;
            View("Error").ExecuteResult(this.ControllerContext);
        }

/*        public ActionResult SearchUser( String ByName, String Email, String UserRoles)
        {
            try
            {
                ViewBag.Result = "Message";
  //              List<Transaction> tran = eBankingTask.TransactionReportsGenerator(transaction_repo.GetAll(),service_repo.GetAll().Where(x => x.ParentId == ConstMessage.SERVICES_MONEYTRANSFER_ID).Select(x=>x).ToList(),StatusList,destination_repo.GetAll(),null);
    //            ChangeIsTimeout();
   //             List<AdminController> adming =eBankingTask
                if(!string.IsNullOrEmpty(ByName) && ByName != null)
                {
  //                  tran = tran.Where(x => x.UserId.Contains(ByName)).Select(x => x).ToList();
                }
            }
            catch(Exception ex)
            {
                //String a = ex.Message;
            }
            return View("Error");
        }
 */
        // GET: /Admin/
       
        //display all registered user list in a view model

        public IQueryable<UserEditViewModel> GetAllQueryable()
        {
            try
            {
  //              return db.Users;
            }
            catch (Exception) { }
            return null;
        }

//[ValidateAntiForgeryToken]
        public ActionResult TransactionRecords()
        {
            //user balance
            try
            {
            IAdminRepository admin_Repo = new AdminRepository();
            var users = admin_Repo.GetAll_User();
            var userBalance = users.Sum(x => x.CurrentBalance);

            //vendor balance
            var vendors = vendor_repo.Vendor_GetAll();
            var vendorBalance = vendors.Sum(x => x.CurrentBalance);

            //distributor balance
            IDistributorRepository distributor_repo = new DistributorRepository();
            var distributors = distributor_repo.Distributor_GetAll();
            var distributorBalance = distributors.Sum(x => x.DistributorBalance);

            //transaction
            DateTime dt = DateTime.Today;
            var tr = transaction_repo.GetAll().Where( x => x.TransactionDate >= (DateTime.Today));
            var totalTransaction = tr.Sum(x => x.AmountOut);
            var topupTransaction = tr.Where(x => x.ServiceId == 5).Sum(x => x.AmountOut);
            var moneyTransferTransaction = tr.Where(x => x.ServiceId == 1).Sum(x => x.AmountOut);
            var returnBalance = tr.Where(x => x.DistributorCommission > 0).Sum(x => x.AmountIN);

            //Pin

            var pins = pin_repo.GetAll();
            var pinNumber = pins.Count();
            var activePinNumber = pins.Where(x => x.IsActive.Equals(true)).Count();
            var activePinBalance = pins.Where(x => x.IsActive.Equals(true)).Sum(x => x.Value);
            var usedPinNumber = pins.Where(x => x.Status == 102).Count();
            var usedPinBalance = pins.Where(x => x.Status == 102).Sum(x => x.Value);

            List<TransactionRecordsVM> tRVM = new List<TransactionRecordsVM>();
            //tRVM = new TransactionRecordsVM();
            
            
            
            

            TransactionRecordsVM trvm = new TransactionRecordsVM();
            foreach (var item in vendors)
            {
                //string[item] name = item.Name;
                //decimal[item balance = item.CurrentBalance;
                if (item.Id == 1)
                    ViewBag.babulslt = item.CurrentBalance;
                if (item.Id == 2)
                    ViewBag.ezzeTopUp = item.CurrentBalance;
                if (item.Id == 3)
                    ViewBag.AbrarTel = item.CurrentBalance;
                //if (item.Id == 4)
                //    ViewBag.Swift = item.CurrentBalance;
                //else
                //    ViewBag.Swift = 0;

                trvm.VendorsBalance = item.CurrentBalance;
                trvm.VendorName = item.Name;
            }
            ViewBag.VendorsBalance = trvm.VendorsBalance;


            var services = service_repo.GetAll(true);

            var rawItems = service_repo.CreateTree(service_repo.GetAll(true), 0); //
            var ServiceList = new SelectList(rawItems, "Id", "Name", "ParentName", 1);
            ViewBag.ServiceList = ServiceList;

            var TotalCustomerTransaction = tr.Sum( x => x.AmountOut * x.ConversationRate);
            ViewBag.TotalCustomerTransaction = TotalCustomerTransaction;

      //      var ata = tr.Sum( x => x.AmountOut * x.ConversationRate)( x => x.ServiceId).
            List<TransactionRecordsVM> TopServices  = null;
             TopServices = (from tran in tr
                               //join s in services on tran.ServiceId equals s.Id into serviceJoin
                               //from sj in serviceJoin.DefaultIfEmpty()
                                                      group tran by tran.ServiceId into g
                                               // let   TopServiceTotalCount = g.Count(x => x.Status)   
                                                 orderby g.Sum(x => x.AmountOut) descending
                            select new TransactionRecordsVM
                                                     {
                                                         TopServiceId = g.First().ServiceId,
                                                         TopServiceName = g.First().ServiceName,
                                                         //TopServiceTotalCount = g.Count(x => x.ServiceId),
                                                         TopServiceProfit = g.Sum(x => x.AmountOut)
                                                     }).ToList();
           
           var topServices = TopServices.Take(5);

     //      ViewBag.topServices = topServices;
            var CustomerTransactionForEachService0 = (from tran in tr
                                                      join s in services on tran.ServiceId equals s.Id
                                                     group tran by tran.ServiceId into trang
                                                     select new
                                                     {
                                                         ServiceId = trang.Select( x => x.ServiceId),
                                                         ServiceName = trang.Select( x => x.ServiceName),
                                                         TransactionForEachService = trang.Sum(x => x.AmountOut * x.ConversationRate)
                                                     }).ToArray();
            
            var TransactionAmountsForEachService = CustomerTransactionForEachService0.Select(x => x.TransactionForEachService);
            var EachServiceName = CustomerTransactionForEachService0.Select(x => x.ServiceName);
            //decimal a = TransactionAmounts;

           // var ServiceId = tr.Select(x => x.ServiceId).ToArray();
                        
            var CustomerTransactionForEachService = (from tran in tr
                                                     join s in services on tran.ServiceId equals s.Id
                                                     group tran by tran.ServiceId into trang
                                                     select new
                                                     {
                                           //            ServiceId = services.Select(x => x.Id),
                                                         TransactionForEachService = trang.Sum(x => x.AmountOut * x.ConversationRate)
                                                     }).ToArray();
          //int[] CustomerTransactionForEachService4 = (int)CustomerTransactionForEachService;
            ViewBag.ServiceAmount = CustomerTransactionForEachService;
            var CustomerTransactionForEachService2 = (from tran in tr
                                                      select tran.ServiceId).ToArray();

            //var CustomerTransactionForEachService3 = (from tran in tr
            //                                          group tran by tran.ServiceId into trang
            //                                          select new
            //                                          {
            //                                              trang.ServiceId
            //                                          }).ToArray();
        
            DateTime today = DateTime.Today;         
           // DateTime dt = Convert.ToDateTime(today);           
            tr = tr.Where(x => x.TransactionDate >= today);

            var PurchaseByCustomerToday = tr.Sum(x => x.AmountOut * x.ConversationRate);
            ViewBag.PurchaseByCustomerToday = PurchaseByCustomerToday;
            
            int TotalTransactionCountToday = tr.Count();
            ViewBag.TotalTransactionCountToday = TotalTransactionCountToday; 
           
           // decimal PurchaseFromVendorToday = vendors.Where(x => x.CreatedOn.Equals(DateTime.Today)).Sum(x => (decimal?) x.AmountOutUSD) ?? 0 ;
          //  ViewBag.PurchaseFromVendorToday = PurchaseFromVendorToday;

            var TotalVendorsBalance = vendors.Sum(x => x.CurrentBalance);
            ViewBag.TotalVendorsBalance = TotalVendorsBalance;

            var EachVendorsCurrentBalance = (from v in vendors
                                             group v by v.Name into vng
                                             select new
                                             {
                                                 Name = vng.Key,
                                                 CurrentBalance = vng.Sum(x => x.CurrentBalance)
                                             });
            
            var EachVendorsCurrentBalance2 = vendors.Sum(x => x.CurrentBalance);
            ViewBag.AllVendorsCurrentBalance = EachVendorsCurrentBalance;
            
            //Active Pins and total pin value
            int PinCount = pins.Count();
            decimal PinAmount = pins.Where(x => x.IsActive).Sum(x => x.Value);
            ViewBag.PinAmount = PinAmount;
            decimal[] cost = { 5, 9, 8, 6, 7, 58, 66 };
            ViewBag.Cost = cost;
            decimal[] Deducted = { 9, 6, 7, 8, 2, 7, 18 };
            ViewBag.Deducted = Deducted;
            var cd = new TransactionRecordsVM();
            //new values
            cd.userBalance = userBalance;
            cd.vendorBalance = vendorBalance;
            cd.distributorBalance = distributorBalance;

            cd.topupTransaction = topupTransaction;
            cd.moneyTransferTransaction = moneyTransferTransaction;
            cd.returnBalance = returnBalance;

            cd.pinNumber = pinNumber;
            cd.activePinNumber = activePinNumber;
            cd.activePinBalance = activePinBalance;
            cd.usedPinNumber = usedPinNumber;
            cd.usedPinBalance = usedPinBalance;
            cd.totalTransaction = totalTransaction;

            //old values
            //cd.TotalVendorsBalance = vendors.CurrentBalance;
            cd.PurchaseByCustomerToday = PurchaseByCustomerToday;
      //      cd.PurchaseFromVendorToday = PurchaseFromVendorToday;
            cd.TotalTransactionCountToday = TotalTransactionCountToday;
            cd.VendorsBalance = TotalVendorsBalance;
            cd.TotalCustomerTransaction = TotalCustomerTransaction;
            //cd.CustomerTransactionForEachService = CustomerTransactionForEachService;
           // cd.Deducted = Deducted;
            //cd.Cost = Cost;
            //cd.TopServices = TopServices;
           // cd.PinCount = PinCount;
            cd.PinAmount = PinAmount;
     //       cd.Status = Status;

            tRVM.Add(cd);
            ViewBag.ServiceId = CustomerTransactionForEachService2;
            
            return View(tRVM);
        }
        catch(Exception)
    {

    }
            return null;
        }

       //AdminRepository:IAdminRepository
        public ActionResult Index(string Name, string UserRole, int? page, int? CountryCode)
        {
            int pageSize = ConstMessage.ITEMS_PER_PAGE;
            int pageNumber = (page ?? 1);
            ViewBag.Name = Name;
            ViewBag.UserRoleSelected = UserRole;
            ViewBag.SelectedCountry = CountryCode;
            ViewBag.UserRole = new SelectList(db.Roles.ToList(), "Name", "Name");
            ViewBag.CountryCodeList = new SelectList(db.Destinations.Where(x => x.CountryCode != null).ToList(), "Id", "CountryCode");
            
            try
            {
                IEnumerable<UserEditViewModel> List = null;
                List  = adminReposiotry.SearchByUserAndRole(Name, UserRole, CountryCode);
                return View(List.ToPagedList(pageNumber,pageSize));
            }
            catch (Exception)
            {
                // string a = mx.Message;
                return View("Error");
            }
            //  return View();
        }


        //create a registered 
        [HttpGet]
        public ActionResult Create(int? ajaxLoad)
        {
            try
            {
                // var rolelist = db.Roles.ToList();
                ViewData["RoleName"] = new SelectList(db.Roles.ToList(), "Name", "Name");
                //ViewBag.RoleName = new SelectList(db.Roles.ToList(), "Name", "Name");
            }
            catch (Exception ex)
            {
                string a = ex.Message.ToString();
                return View("Error");
            }
            //"Id", "Name" if want to get Id by selected Name
            if (ajaxLoad == 1)
                return PartialView();
            else
            {

                //genrate menu
                //AdminUserRole adminUserRole = new AdminUserRole();
                _variable.UserName = HttpContext.User.Identity.Name;

                //ViewBag.Menu = null;

                //if (_variable.UserName != null && !string.IsNullOrEmpty(_variable.UserName))
                //{
                //    ViewBag.Menu = adminUserRole.GenarateMenu(_variable.UserName);

                //}

            }
            return View();
        }


        //create a registered user and assigned a role to this user
        [HttpPost]
        public ActionResult Create(UserCreateViewModel user_create)
        {


            if (ModelState.IsValid && user_create.RoleName != "" && !string.IsNullOrEmpty(user_create.RoleName))
            {
                var user = new eBankingUser { UserName = user_create.UserName };
                user.Email = user_create.Email;
                user.CreatedDate = DateTime.Now;

                IdentityResult result = ObjIdentity.UserManager.Create(user, user_create.Password);
                if (result.Succeeded)
                {
                    var assignRole = ObjIdentity.UserManager.AddToRole(user.Id, user_create.RoleName);

                    if (!assignRole.Succeeded)
                    {
                        //then delete user and give a error message
                        ModelState.AddModelError("", "Can not assign User to Role " + user_create.RoleName + ".Try again!!");
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    // ModelState.AddModelError("", "User Creation failed.");
                    AddErrors(result);
                }

            }

            //else mean not valid entry so genrate menu
            ViewBag.RoleName = new SelectList(db.Roles.ToList(), "Name", "Name", user_create.RoleName);

            //genrate menu

            _variable.UserName = HttpContext.User.Identity.Name;

            //ViewBag.Menu = null;

            //if (_variable.UserName != null && !string.IsNullOrEmpty(_variable.UserName))
            //{
            //    ViewBag.Menu = adminUserRole.GenarateMenu(_variable.UserName);

            //}
            return View(user_create);

        }

        //edit a user user name,other info and changed Assigned role 

        [HttpGet]
        public ActionResult Edit(int? ajaxLoad, string name)   //string id,
        {
            UserEditViewModel model = new UserEditViewModel();

            if (name == null && string.IsNullOrEmpty(name))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var findUser = ObjIdentity.UserManager.FindByName(name);

            if (findUser == null)
            {
                return HttpNotFound();
            }
            else
            {
                model.UserId = findUser.Id;
                model.UserName = findUser.UserName;
                model.Email = findUser.Email;
                model.IsActive = findUser.IsActive;
                model.DistributorCode = findUser.DistributorCode;
            }

            _variable.oldRoleName = FindCurrentUserRole(findUser);
            ViewData["RoleName"] = new SelectList(db.Roles.ToList(), "Name", "Name", _variable.oldRoleName);
            //ViewBag.RoleName = new SelectList(db.Roles.ToList(), "Name", "Name", _variable.oldRoleName);

            if (ajaxLoad == 1)
                return PartialView(model);

            else
            {
                #region ReturnWithLayout

                //then return with layout Menu
                //genrate menu

                UserMenuGenarator adminUserRole = new UserMenuGenarator();
                _variable.UserName = HttpContext.User.Identity.Name;

                //ViewBag.Menu = null;

                //if (_variable.UserName != null && !string.IsNullOrEmpty(_variable.UserName))
                //{
                //    ViewBag.Menu = adminUserRole.GenarateMenu(_variable.UserName);
                //}

                #endregion

                return View(model);
            }
        }


        //edit a user user name,other info and changed Assigned role 
        [HttpPost]
        public ActionResult Edit(UserEditViewModel model, string RoleName)
        {
            // get user object from the storage
            var user = ObjIdentity.UserManager.FindById(model.UserId);

            // change username and email

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.IsActive = model.IsActive;
            user.DistributorCode = model.DistributorCode;

            IdentityResult updateUser = ObjIdentity.UserManager.Update(user);

            //get this user current Role
            _variable.oldRoleName = FindCurrentUserRole(user);

            //then update Add to role if role is chnaged
            if (updateUser.Succeeded)
            {
                try
                {

                    //check this user oldRoleName with our givenRoleName
                    //if don't match then remove oldrole and add new role to this user
                    //still no role assigned
                    if (_variable.oldRoleName != null && !string.IsNullOrEmpty(_variable.oldRoleName) && _variable.oldRoleName!=ConstMessage.RoleNotFound)
                    {
                        if (_variable.oldRoleName != RoleName && _variable.oldRoleName != null && !string.IsNullOrEmpty(_variable.oldRoleName))
                        {
                            //should create a logg file if exception

                            ObjIdentity.UserManager.RemoveFromRole(user.Id, _variable.oldRoleName);
                            ObjIdentity.UserManager.AddToRole(user.Id, model.RoleName);

                        }
                    }
                    else
                    { 
                      //still has no role so assign a role
                        ObjIdentity.UserManager.AddToRole(user.Id, model.RoleName);
                    }
                    return RedirectToAction("Index");



                }
                catch (Exception)
                {
                    return View("Error");
                }
            }
            else
            {

                #region GenarateErroWithLayout

                AddErrors(updateUser);
                //then return with layout Menu
                //genrate menu

                UserMenuGenarator adminUserRole = new UserMenuGenarator();
                _variable.UserName = HttpContext.User.Identity.Name;

                //ViewBag.Menu = null;

                //if (_variable.UserName != null && !string.IsNullOrEmpty(_variable.UserName))
                //{
                //    ViewBag.Menu = adminUserRole.GenarateMenu(_variable.UserName);
                //}

                ViewBag.RoleName = new SelectList(db.Roles.ToList(), "Name", "Name", _variable.oldRoleName);
                #endregion
            }
            return View(model);

        }


        //find out a user assigned Role
        [NonAction]
        public string FindCurrentUserRole(IdentityUser findUser)
        {
            //check if user is in our modified role                   
            //get RoleId of this user from aspnetUserRole table 
            //if no role found it get nullreference exception Object reference not set to an instance of an object.

            try
            {
                //get this user current role
                _variable.oldRoleId = findUser.Roles.SingleOrDefault().RoleId;

                //get matching the RoleId with aspnetRoles table
                _variable.oldRoleName = db.Roles.SingleOrDefault(r => r.Id == _variable.oldRoleId).Name;

            }
            catch (Exception ex)
            {
                string a = ex.Message;
                if (_variable.oldRoleId == null)
                    return ConstMessage.RoleNotFound;
                else
                return null;
            }

            return _variable.oldRoleName;
        }

        [NonAction]
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }


        //Changed a user password 
        [HttpPost]
        public async Task<ActionResult> ChangeUserPassword(ChangeUserPasswordVM changePass, string Message)
        {
            _variable.ApiStatus = ConstMessage.STATUS_FAILED_ID;
            ViewBag.Message = Message;
            if (changePass != null)
            {
                if ((changePass.Id == null || changePass.Id.Length < 0) && changePass.UserName.Length > 0)
                {
                    var validuser = db.Users.Where(x => x.UserName == changePass.UserName).SingleOrDefault();
                    if (validuser != null)
                        changePass.Id = ObjIdentity.UserManager.FindByName(changePass.UserName).Id;
                    else
                        return RedirectToAction("ChangeUserPassword", new { changePass, Message = "Invalid User" });
                }
                if (changePass.Id.Length > 0)
                {
                    //changePass.UserName = db.Users.Find(changePass.Id).Name; //db.Users.Where(i => i.Id == Id).FirstOrDefault().Name;

                    if (changePass.NewPassword != null && changePass.NewPassword.Length > 0)
                    {
                        IdentityResult remove = await ObjIdentity.UserManager.RemovePasswordAsync(changePass.Id);
                        if (remove.Succeeded)
                        {
                            IdentityResult reset = await ObjIdentity.UserManager.AddPasswordAsync(changePass.Id, changePass.NewPassword);
                            if (reset.Succeeded)
                            {
                                _variable.ApiStatus = ConstMessage.STATUS_COMPLETE_ID;
                                changePass.NewPassword = null;
                                ModelState.Clear();
                                if (changePass.SendSms != null && (bool)changePass.SendSms)
                                {
                                    return Json(new { Status = _variable.ApiStatus }, JsonRequestBehavior.AllowGet);
                                }
                                return RedirectToAction("ChangeUserPassword", new { changePass, Message = "SUCCESS" });
                            }
                        }
                        changePass.NewPassword = null;
                        ModelState.Clear();
                        if (changePass.SendSms != null && (bool)changePass.SendSms)
                        {
                            return Json(new { Status = _variable.ApiStatus }, JsonRequestBehavior.AllowGet);
                        }
                        return RedirectToAction("ChangeUserPassword", new { changePass, Message = "ERROR" });
                    }

                }

            }


            return View(changePass);
        }

        //
        [HttpGet]
        public ActionResult ChangeUserPassword(string UserName,string Message)
        {
            ViewBag.Message = Message;

            if (UserName != null && !string.IsNullOrEmpty(UserName))
            {
                var User = ObjIdentity.UserManager.FindByName(UserName);

                if (User != null)
                {
                    ChangeUserPasswordVM changePass = new ChangeUserPasswordVM();

                    changePass.Id = User.Id;
                    changePass.UserName = User.UserName;


                    return View(changePass);
                }
            }

            return View();
        }
        //[HttpGet]
        //public ActionResult ChangeUserPasswordByUserName(string UserName, string Message)
        //{
        //    ViewBag.Message = Message;

        //    if (UserName != null && !string.IsNullOrEmpty(UserName))
        //    {
        //        var User = ObjIdentity.UserManager.FindByName(UserName);

        //        if (User != null)
        //        {
        //            ChangeUserPasswordVM changePass = new ChangeUserPasswordVM();

        //            changePass.Id = User.Id;
        //            changePass.UserName = User.UserName;


        //            return View(changePass);
        //        }
        //    }

        //    return View();
        //}

        //[HttpPost]
        //public ActionResult ChangeUserPassword(ChangeUserPasswordVM changePass)
        //{
        //   // ViewBag.Message = Message;
        //    if (changePass != null && changePass.Id.Length > 0)
        //    {
        //        //changePass.UserName = db.Users.Find(changePass.Id).Name; //db.Users.Where(i => i.Id == Id).FirstOrDefault().Name;

        //        if (changePass.NewPassword != null && changePass.NewPassword.Length > 0)
        //        {
        //            IdentityResult remove = ObjIdentity.UserManager.RemovePassword(changePass.Id);
        //                //await ObjIdentity.UserManager.RemovePasswordAsync(changePass.Id);
        //            if (remove.Succeeded)
        //            {
        //                IdentityResult reset = ObjIdentity.UserManager.AddPassword(changePass.Id,changePass.NewPassword);
                          
        //                if (reset.Succeeded)
        //                {
        //                    changePass.NewPassword = null;

        //                    return RedirectToAction("ChangeUserPassword", new { Message = "SUCCESS" }); //changePass,
        //                }
        //            }
        //            changePass.NewPassword = null;

        //            ModelState.AddModelError("", "Password not changed, please try again.");

        //          //  return RedirectToAction("ChangeUserPassword", new { changePass, Message = "ERROR" });
        //        }

        //    }

        //    return View(changePass);
        //}
        #region Helpers
        public bool UpdateUserRole(string NewRoleName, string UserName)
        {
            try
            {
                var user = ObjIdentity.UserManager.FindByName(UserName);
                string OldRoleName = FindCurrentUserRole(user);
                if (OldRoleName != null && !string.IsNullOrEmpty(OldRoleName) && OldRoleName != ConstMessage.RoleNotFound)
                {
                    if (OldRoleName != NewRoleName && OldRoleName != null && !string.IsNullOrEmpty(OldRoleName))
                    {
                        //should create a logg file if exception

                        ObjIdentity.UserManager.RemoveFromRole(user.Id, OldRoleName);
                        ObjIdentity.UserManager.AddToRole(user.Id, NewRoleName);
                        return true;

                    }
                }
                else
                {
                    //still has no role so assign a role
                    ObjIdentity.UserManager.AddToRole(user.Id, NewRoleName);
                    return true;
                }
            }
            catch (Exception) { }
            return false;
        }
        #endregion
    }
}