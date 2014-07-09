using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;

using Omu.ValueInjecter;
using LinqKit;

using DataLayer.Repositories.Settings;
using DataLayer.DataModels;
using DataLayer.ViewModels;


namespace DataLayer.Repositories
{
    public class GroupsRepo : UOWRepo<GroupsRepo>
    {
        public GroupsRepo()
            : base()
        {
        }

        internal GroupsRepo(ManifestDBContext context)
            : base(context)
        {
        }


        public IEnumerable<ArticleGroupVM> GetGroups()
        {
            var query =
                (from g in _context.ArticleGroups
                 where g.SubdomainID == RequestParameters.SubdomainID
                 select g);

            return (from g in query.ToList()
                    select (ArticleGroupVM)new ArticleGroupVM().InjectFrom(g));
        }


        public void UpdateOrCreateGroup(ArticleGroupVM gvm)
        {
            if (gvm.ID == 0)
            {
                var gdm = ArticleGroupDM.New(_context, subdomainID: RequestParameters.SubdomainID);
                gdm.UpdateFrom(gvm);
            }
            else
            {
                var gdm = _context.ArticleGroups.Find(gvm.ID);

                gdm.UpdateFrom(gvm);
            }

            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }


        public void DeleteGroup(int groupId)
        {
            // optimistic-throws, down-the-chain-delegating

            var groupDM = _context.ArticleGroups.Find(groupId);

            groupDM.Delete(_context);

            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }
    }
}
