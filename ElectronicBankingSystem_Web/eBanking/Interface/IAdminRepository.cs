using eBanking.Models;
using System.Collections.Generic;
using System.Linq;

namespace eBanking.Interface
{
    public interface IAdminRepository
    {
        IEnumerable<eBankingUser> GetAll_User();
        IEnumerable<UserEditViewModel> SearchByUserAndRole(string Name, string UserRole, int? Country);
        IEnumerable<SMSDR> SmsdrGetAll();
        IQueryable<SMSDR> SmsdrGetAllQueryable();
        bool SmsdrAdd(SMSDR entity);
        bool SmsdrEdit(SMSDR entity);
        bool TopUpDbEntry(Transaction tModel, SMSDR smsdr, bool? FromSms);
        bool VoucherDbEntry(Transaction tModel, Pin pin, string UserName, decimal CurrentBalance);
        eBankingUser GetUserByUserName(string UserName);
        bool UserUpdate(eBankingUser user);

        bool UpdatePendingRequest_ALL_Link(Transaction tModel, SMSDR smsdr);//, int? Status
        bool MoneyTransferDbEntry(Transaction model, SMSDR smsdr);
        bool CreditTransferDbEntry(Transaction tIn, Transaction tOut, string fromUser, string toUser);
        bool ClientIsActive(string Version);
        Client GetClientByApkVersion(string Version);
        string GetUserCurrencyISOByUserName(string UserName);
        void Save();
    }
}
