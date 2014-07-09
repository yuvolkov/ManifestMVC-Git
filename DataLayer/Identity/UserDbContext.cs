using Microsoft.AspNet.Identity.EntityFramework;
using DataLayer.Identity;

namespace DataLayer.Identity
{
    public class UserDbContext : IdentityDbContext<ApplicationUser, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public UserDbContext()
            : base("ManifestDBContext")
        {
        }

        public static UserDbContext Create()
        {
            return new UserDbContext();
        }

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .ToTable("Users");

            modelBuilder.Entity<CustomUserRole>()
                .ToTable("UserRoles");

            modelBuilder.Entity<CustomUserLogin>()
                .ToTable("UserExternalLogins");

            modelBuilder.Entity<CustomUserClaim>()
                .ToTable("UserClaims");

            modelBuilder.Entity<CustomRole>()
                .ToTable("Roles");
        }
    }
}