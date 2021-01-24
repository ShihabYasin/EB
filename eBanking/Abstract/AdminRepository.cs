using eBanking.App_Code;
using eBanking.Interface;
using eBanking.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;


namespace eBanking.Abstract
{
    public class AdminRepository:IAdminRepository
    {
        private eBankingDbContext db;
        //private ITransactionRepository transaction_repo = new TransactionRepository(db);
        //private IPinRepository pin_repo = new PinRepository(db);
        private SMSDR_Helper smsHelper = new SMSDR_Helper();
        //private eBankingUser user;


        public AdminRepository()
        {
            this.db = new eBankingDbContext();
            //transaction_repo = new TransactionRepository(db);
            //pin_repo = new PinRepository(db);
        }
        public AdminRepository(eBankingDbContext context)
        {
            this.db = context;
            //transaction_repo = new TransactionRepository(db);
            //pin_repo = new PinRepository(db);
        }
        public IEnumerable<UserEditViewModel> SearchByUserAndRole(string Name, string UserRole, int? Country)
        {
            var UserList = db.Users.ToList();
            var RoleList = db.Roles.ToList();
            var userRole = db.userInRole.ToList();
            var CountryList = db.Destinations.ToList();
            IEnumerable<UserEditViewModel> List = null;
            if (!string.IsNullOrEmpty(Name) && Name != null)
            {
                UserList = UserList.Where(x => x.UserName.Contains(Name)).ToList(); 
            }
            if (Country>0)
            {
                UserList = UserList.Where(x => x.LocalCurrencyId.Equals(Country)).ToList();
            }
            if (!string.IsNullOrEmpty(UserRole) && UserRole != null)
            {
                RoleList = RoleList.Where(x => x.Name.Contains(UserRole)).ToList();
            }
            List = (from user in UserList
                    join userrole in userRole on user.Id equals userrole.UserId
                    join role in RoleList on userrole.RoleId equals role.Id
                    select new UserEditViewModel
                    {
                      //  UserId = user.Id,
                        UserName = user.UserName,
                        CreatedDate = user.CreatedDate,
                        //Address = null,
                        Email = user.Email,
                        IsActive = user.IsActive,
                        DistributorCode = user.DistributorCode,
                        RoleName = role.Name,
                        CurrentBalance = user.CurrentBalance,
                        CountryName = user.LocalCurrencyId > 0 ? (from dest in CountryList where dest.Id == user.LocalCurrencyId select dest.DestinationName).SingleOrDefault() : ""

                    });           

            return List;
        }

        public string GetUserCurrencyISOByUserName(string UserName)
        {
            CurrencyRepository currency_repo = new CurrencyRepository();
            try
            {
                var localCurrencyId = db.Users.Where(u => u.UserName == UserName).FirstOrDefault().LocalCurrencyId;
                if (localCurrencyId == 0)
                    return currency_repo.FindById(14).ISO;
                else
                    return currency_repo.FindById(localCurrencyId).ISO;
            }
            catch (Exception) { }
            return "";
        }

        public IQueryable<UserEditViewModel> GetAllQueryable()
        {
            try
            {
         //       return db.Pins;
            }
            catch (Exception) { }
            return null;
        }
        public IPagedList<UserEditViewModel> Search(string Name, string UserRole, int? ajaxLoad)
        {
            return null;
        }
        
        public IEnumerable<SMSDR> SmsdrGetAll()
        {
            try
            {
                return db.Smsdrs.ToList();
            }
            catch (Exception) { }
            return null;
        }
        public IQueryable<SMSDR> SmsdrGetAllQueryable()
        {
            try
            {
                return db.Smsdrs;
            }
            catch (Exception) { }
            return null;
        }
        public bool SmsdrAdd(SMSDR entity)
        {
            try
            {
                db.Smsdrs.Add(entity);
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        public bool SmsdrEdit(SMSDR entity)
        {
            try
            {
                db.Entry(entity).State = EntityState.Modified;
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }
        public IEnumerable<eBankingUser> GetAll_User()
        {
            return db.Users;
        }
        public eBankingUser GetUserByUserName(string UserName)
        {
            if (!string.IsNullOrEmpty(UserName))
                return db.Users.Where(u=>u.UserName == UserName).SingleOrDefault();
            else
                return null;
        }
        //hack - patch 2.1 - removed username from parameter
        public bool TopUpDbEntry(Transaction tModel, SMSDR smsdr, bool? FromSms)
        {
            int result = ConstMessage.STATUS_FAILED_ID;
            bool TopUpResult = false;
            ITransactionRepository transaction_repo = new TransactionRepository(db);

            using (var contextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (!FromSms ?? true)
                        tModel.UserBalance = transaction_repo.UserBalanceUpdate(tModel.UserId, -(decimal)tModel.AmountOut);
                    else
                        tModel.UserBalance = GetUserByUserName(tModel.UserId).CurrentBalance;
                    
                    transaction_repo.Add(tModel);
                    smsdr.TRANSACTION_ID = tModel.Id;
                    
                    //SmsdrEdit(smsdr);
                    SmsdrAdd(smsdr);

                    contextTransaction.Commit();

                    TopUpResult = true;
                }
                catch (Exception) 
                {
                    contextTransaction.Rollback();
                }
            }
            return TopUpResult;
        }
        //hack - patch 2.1 - added smsdr part and removed username from parameter
        public bool MoneyTransferDbEntry(Transaction model, SMSDR smsdr)
        {
            ITransactionRepository transaction_repo = new TransactionRepository(db);
            using (var contextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    model.UserBalance = transaction_repo.UserBalanceUpdate(model.UserId, -(decimal)model.AmountOut);
                    transaction_repo.Add(model);
                    //hack - patch 2.1 - added smsdr part
                    smsdr.TRANSACTION_ID = model.Id;
                    SmsdrAdd(smsdr);
                    //

                    contextTransaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    contextTransaction.Rollback();
                }   
            }
            return false;
        }

        public bool UpdatePendingRequest_ALL_Link(Transaction tModel, SMSDR smsdr)//, int? Status
        {
            IVendorRepository vendor_repo = new VendorRepository(db);
            ITransactionRepository transaction_repo = new TransactionRepository(db);
            IPinRepository pin_repo = new PinRepository(db);
            using (var contextTransaction = db.Database.BeginTransaction())
            {
                bool balanceUpdate = true;
                try
                {
                    transaction_repo.Edit(tModel);
                    SmsdrEdit(smsdr);
                    if (smsdr.STATUS == ConstMessage.STATUS_FAILED_ID)
                    {
                        //decimal balanceUpdate = UserBalanceUpdate(tModel.UserId, tModel.AmountOut.GetValueOrDefault());
                        if(tModel.PinId.HasValue)
                        {
                            pin_repo.PinReactivate((int)tModel.PinId);
                            balanceUpdate = true;
                        }
                        else
                        {
                            balanceUpdate = transaction_repo.Return_User_Balance(tModel);
                        }
                        //vendor_repo.VendorTransaction_Add(tModel, ConstMessage.Balance_Add);
                        vendor_repo.VendorBalanceReturn(tModel.Id);
                    }
                    

                    if(balanceUpdate)
                    {
                        contextTransaction.Commit();
                        return true;
                    }
                    else
                    {
                        contextTransaction.Rollback();
                        return false;
                    }

                }
                catch (Exception)
                {
                    contextTransaction.Rollback();
                }
            }
            return false;
        }
        public bool CreditTransferDbEntry(Transaction tIn, Transaction tOut, string fromUser, string toUser)
        {
            ITransactionRepository transaction_repo = new TransactionRepository(db);
            using(var contextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    tOut.UserBalance = transaction_repo.UserBalanceUpdate(fromUser, -(decimal)tOut.AmountOut);
                    transaction_repo.Add(tOut);
                    tIn.UserBalance = transaction_repo.UserBalanceUpdate(toUser, (decimal)tOut.AmountOut);
                    transaction_repo.Add(tIn);
                    //UserUpdate(fromUser);
                    //UserUpdate(toUser);

                    contextTransaction.Commit();
                    return true;
                }
                catch (Exception) 
                {
                    contextTransaction.Rollback();
                }
            }
            
            return false;
        }
        public bool VoucherDbEntry(Transaction tModel, Pin pin, string UserName, decimal CurrentBalance)
        {
            bool result = false;
            using (var contextTransaction = db.Database.BeginTransaction())
            {
                ITransactionRepository transaction_repo = new TransactionRepository(db);
                IPinRepository pin_repo = new PinRepository();
                try
                {
                    tModel.UserBalance = transaction_repo.UserBalanceUpdate(UserName, CurrentBalance); 
                    transaction_repo.Add(tModel);
                    pin_repo.Edit(pin);

                    contextTransaction.Commit();
                    result = true;
                }
                catch (Exception)
                {
                    contextTransaction.Rollback();
                }
            }
            return result;
        }
        public bool ClientIsActive(string Version)
        {
            try
            {
                return db.Clients.Where(c=>c.Version == Version).FirstOrDefault().IsActive;
            }
            catch(Exception)
            { }
            return false;
        }
        public Client GetClientByApkVersion(string Version)
        {
            try
            {
                return db.Clients.Where(c => c.Version == Version).FirstOrDefault();
            }
            catch (Exception) { }
            return null;
        }
        public bool UserUpdate(eBankingUser user)
        {
            try
            {
                db.Entry(user).State = EntityState.Modified;
                Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        

        public void Save()
        {
            db.SaveChanges();
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