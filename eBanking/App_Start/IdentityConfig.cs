using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web;

using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

using eBanking.Models;
using eBanking.App_Code;

namespace eBanking.App_Start
{
    public class IdentityConfig
    {

        public IdentityConfig()
            : this(new UserManager<eBankingUser>(new UserStore<eBankingUser>(new eBankingDbContext())),new RoleManager<eBankingRole>(new RoleStore<eBankingRole>(new eBankingDbContext())))
        {
        }

        public IdentityConfig(UserManager<eBankingUser> userManager, RoleManager<eBankingRole> roleManager)
        {
            UserManager = userManager;
            RoleManager=roleManager;
        }

        public UserManager<eBankingUser> UserManager { get; set; }

        public RoleManager<eBankingRole> RoleManager { get; set; }


        //  public IdentityConfig()
        //    : this(new UserManager<eBankingUser>(new UserStore<eBankingUser>(new eBankingDbContext())))
        //{
        //}

        //public IdentityConfig(UserManager<eBankingUser> userManager)
        //{
        //    UserManager = userManager;
        //    RoleManager=roleManager;
        //}

        //private RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new eBankingDbContext()));

        //public UserManager<eBankingUser> UserManager { get;  set; }

        //public RoleManager<IdentityRole> RoleManager { get; set; }

        
        

    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.

    public class eBankingUserManager : UserManager<eBankingUser>
    {
        public eBankingUserManager(IUserStore<eBankingUser> store)
            : base(store)
        {
        }

        public static eBankingUserManager Create(IdentityFactoryOptions<eBankingUserManager> options,
            IOwinContext context)
        {
            var manager = new eBankingUserManager(new UserStore<eBankingUser>(context.Get<eBankingDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<eBankingUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                //RequireNonLetterOrDigit = true,
                //RequireDigit = true,
                //RequireLowercase = true,
                //RequireUppercase = true,
            };

            // Configure user lockout defaults 3 times can access for login if failed then 30 minutes lock
            //after 30 minutes user can try again for login before 3 line of code

            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(ConstMessage.DEFAULT_ACCOUNT_LOCKOUT_TIME_SPAN_MINUTES);
            manager.MaxFailedAccessAttemptsBeforeLockout =ConstMessage.MAX_FAILED_ACCESS_ATTEMPTS_BEFORE_LOCKOUT;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug in here.
            manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<eBankingUser>
            {
                MessageFormat = "Your security code is: {0}"
            });
            manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<eBankingUser>
            {
                Subject = "SecurityCode",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<eBankingUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the RoleManager used in the application. RoleManager is defined in the ASP.NET Identity core assembly
    public class eBankingRoleManager : RoleManager<eBankingRole>
    {
        public eBankingRoleManager(IRoleStore<eBankingRole, string> roleStore)
            : base(roleStore)
        {
        }

        public static eBankingRoleManager Create(IdentityFactoryOptions<eBankingRoleManager> options, IOwinContext context)
        {
            return new eBankingRoleManager(new RoleStore<eBankingRole>(context.Get<eBankingDbContext>()));
        }
    }

    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your sms service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // This is useful if you do not want to tear down the database each time you run the application.
    // public class ApplicationDbInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    // This example shows you how to create a new database if the Model changes
    //public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    //{
    //    protected override void Seed(ApplicationDbContext context)
    //    {
    //        InitializeIdentityForEF(context);
    //        base.Seed(context);
    //    }

    //    //Create User=Admin@Admin.com with password=Admin@123456 in the Admin role        
    //    public static void InitializeIdentityForEF(ApplicationDbContext db)
    //    {
    //        var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
    //        var roleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
    //        const string name = "admin@example.com";
    //        const string password = "Admin@123456";
    //        const string roleName = "Admin";

    //        //Create Role Admin if it does not exist
    //        var role = roleManager.FindByName(roleName);
    //        if (role == null)
    //        {
    //            role = new ApplicationRole(roleName);
    //            var roleresult = roleManager.Create(role);
    //        }

    //        var user = userManager.FindByName(name);
    //        if (user == null)
    //        {
    //            user = new ApplicationUser { UserName = name, Email = name };
    //            var result = userManager.Create(user, password);
    //            result = userManager.SetLockoutEnabled(user.Id, false);
    //        }

    //        // Add user admin to Role Admin if not already added
    //        var rolesForUser = userManager.GetRoles(user.Id);
    //        if (!rolesForUser.Contains(role.Name))
    //        {
    //            var result = userManager.AddToRole(user.Id, role.Name);
    //        }
    //    }
    //}

    public class eBankingSignInManager : SignInManager<eBankingUser, string>
    {
        public eBankingSignInManager(eBankingUserManager userManager, IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager) { }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(eBankingUser user)
        {
            return user.GenerateUserIdentityAsync((eBankingUserManager)UserManager);
        }

        public static eBankingSignInManager Create(IdentityFactoryOptions<eBankingSignInManager> options, IOwinContext context)
        {
            return new eBankingSignInManager(context.GetUserManager<eBankingUserManager>(), context.Authentication);
        }
    }

}