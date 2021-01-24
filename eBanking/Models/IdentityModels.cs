using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Security.Claims;
using System.Threading.Tasks;

using System;

namespace eBanking.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class eBankingUser : IdentityUser
    {
        public object RoleName;
        public async Task<ClaimsIdentity>
           GenerateUserIdentityAsync(UserManager<eBankingUser> manager)
        {
            var userIdentity = await manager
                .CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
        

        public string Name { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string DateofBirth { get; set; }
       // public string Email { get; set; }
        public string PresentAddress { get; set; }
        public string ParmanentAddress { get; set; }
        public string OtherConductInfo { get; set; }     
 
        [Display(Name="Active")]
        public bool IsActive { get; set; }
        public decimal CurrentBalance { get; set; }
        public int LocalCurrencyId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string DistributorCode { get; set; }
        public string NomineePhoneNumber { get; set; }
    }

    public class eBankingRole : IdentityRole
    {
        public string Description { get; set; }

        public eBankingRole() : base() { }
        public eBankingRole(string name)
            : this()
        {
            this.Name = name;
        }

        public eBankingRole(string name, string description)
            : this(name)
        {
            this.Description = description;
        }

    }


    public class aspnetuserroles
    {       

        [Key]
        [Column(Order = 0)]
        public string UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        public string RoleId { get; set; }
    }
    public class eBankingDbContext : IdentityDbContext<eBankingUser>
    {
        public eBankingDbContext()
            : base("eBankingConnection")
        {
        }
        public IDbSet<AssignDistributorRateplan> AssignDistributorRateplans { get; set; }
        public IDbSet<Audit> Audits { get; set; }
        public IDbSet<SMSDR> Smsdrs { get; set; }
        public IDbSet<Sequencer> Sequencers { get; set; }
        public IDbSet<StatusMsg> StatusMsgs { get; set; }
        public IDbSet<Transaction> Transactions { get; set; }
        public IDbSet<RatePlan> RatePlans { get; set; }
        public IDbSet<Service> Services { get; set; }
        public IDbSet<Pin> Pins { get; set; }
     
        //In Db table Name should be currencies not Currencys
        public IDbSet<Currency> Currencies { get; set; }
        public IDbSet<Destination> Destinations { get; set; }
        public IDbSet<RoleDetail> RoleDetails { get; set; }
        public IDbSet<Link> Links { get; set; }
        public IDbSet<aspnetuserroles> userInRole { get; set; }
        public IDbSet<Client> Clients { get; set; }
        public IDbSet<ChangeUserPasswordVM> ChangeUserPasswordVMs { get; set; }
        public IDbSet<Vendor> Vendors { get; set; }
        public IDbSet<VendorTarrif> VendorTarrifs { get; set; }
        public DbSet<VendorTransaction> VendorTransactions { get; set; }
        public IDbSet<VendorRequestLog> VendorRequestLogs { get; set; }
        public IDbSet<VendorRequestLogDetail> VendorRequestLogDetails { get; set; }
        public IDbSet<VendorRoute> VendorRoutes { get; set; }
        public IDbSet<DistributorCommissionRateplan> DistributorCommissionRateplans { get; set; }
        public IDbSet<Distributor> Distributors { get; set; }
        public IDbSet<DistributorTransaction> DistributorTransactions { get; set; }
        public IDbSet<PinHistory> PinHistories { get; set; }
        public IDbSet<DistributorTransactionDetail> DistributorTransactionDetails { get; set; }
        public static eBankingDbContext Create()
        {
            return new eBankingDbContext();
        }

        public System.Data.Entity.DbSet<eBanking.Models.SMSTransactionReport> SMSTransactionReports { get; set; }

        //public System.Data.Entity.DbSet<eBanking.Models.AssignDistributorRateplan> AssignDistributorRateplans { get; set; }


       // public System.Data.Entity.DbSet<eBanking.Models.VendorTarrif> VendorTarrifs { get; set; }

        //public System.Linq.IQueryable<UserEditViewModel> UserEditViewModels { get; set; }

        //public System.Data.Entity.DbSet<eBanking.Models.Vendor> Vendors { get; set; }
    }
}