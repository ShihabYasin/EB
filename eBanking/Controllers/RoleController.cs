using eBanking.App_Code;
using eBanking.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Mvc;
//using System.Web.Http;

namespace eBanking.Controllers
{

    [CustomAuth]
    public class RoleController : Controller
    {

        #region Repository,Context And Variable Declaration

        private List<RoleDetail> RoleDetails;

        private UserMenuGenarator adminUserRole;

        private eBankingDbContext db;
        private Variable _variable;
        public RoleController()
        {
            db = new eBankingDbContext();
            UserManager = new UserManager<eBankingUser>(new UserStore<eBankingUser>(db));
            RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            _variable = new Variable();
            RoleDetails = new List<RoleDetail>();
            adminUserRole = new UserMenuGenarator();
        }
     
        private UserManager<eBankingUser> UserManager { get; set; }

        private RoleManager<IdentityRole> RoleManager { get; set; }

        #endregion



        /*---------------------------------------------------------
        *  HandleUnknownAction method is fired when an an illigal action or any requested action from uri that is not
        *  currently  present in any controller hear RoleController
        *---------------------------------------------------------*/

        protected override void HandleUnknownAction(string actionName)
        {
            ViewData["name"] = actionName;
            View("Error").ExecuteResult(this.ControllerContext);
        }


        /*--------------------------------------------------------------
         * if db context or UserManager and RoleManager is open then Dispose the context hear
         * 
         * -------------------------------------------------------------*/
        protected override void Dispose(bool disposing)
        {
            if (disposing && (UserManager != null || RoleManager != null))
            {
                if (UserManager != null)
                {
                    UserManager.Dispose();
                    UserManager = null;
                }
                if (RoleManager != null)
                {
                    RoleManager.Dispose();
                    RoleManager = null;
                }

            }

            base.Dispose(disposing);
        }




        /*--------------------------------------------------------------
         * this is Index page to display All Role Name
         * 
         * -------------------------------------------------------------*/
        public ActionResult Index()
        {           
            try
            {
                IEnumerable<IdentityRole> rolelist = db.Roles.ToList();               
               
                return View(rolelist);
            }
            catch(Exception ex)
            {
                string ms = ex.Message.ToString();
               
            }
            return View("Error");
        }




        /*--------------------------------------------------------------
         * CreateRole display All Controller  and Action name with Active option 
         * using reflection
         * -------------------------------------------------------------*/
        [HttpGet]
        public ActionResult CreateRole()
        {           
            try
            {
                
                //using reflector get all RoleDetails controller Action list
                RoleDetails = GetControllerNames();               
                    
                return View(RoleDetails);
            
            }
            catch (Exception)
            {
              
            }

            return View("Error");
        }



        /*--------------------------------------------------------------
         * if db context or UserManager and RoleManager is open then Dispose the context hear
         * 
         * -------------------------------------------------------------*/

        [HttpPost]
        public ActionResult CreateRole(List<RoleDetail> model, string RoleName)  // string RoleDescription
        {
            _variable.Flag = -1;

            if (ModelState.IsValid && !string.IsNullOrEmpty(RoleName) )   //&& !string.IsNullOrEmpty(RoleDescription)
            {
                //check is the role already exists 
                var IsExists = (from e in db.Roles where e.Name == RoleName select e.Id).FirstOrDefault();
                
                // return Json(new { IsExists }, JsonRequestBehavior.AllowGet);
                //if not exists create a new role

                if (IsExists == null)
                {

                    var role = new IdentityRole() { Name = RoleName };
                   // role.Description = RoleDescription;

                    IdentityResult result =  RoleManager.Create(role);

                    if (result.Succeeded)
                    {
                        _variable.RoleId = role.Id;                       
                    }
                    else
                    {
                        _variable.Flag = ConstMessage.CretionFailed;
                        AddErrors(result);
                    }

                }
                else
                {
                    //Role Already exists genarate Error Message
                    _variable.Flag = ConstMessage.Dupliacation;
                    ModelState.AddModelError("", RoleName+" Role Already Exists !!");              
                                 
                }

                //if id is not 0 zero
                if (_variable.RoleId != null && !string.IsNullOrEmpty(_variable.RoleId))
                {

                    RoleDetail roleDetails = new RoleDetail();
                    foreach (RoleDetail Rdetails in model)
                    {
                        //if update then find row by Id then assign updated data
                        //for update the field
                        if (Rdetails.Id != 0)
                        {
                            roleDetails = db.RoleDetails.Find(Rdetails.Id);

                            //if change occure then update field
                            if (roleDetails.IsAccessible != Rdetails.IsAccessible)
                            {
                                roleDetails.RoleId = Rdetails.RoleId;
                                roleDetails.ControllerName = Rdetails.ControllerName;
                                roleDetails.ActionName = Rdetails.ActionName;
                                roleDetails.IsAccessible = Rdetails.IsAccessible;
                                db.SaveChanges();
                            }

                        }
                        else
                        {
                            //add new field
                            roleDetails.ControllerName = Rdetails.ControllerName;
                            roleDetails.ActionName = Rdetails.ActionName;

                            roleDetails.RoleId = _variable.RoleId;  //IsExists.Id;

                            roleDetails.IsAccessible = Rdetails.IsAccessible;
                            db.RoleDetails.Add(roleDetails);
                            db.SaveChanges();
                        }

                    }

                    //db.SaveChanges();
                    //return to Index

                    return RedirectToAction("Index");
                }

            }
            else
            {
                _variable.Flag = ConstMessage.NullReference;

                if (string.IsNullOrEmpty(RoleName))   //&& !string.IsNullOrEmpty(RoleDescription)
                    ModelState.AddModelError("", "Role Name is Requiread.");

                //else if (string.IsNullOrEmpty(RoleDescription) && !string.IsNullOrEmpty(RoleName))
                //    ModelState.AddModelError("", "Role Description is Requiread.");

                //else if (string.IsNullOrEmpty(RoleName) && string.IsNullOrEmpty(RoleDescription))
                //    ModelState.AddModelError("", "Role Name and Description is Requiread.");
                else
                    ModelState.AddModelError("", "Invalid Entry!!");

            }

            //genarate layout with view
            //if (_variable.Flag > 0)
            //{
            //    //_variable.UserName = HttpContext.User.Identity.Name;

            //    //ViewBag.Menu = null;

            //    //if (_variable.UserName != null && !string.IsNullOrEmpty(_variable.UserName))
            //    //{
            //    //    ViewBag.Menu = adminUserRole.GenarateMenu(_variable.UserName);

            //    //}         
            
            //}
             return View(model);
        }


        /*--------------------------------------------------------------
         * this is Edit Role HttpGet display all Controller and Action and Active option
         * from merging Controller,Action get from reflection and get from DB
         * -------------------------------------------------------------*/
        [HttpGet]
        public ActionResult Edit(string id,string RoleName)
        {
           
            try
            {

                //get all RoleDetails list  from Db
                var RDListFromDB = db.RoleDetails.Where(x=>x.RoleId==id).Select(t => t).ToList();

                if (RDListFromDB != null)
                {
                    ViewBag.RoleName = RoleName;
                    //using reflector get all RoleDetails controller Action list dynamically
                    RoleDetails = GetControllerNames();

                    //now mearge the above two list in the stretagy ->All/Commom row Field will select From RDListFromDB,New filed come from RoleDetails
                    //mearge when Both List has Data

                    if (RDListFromDB.Count > 0 && RoleDetails.Count > 0)
                    {
                        RoleDetails = MeargeTwoList(RDListFromDB, RoleDetails);
                    }
                    //data exists in DB but Reflector not then assign
                    else if (RDListFromDB.Count > 0 && RoleDetails.Count == 0)
                    {
                        RoleDetails = RDListFromDB;
                    }

                    //if (ajaxLoad == 1)
                    //    return PartialView(RoleDetails);
                    //else
                    //{
                    //    //genarate Menu
                    //    _variable.UserName = HttpContext.User.Identity.Name;

                    //    ViewBag.Menu = null;

                    //    if (_variable.UserName != null && !string.IsNullOrEmpty(_variable.UserName))
                    //    {
                    //        ViewBag.Menu = adminUserRole.GenarateMenu(_variable.UserName);
                    //    }    
                    //}

                    return View(RoleDetails);
                }
              

                return View("Error");

               
            }
            catch (Exception)
            {
                return View("Error");
            }

           
        }
        

        /*--------------------------------------------------------------
         * EditRole is responsible to rename a Role and Manage(Active/DeActive) a Controller Action
         * for this Role Name
         * 
         * -------------------------------------------------------------*/
        [HttpPost]
        [ActionName("Edit")]
        public ActionResult EditRole(List<RoleDetail> model, string RoleName, string ChangeRoleName)
        {
            _variable.Flag = 0;

            if (ModelState.IsValid && !string.IsNullOrEmpty(RoleName))
            {
                //check is the role already exists 
                var IsExists = RoleManager.FindByName(RoleName);

                RoleDetail roleDetails = new RoleDetail();
                //if id is not 0 zero
                if (!string.IsNullOrEmpty(IsExists.Id) && IsExists.Id!=null)
                {
                    //check if ChangeRoleName then rename Role 
                    if (!string.IsNullOrEmpty(ChangeRoleName) && ChangeRoleName != null)
                    {
                        IsExists.Name = ChangeRoleName;
                       
                        IdentityResult result = RoleManager.Update(IsExists);
                       
                        if (result.Succeeded) {
                            _variable.Flag = 1;
                        }
                        else
                        {
                            AddErrors(result);
                                               
                        }
                    }
                    else
                    {
                        //if role does not change
                        _variable.Flag = 2;
                       
                    }

                    if (_variable.Flag == 1 || _variable.Flag == 2)
                    {
                        foreach (RoleDetail Rdetails in model)
                        {
                            //if update then find row by Id then assign updated data
                            //for update the field
                            if (Rdetails.Id != 0)
                            {
                                roleDetails = db.RoleDetails.Find(Rdetails.Id);

                                //if change occure then update field
                                if (roleDetails.IsAccessible != Rdetails.IsAccessible)
                                {
                                    roleDetails.RoleId = Rdetails.RoleId;
                                    roleDetails.ControllerName = Rdetails.ControllerName;
                                    roleDetails.ActionName = Rdetails.ActionName;
                                    roleDetails.IsAccessible = Rdetails.IsAccessible;

                                    try
                                    {
                                        //edit
                                        db.Entry(roleDetails).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }

                            }
                            else
                            {
                                //new field so create a field 
                                //if is accessible true
                                if (Rdetails.IsAccessible == true)
                                {
                                    //add new field
                                    roleDetails.ControllerName = Rdetails.ControllerName;
                                    roleDetails.ActionName = Rdetails.ActionName;

                                    roleDetails.RoleId = IsExists.Id;

                                    roleDetails.IsAccessible = Rdetails.IsAccessible;
                                    db.RoleDetails.Add(roleDetails);

                                    try
                                    {
                                        db.SaveChanges();
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                            }

                        }
                    }

                    return RedirectToAction("Index");
                        
                                      
                }

            }
            else
            {
                if (string.IsNullOrEmpty(RoleName))
                    ModelState.AddModelError("", "Role Name is Requiread.");               
                else
                    ModelState.AddModelError("", "Invalid Entry!!");

            }

            //genarate Menu
            //_variable.UserName = HttpContext.User.Identity.Name;

            //ViewBag.Menu = null;

            //if (_variable.UserName != null && !string.IsNullOrEmpty(_variable.UserName))
            //{
            //    ViewBag.Menu = adminUserRole.GenarateMenu(_variable.UserName);
            //}    

            return View(model);
          
        
        }

        #region helper

        [NonAction]
        public void RoleModefication()
        { 
        
        }


        /*--------------------------------------------------------------
         * when any exception occured it populate the error message
         * 
         * -------------------------------------------------------------*/
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        #region meargeRoleDetailList_FromDB_WithReflectorList

         /*--------------------------------------------------------------
         * To display MVC Controller Name and Action name from Controllers folder of this project 
         * and Controller & Action Name that is priviously saved in DB
         * as Union Process we mearge this two list 
         * -------------------------------------------------------------*/
        [NonAction]
        public List<RoleDetail> MeargeTwoList(List<RoleDetail> ListFromDB, List<RoleDetail> ListFromReflector)
        {
            // List<RoleDetail> newRoleDetails = new List<RoleDetail>();

            foreach (RoleDetail DBlist in ListFromDB)
            {
                var search = (from e in ListFromReflector where e.ControllerName == DBlist.ControllerName && e.ActionName == DBlist.ActionName select e).ToList();

                //remove list from reflectorList
                if (search.Count > 0)
                {
                    //for same action name one or more list as 

                    for (int i = 0; i < search.Count; i++)
                    {
                        ListFromReflector.Remove(search[i]);
                    }
                }
            }

            //now add all remaining Reflector list in ListFromDB
            foreach (RoleDetail ExistingList in ListFromReflector)
            {
                ListFromDB.Add(new RoleDetail { ControllerName = ExistingList.ControllerName, ActionName = ExistingList.ActionName });

            }

            return ListFromDB;

        }
            

        #endregion


        #region GetControllerActionList_Using_Reflection
        public List<RoleDetail> GetControllerNames()   
        {
            

            IEnumerable<MethodInfo> list;

            List<string> ActionList = new List<string>();
            List<string> controllerNames = new List<string>();

            
           
                //Get All Api Controller
            GetSubClasses<System.Web.Http.ApiController>().ForEach
                    (type => controllerNames.Add(type.Name));
           
            //Get all MVC Controller  system.Net.Http
            GetSubClasses<Controller>().ForEach(
                type => controllerNames.Add(type.Name));

        
            foreach (var ConName in controllerNames)
            {


                ////ActionList = ActionNames(ConName);
                var thisType = GetType();
                Type t = thisType.Assembly.GetType(thisType.Namespace + "." + ConName);
                // var Name = t.Name;

                //  MemberInfo[] members = t.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                list = (from action in t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        where action.ReturnType == typeof(ActionResult) || action.ReturnType == typeof(HttpResponseMessage)
                        select action);

                var filter = list.Select(x=>x).OrderBy(x=>x.Name).GroupBy(x=>x.Name);
                try
                {
                    foreach (var info in filter)  //MethodInfo info in list
                    {
                        //new RoleDetail { ControllerName=t.Name, ActionName=info.Name };
                        RoleDetails.Add(new RoleDetail { ControllerName = t.Name, ActionName = info.Key.ToString() });

                    }
                }
                catch (Exception ex)
                {
                    string a = ex.Message;
                }

            }

            // IEnumerable<RoleDetail> filter = RoleDetails.Select(x=>x).GroupBy(t=>t.ActionName);

            //return filter;
            return RoleDetails;
        }

        private static List<Type> GetSubClasses<T>()
        {
            //Controller Name order by alphabetically
            return Assembly.GetCallingAssembly().GetTypes().Where(
                type => type.IsSubclassOf(typeof(T))).OrderBy(x=>x.Name).ToList();
        }

        #endregion
        #endregion
	}
}