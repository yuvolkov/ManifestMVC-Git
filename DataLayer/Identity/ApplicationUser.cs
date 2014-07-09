using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using System.Threading.Tasks;

using BrockAllen.IdentityReboot;
using BrockAllen.IdentityReboot.Ef;


namespace DataLayer.Identity
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    //public class ApplicationUser : IdentityUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    public class ApplicationUser : IdentityRebootUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public ApplicationUser() : 
            base()
        {
            // Defaults for a new user:
            this.LockoutEnabled = true;
            this.TwoFactorEnabled = true;
        }

        
        //public async Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(IdentityRebootUserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class CustomRole : IdentityRole<int, CustomUserRole>
    {
        public CustomRole() { }
        public CustomRole(string name) { Name = name; }
    }
 
    public class CustomUserRole  : IdentityUserRole <int> { }
    public class CustomUserClaim : IdentityUserClaim<int> { }
    public class CustomUserLogin : IdentityUserLogin<int> { }
}