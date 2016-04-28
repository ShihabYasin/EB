using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using eBanking.Models;
using eBanking.Interface;
using eBanking.Abstract;

namespace eBanking.Abstract
{
    public class UserRoleRepository:IUserRoleRepository
    {
        private eBankingDbContext db;
        private aspnetuserroles user_role;
        private eBankingUser User;

        public UserRoleRepository()
        {
            this.db = new eBankingDbContext();
        }
        public UserRoleRepository(eBankingDbContext context)
        {
            this.db = context;
           
        }
        public IEnumerable<aspnetuserroles> GetAll()
        {
            try 
            {
                return db.userInRole.ToList();
            }
            catch(Exception)
            {
            
            }
            return null;
        }

        public aspnetuserroles FindByUserId(string UserId)
        {
            try
            {
                //user_role = new aspnetuserroles();
                user_role = (from e in GetAll()
                             where e.UserId == UserId
                             select new aspnetuserroles
                             {
                             UserId=e.UserId,
                             RoleId=e.RoleId
                 
                             }).SingleOrDefault();
            }
            catch (Exception)
            {
                user_role = null;
            }

            return user_role;
        }

        public  aspnetuserroles FindByRoleId(string RoleId)
        {
            try
            {
                user_role = (from e in GetAll()
                             where e.RoleId == RoleId
                             select new aspnetuserroles
                             {
                                 UserId = e.UserId,
                                 RoleId = e.RoleId

                             }).SingleOrDefault();

                return user_role;
            }
            catch (Exception)
            {

            }
            return null;
        }

       public IEnumerable<eBankingUser> GetAllUser()
        {
            try
            {
                return db.Users.ToList();
            }
            catch (Exception)
            {

            }
            return null;
        }

       public IQueryable<eBankingUser> GetAllUserQueryable()
       {
           try
           {
               return db.Users;
           }
           catch (Exception) { }

           return null;
       }
       public eBankingUser FindUserByUserName(string UserName)
        {
            try
            {
                User = (from u in GetAllUser()
                        where u.UserName == UserName
                        select new eBankingUser
                        {
                            Id = u.Id,
                            UserName = u.UserName,
                            CurrentBalance = u.CurrentBalance, //,PasswordHash = u.PasswordHash
                            DistributorCode = u.DistributorCode
                        }).SingleOrDefault();

                return User;
            }

           catch(Exception)
            {
           
            }
            return null;
        
        }

       public IEnumerable<eBankingUser> GetAllUserOfaRole(string RoleName)
       {
           //var context = new ApplicationDbContext();
           //var users = context.Users.Where(x => x.Roles.Select(y => y.RoleId).Contains(RoleName)).ToList();
           

           IEnumerable<eBankingUser> eBankUser = null;

           try
           {
               eBankUser = (from role in db.Roles.ToList()
                            where role.Name == RoleName
                            join user_role in db.userInRole.ToList() on role.Id equals user_role.RoleId
                            join user in db.Users.ToList() on user_role.UserId equals user.Id
                            select user).ToList();

           }
           catch (Exception mx) {
               string a = mx.Message;
           }

           return eBankUser;
           
       }

       public string GetRoleByUserName(string UserName)
       {
           string roleName = "";
           try
           {
               roleName = (from users in GetAllUser()
                           join roles in db.userInRole on users.Id equals roles.UserId
                           join rNames in db.Roles on roles.RoleId equals rNames.Id
                               where users.UserName == UserName
                               select rNames.Name).SingleOrDefault();
           }
           catch (Exception) { }
           return roleName;
       }


       private bool disposed = false;
       protected virtual void Dispose(bool disposing)
       {
           if (!this.disposed)
           {
               if (disposing)
               {
                   db.Dispose();
               }
           }
           this.disposed = true;
       }

       public void Dispose()
       {
           Dispose(true);
           GC.SuppressFinalize(this);
       }
        
    }
}