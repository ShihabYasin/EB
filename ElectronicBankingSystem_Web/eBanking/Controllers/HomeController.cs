using eBanking.App_Code;
using eBanking.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using eBanking.Abstract;
using eBanking.Interface;

namespace eBanking.Controllers
{
    
   [CustomAuth]
    public class HomeController : Controller
    {
        #region Repository, Context And Variable Declaration

        private eBankingDbContext db = new eBankingDbContext();
        private UserMenuGenarator user_menu=new UserMenuGenarator();
        private Variable _variable = new Variable();

        #endregion

       //Display eBanking Home Page
        public ActionResult Index()
        {

            //check user if the user is csd1 then through the user in ManageTransfer page
            //so csd1 user does not access other page or menu

            //get user name
            var UserName = HttpContext.User.Identity.Name;

           //check user name 
            //if (UserName == ConstMessage.StaticUserName)
            //{ 
            //    return RedirectToAction("ManageTransfer","Transaction");
            //}

            //check if the user has this role
            _variable.ReturnsBooleanResult = false;
            var UserRoleInfo = user_menu.GetUserRole(UserName);

            if (UserRoleInfo != null)
            {
                try
                {
                    var result = db.Roles.Where(x => x.Id == UserRoleInfo.RoleId).Select(x => x).SingleOrDefault();
                    if (result.Name == ConstMessage.REGISTER_USER_ROLE_NAME)
                    {
                        _variable.ReturnsBooleanResult = true;
                    }
                }
                catch(Exception)
                {
                
                }
            }

            //_variable.ReturnsBooleanResult =new AccountController().UserIsInRole(ConstMessage.REGISTER_USER_ROLE_NAME);


            //if user has a role Registered then display User DashBoard data
            //else dashBoard does not show
            if (_variable.ReturnsBooleanResult == true)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                var ctr = new TransactionController();
                var UserModel = ctr.GetUserDashboardData(UserName);

                //var temp = serializer.Serialize(RegisteredUserStatus(User.Identity.Name));

                //var obj = JObject.Parse(temp);

                //UserModel.MoneyTransferIn = (decimal?)obj["Data"]["MoneyTransferIn"];
                //UserModel.MoneyTransferOut = (decimal?)obj["Data"]["MoneyTransferOut"];
                //UserModel.TopUpPostPaid = (decimal?)obj["Data"]["TopUpPostPaid"];
                //UserModel.TopUpPrePaid = (decimal?)obj["Data"]["TopUpPrePaid"];
                return View(UserModel);
            }
     
             return View();
        }

        [NonAction]
        public IEnumerable<Link> GenerateMenuFromHome(string name)
        {
            if (name != null)
                return user_menu.GenarateMenu(name);
            return null;
        }


       //to do
        //[NonAction]
        //public ActionResult RegisteredUserStatus(string userName)
        //{
        //    IEnumerable<Transaction> allTransactions = new List<Transaction>();
        //    try
        //    {
        //        allTransactions = db.Transactions.Where(t => t.UserId == userName).OrderByDescending(t => t.TransactionDate).ToList();
        //    }
        //    catch (Exception ) { }
        //    var moneyTransferTemp = allTransactions.Where(t => t.ServiceId > 1 && t.ServiceId < 5).ToList();
        //    //decimal? mT = 0;
        //    foreach (var item in moneyTransferTemp)
        //    {
        //        if (item.AmountOut != null)
        //            App_Code.ConstMessage.MoneyTransferOut += item.AmountOut;
        //        else if (item.AmountIN != null)
        //            App_Code.ConstMessage.MoneyTransferIn += item.AmountIN;
        //    }
        //    IServiceRepository service_repo = new ServiceRepository();
        //    IEnumerable<Service> topups = service_repo.GetAllQueryable().Where(s => s.ParentId == ConstMessage.SERVICES_TOPUP_ID).ToList();
        //    IEnumerable<Transaction> TopUpTemp = (from t in allTransactions
        //                                       join top in topups on t.ServiceId equals top.Id
        //                                       select t);
                
        //        //allTransactions.Where(t => t.ServiceId == 6 || t.ServiceId == 7).ToList();

        //    foreach (var item in TopUpTemp)
        //    {
        //        if (item.ServiceId == 6 && item.AmountOut != null)
        //            App_Code.ConstMessage.TopUpPrePaid += item.AmountOut;
        //        else if (item.ServiceId == 7 && item.AmountOut != null)
        //            App_Code.ConstMessage.TopUpPostPaid += item.AmountOut;
        //    }
        //    var MoneyTransferIn = App_Code.ConstMessage.MoneyTransferIn;
        //    var MoneyTransferOut = App_Code.ConstMessage.MoneyTransferOut;
        //    var TopUpPrePaid = App_Code.ConstMessage.TopUpPrePaid;
        //    var TopUpPostPaid = App_Code.ConstMessage.TopUpPostPaid;
        //    JavaScriptSerializer serializer = new JavaScriptSerializer();
        //    var TransactionHistory = serializer.Serialize( allTransactions);

        //    return Json(new { MoneyTransferIn, MoneyTransferOut, TopUpPrePaid, TopUpPostPaid, TransactionHistory }, JsonRequestBehavior.AllowGet);
        //}

     

        
    }
}