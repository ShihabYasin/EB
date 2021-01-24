using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eBanking.Models;
using eBanking.Interface;
using eBanking.Abstract;

namespace eBanking.App_Code
{
    public class UserMenuGenarator
    {
         private List<Link> list;
        private eBankingDbContext db = new eBankingDbContext();

        private IUserRoleRepository user_role_repo;
      private IEnumerable<Link> GetMenu(string UserID, string RoleID)
        {
            try
            {

                var query = (from role in db.RoleDetails.Where(x => x.RoleId == RoleID && x.IsAccessible == true).ToList()
                             from link in db.Links.Where(x => x.IsActive == true).ToList()
                             where role.ControllerName == link.ControllerName && role.ActionName == link.ActionName
                             select link).OrderBy(x=>x.SequenceId).ToList();
                             
             
                             //.OrderBy(x=>x.SequenceId).Select(x=>new Link{

                             //    Id = x.Id,
                             //    Name = x.Name,
                             //    ControllerName = x.ControllerName,
                             //    ActionName = x.ActionName,
                             //    GroupName = x.GroupName,
                             //    SequenceId = x.SequenceId
                             
                             //}).ToList(); 

                                      
                return query;
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }
            return null;

        }


        public aspnetuserroles GetUserRole(string UserName)
        {
            try
            {
                
                //var list = db.userInRole.ToList();

                user_role_repo = new UserRoleRepository(new eBankingDbContext());

                var query = user_role_repo.FindUserByUserName(UserName);
                    //db.Users.Where(x => x.UserName == UserName).Select(x => x).ToList();        

                if (query != null)
                {
                    var isInRole = user_role_repo.FindByUserId(query.Id);
                    //db.userInRole.Where(x => x.UserId == query[0].Id).Select(x => x).ToList();


                    if (isInRole != null)
                    {
                        return isInRole;
                    }
                   
                }
            }
            catch (Exception ex)
            {
                string m = ex.Message;
               
            }

            return null;
        }

        public IEnumerable<Link>  GenarateMenu(string UserName)
        {


            IEnumerable<Link> AccessibleMenu;

            if (UserName != null && !string.IsNullOrEmpty(UserName))
            {
                var hasAnyRole = GetUserRole(UserName);

                if (hasAnyRole != null)
                {
                    AccessibleMenu = GetMenu(hasAnyRole.UserId, hasAnyRole.RoleId);
                    return AccessibleMenu;
                }

            }
            return null;
        }
    }
}
