using eBanking.Models;
using System.Collections.Generic;
using System.Linq;

namespace eBanking.Interface
{
   public interface IUserRoleRepository
    {
        IEnumerable<aspnetuserroles> GetAll();

        IEnumerable<eBankingUser> GetAllUser();
        IQueryable<eBankingUser> GetAllUserQueryable();

        aspnetuserroles FindByUserId(string UserId);

        aspnetuserroles FindByRoleId(string RoleId);

        eBankingUser FindUserByUserName(string UserName);

        IEnumerable<eBankingUser> GetAllUserOfaRole(string RoleName);
        string GetRoleByUserName(string UserName);

      
    }
}
