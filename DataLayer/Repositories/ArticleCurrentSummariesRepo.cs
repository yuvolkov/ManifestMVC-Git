using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using System.Linq.Expressions;

using Omu.ValueInjecter;

using DataLayer.Repositories.Settings;
using DataLayer.DataModels;
using DataLayer.ViewModels;
using DataLayer.Helpers.ValueInjecter;


namespace DataLayer.Repositories
{
    public class ArticleCurrentSummariesRepo : UOWRepo<ArticleCurrentSummariesRepo>
    {
        public ArticleCurrentSummariesRepo()
            : base()
        {
        }

        internal ArticleCurrentSummariesRepo(ManifestDBContext context)
            : base(context)
        {
        }


        //#region Query Extenders

        //// Does not work - we can't do Include to include complex properties!
        //// So the only way to include them is no use View
        //internal static IEnumerable<ArticleCurrentSummaryVM> ExtendWithCurrentSummaries(
        //    IQueryable<ArticleDM> query,
        //    bool skipWithoutCurrentVersion = false, 
        //    bool includeArticle = false, bool includeCurrentVersion = false)
        //{
        //    var acs_query =
        //        (from a in query
        //         let vCur = a.ArticleVersions
        //            .Where(v => v.Status == ArticleVersionStatus.Current ||
        //                        v.Status == ArticleVersionStatus.BeingAltered)
        //            .FirstOrDefault()   // should be only one, by implementation (but no DB enforcements)
        //         let vNew = a.ArticleVersions
        //            .Where(v => v.Status == ArticleVersionStatus.BeingEdited ||
        //                        v.Status == ArticleVersionStatus.BeingReviewed)
        //            .FirstOrDefault()   // should be only one, by implementation (but no DB enforcements)
        //         let rejectedCount = a.ArticleVersions
        //            .Count(v => v.Status == ArticleVersionStatus.Rejected &&
        //                        v.ID > vCur.ID)
        //         select new
        //         {
        //             ArticleID     = a.ID,
        //             ArticleStatus = a.Status,
        //             Title         = a.Title,
        //             CurrentID             = vCur != null ? vCur.ID                 : (int?)                  null,
        //             CurrentStatus         = vCur != null ? vCur.Status             : (ArticleVersionStatus?) null,
        //             CurrentVersionString  = vCur != null ? vCur.VersionString      :                         null,
        //             CurrentDateOfAltering = vCur != null ? vCur.DateOfAltering     : (DateTime?)             null,
        //             NewID                 = vNew != null ? vNew.ID                 : (int?)                  null,
        //             NewStatus             = vNew != null ? vNew.Status             : (ArticleVersionStatus?) null,
        //             NewVersionString      = vNew != null ? vNew.VersionString      :                         null,
        //             NewDateOfReviewEnding = vNew != null ? vNew.DateOfReviewEnding : (DateTime?)             null,
        //             RejectedCount  = rejectedCount,
        //             Article        = includeArticle        ? a    : null,
        //             CurrentVersion = includeCurrentVersion ? vCur : null,
        //             //NewVersion            = vNew
        //         })
        //         .Include(x => x.Article)         // do not work!
        //         .Include(x => x.CurrentVersion); // do not work! 

        //    if (skipWithoutCurrentVersion)
        //        acs_query = acs_query.Where(a => a.CurrentID > 0);

        //    var acs_raw = acs_query.ToList();

        //    var acsList = acs_raw
        //        .Select(acs => 
        //            (new ArticleCurrentSummaryVM())
        //            .InjectFrom<DoNotReadTargetValuesConventionInjection>(acs) as ArticleCurrentSummaryVM)
        //        .ToList();

        //    //if (acsVM.Article != null)
        //    //    cavsVM.Article = (new ArticleVM()).InjectFrom(acsVM.Article) as ArticleVM;
        //    //if (acsVM.CurrentVersion != null)
        //    //    cavsVM.CurrentVersion = (new ArticleVersionVM()).InjectFrom(acsVM.CurrentVersion) as ArticleVersionVM;

        //    return acsList;
        //}

        //#endregion



        public ArticleCurrentSummaryVM FindArticleCurrentSummary(
            int articleId, 
            bool includeArticle = false, bool includeCurrentVersion = false, bool includeNewVersion = false, 
            bool includeGroup = false)
        {
            // retrieving existing data
            var query =
                (from s in _context.ArticleCurrentSummaries
                 where s.ArticleID == articleId
                 select s);

            if (includeArticle)
                query = query.Include(x => x.Article);

            if (includeCurrentVersion)
                query = query.Include(x => x.CurrentVersion);

            if (includeNewVersion)
                query = query.Include(x => x.NewVersion);

            if (includeGroup)
                query = query.Include(x => x.Group);

            var acsDM = query.SingleOrDefault();

            // exists?

            if (acsDM == null)
                return null;

            // processing data

            var acsVM = new ArticleCurrentSummaryVM();
            acsVM.InjectFrom(acsDM);
            if (acsDM.Article != null)
                acsVM.Article = (new ArticleVM()).InjectFrom(acsDM.Article) as ArticleVM;
            if (acsDM.CurrentVersion != null)
                acsVM.CurrentVersion = (new ArticleVersionVM()).InjectFrom(acsDM.CurrentVersion) as ArticleVersionVM;
            if (acsDM.NewVersion != null)
                acsVM.NewVersion = (new ArticleVersionVM()).InjectFrom(acsDM.NewVersion) as ArticleVersionVM;
            if (acsDM.Group != null)
                acsVM.Group = (new ArticleGroupVM()).InjectFrom(acsDM.Group) as ArticleGroupVM;

            return acsVM;
        }



        public IEnumerable<ArticleCurrentSummaryVM> GetArticleCurrentSummaries(
            bool showClosed, bool showWithoutCurrentVersion,
            bool includeGroup = false)
        {
            IQueryable<ArticleCurrentSummaryDV> query;

            if (!showClosed)
                query =
                    from a in _context.ArticleCurrentSummaries
                    where a.ArticleStatus == ArticleStatus.Current && a.Group.SubdomainID == RequestParameters.SubdomainID
                    select a;
            else // all
                query =
                    from a in _context.ArticleCurrentSummaries
                    select a;

            if (!showWithoutCurrentVersion)
                query = query.Where(a => a.CurrentID > 0);

            if (includeGroup)
                query = query.Include(a => a.Group);

            // transforming DM to VM
            var acsVMs =
                query.ToList()
                .Select(a => 
                    { 
                        var avm = new ArticleCurrentSummaryVM().InjectFrom(a) as ArticleCurrentSummaryVM;
                        if (a.Group != null)
                            avm.Group = (new ArticleGroupVM()).InjectFrom(a.Group) as ArticleGroupVM;

                        return avm;
                    });

            return acsVMs;
        }



        public ArticleCurrentSummaryWithVotesVM FindArticleCurrentSummaryWithVotes(
            int articleId, int? voterId,
            bool includeArticle = false, bool includeCurrentVersion = false, bool includeNewVersion = false,
            bool includeGroup = false)
        {
            var acsVM =
                _GetArticleCurrentSummariesWithVotes(
                    articleId, voterId, null,
                    includeArticle: includeArticle, includeCurrentVersion: includeCurrentVersion, includeNewVersion: includeNewVersion,
                    includeGroup: includeGroup)
                .SingleOrDefault();

            if (acsVM == null)
                return null;

            return acsVM;
        }



        internal IEnumerable<ArticleCurrentSummaryWithVotesVM> _GetArticleCurrentSummariesWithVotes(
            int? articleId, int? voterId,
            Expression<Func<ArticleCurrentSummaryWithVotesDV, bool>> predicateWhere = null,
            bool includeArticle = false, bool includeCurrentVersion = false, bool includeNewVersion = false,
            bool includeGroup = false)
        {
            // retrieving existing data

            IQueryable<ArticleCurrentSummaryWithVotesDV> query;

            if (predicateWhere == null)
            {
                query = (from s in _context.fArticleCurrentSummariesWithVotes(voterId ?? -1)
                         where s.ArticleID == articleId
                         select s);
            }
            else
            {
                query = (from s in (_context.fArticleCurrentSummariesWithVotes(voterId ?? -1)).Where(predicateWhere)
                         where s.Group.SubdomainID == RequestParameters.SubdomainID
                         select s);
            }

            if (includeArticle)
                query = query.Include(x => x.Article);

            if (includeCurrentVersion)
                query = query.Include(x => x.CurrentVersion);

            if (includeNewVersion)
                query = query.Include(x => x.NewVersion);

            if (includeGroup)
                query = query.Include(a => a.Group);

            // processing data
            var acsVMs = query.ToList().Select(acsDM =>
                {
                    var acsVM = new ArticleCurrentSummaryWithVotesVM();
                    acsVM.InjectFrom(acsDM);
                    if (acsDM.Article != null)
                        acsVM.Article = (new ArticleVM()).InjectFrom(acsDM.Article) as ArticleVM;
                    if (acsDM.CurrentVersion != null)
                        acsVM.CurrentVersion = (new ArticleVersionVM()).InjectFrom(acsDM.CurrentVersion) as ArticleVersionVM;
                    if (acsDM.NewVersion != null)
                        acsVM.NewVersion = (new ArticleVersionVM()).InjectFrom(acsDM.NewVersion) as ArticleVersionVM;
                    if (acsDM.Group != null)
                        acsVM.Group = (new ArticleGroupVM()).InjectFrom(acsDM.Group) as ArticleGroupVM;

                    return acsVM;
                });

            return acsVMs;
        }

    }
}
