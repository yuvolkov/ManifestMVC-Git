using System;
using System.Data.Entity;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;

using BrockAllen.IdentityReboot;
using BrockAllen.IdentityReboot.Ef;

using DataLayer.Identity;


namespace DataLayer.Identity
{
    //public class CustomUserStore : UserStore<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    public class CustomUserStore : IdentityRebootUserStore<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public CustomUserStore(UserDbContext context)
            : base(context)
        {
        }
    }

    //public class CustomRoleStore : RoleStore<CustomRole, int, CustomUserRole>
    //{
    //    public CustomRoleStore(UserDbContext context)
    //        : base(context)
    //    {
    //    }
    //}


    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    //public class ApplicationUserManager : UserManager<ApplicationUser, int>
    public class ApplicationUserManager : IdentityRebootUserManager<ApplicationUser, int>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser, int> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var dbContext = new UserDbContext();
            var manager = new ApplicationUserManager(new CustomUserStore(dbContext));

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser, int>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator() { RequiredLength = 6, RequireNonLetterOrDigit = false };

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug in here.
            //manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<ApplicationUser>
            //{
            //    MessageFormat = "Your security code is: {0}"
            //});
            //manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser>
            //{
            //    Subject = "SecurityCode",
            //    BodyFormat = "Your security code is {0}"
            //});
            manager.RegisterTwoFactorProvider("PhoneCode", new MobileStoredTwoFactorCodeProvider<ApplicationUser, int>() 
            { 
            });

            manager.EmailService = new EmailService();
            manager.SmsService = new SMSService();

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser, int>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
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

    public class SMSService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your sms service here to send a text message.
            return Task.FromResult(0);
        }
    }

}