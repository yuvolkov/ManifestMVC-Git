using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using Microsoft.AspNet.Identity;

using Omu.ValueInjecter;

using DataLayer.DataModels;
using DataLayer.ViewModels;
using DataLayer.Identity;
using System.Security.Claims;

namespace DataLayer.Repositories
{
    public class UsersRepo :
        UOWRepo<UsersRepo>, IUserStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>,
        IUserClaimStore<ApplicationUser> //, IUserRoleStore<TUser>
        //, IUserSecurityStampStore<ApplicationUser>
    {
        UserDM _newUser;


        public UsersRepo()
            : base()
        {
        }

        internal UsersRepo(ManifestDBContext context)
            : base(context)
        {
        }


        public Task CreateAsync(ApplicationUser user)
        {
            var userDM = _newUser;

            userDM.IsAdmin = false;
            userDM.IsLoginEnabled = true;
            userDM.DateRegistered = DateTime.Now;
            userDM.Name = user.UserName;
            userDM.Email = user.Email;
            //userDM.PasswordHash = user.PasswordHash;

            _context.Users.Add(userDM);

            //_context.SaveChangesAsync();
            _context.SaveChanges();

            user.IdAsInt = userDM.ID;

            return Task.FromResult<int>(0);
        }

        public Task DeleteAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser> FindByIdAsync(string userId)
        {
            int id = int.Parse(userId);

            return
                (from u in _context.Users
                 where u.ID == id
                 select new ApplicationUser()
                 {
                     IdAsInt = u.ID,
                     UserName = u.Name,
                     Email = u.Email
                 }).FirstOrDefaultAsync();
        }

        public Task<ApplicationUser> FindByNameAsync(string userName)
        {
            return
                (from u in _context.Users
                 where u.Name == userName
                 select new ApplicationUser()
                 {
                     IdAsInt = u.ID,
                     UserName = u.Name,
                     Email = u.Email
                 }).FirstOrDefaultAsync();
        }
    
        public Task UpdateAsync(ApplicationUser user)
        {
            var userDM = _context.Users.Find(user.Id);
            
            userDM.Name = user.UserName;
            userDM.Email = user.Email;
            //userDM.PasswordHash = user.PasswordHash;

            return _context.SaveChangesAsync();
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user)
        {
            return
                (from u in _context.Users
                 where u.ID == user.IdAsInt
                 select u.PasswordHash
                 ).FirstOrDefaultAsync();
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user)
        {
            return
                (from u in _context.Users
                 where u.ID == user.IdAsInt
                 select u.PasswordHash.Trim() != ""
                 ).FirstOrDefaultAsync();
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash)
        {
            if (user.Id == null)
            {
                var userDM = UserDM.New();

                userDM.PasswordHash = passwordHash;

                _newUser = userDM;

                return Task.FromResult(0);
            }
            else
            {
                int id = int.Parse(user.Id);

                var userDM = _context.Users.Find(id);

                userDM.PasswordHash = passwordHash;

                return _context.SaveChangesAsync();
            }
        }


        public Task AddClaimAsync(ApplicationUser user, Claim claim)
        {
            return Task.FromResult<int>(0);
        }

        public Task<IList<Claim>> GetClaimsAsync(ApplicationUser user)
        {
            return Task.FromResult<IList<Claim>>(new List<Claim>());
        }

        public Task RemoveClaimAsync(ApplicationUser user, Claim claim)
        {
            return Task.FromResult<int>(0);
        }

    }
}
