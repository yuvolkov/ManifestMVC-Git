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
    public class ArticlesRepo : UOWRepo<ArticlesRepo>
    {
        public ArticlesRepo()
            : base()
        {
        }

        internal ArticlesRepo(ManifestDBContext context)
            : base(context)
        {
        }


        #region Predicates

        internal Expression<Func<T, bool>> IsDueSummariesPredicate<T>() where T : class, IArticleCurrentSummaryDV
        {
            return
                (acs) => acs.ArticleStatus == ArticleStatus.Current &&
                       (acs.NewStatus == ArticleVersionStatus.BeingReviewed && acs.NewDateOfReviewEnding < DateTime.Now ||
                        acs.CurrentStatus == ArticleVersionStatus.Current && acs.CurrentDateOfAltering < DateTime.Now);
        }

        #endregion


        //public IEnumerable<ArticleVM> GetArticles(bool showClosed, bool includeGroup = false)
        //{
        //    IQueryable<ArticleDM> articlesQuery;

        //    if (!showClosed)
        //        articlesQuery =
        //            from a in _context.Articles
        //            where a.Status == ArticleStatus.Current && a.Group.SubdomainID == RequestParameters.SubdomainID
        //            select a;
        //    else // all
        //        articlesQuery =
        //            from a in _context.Articles
        //            select a;

        //    if (includeGroup)
        //        articlesQuery = articlesQuery.Include(a => a.Group);

        //    return (from a in articlesQuery.ToList()
        //            select (ArticleVM) new ArticleVM().InjectFrom(a));
        //}

        public ArticleVM FindArticle(int id, bool includeGroup = false)
        {
            // retrieving existing data
            var articlesQuery =
                from a in _context.Articles
                where a.ID == id
                select a;

            if (includeGroup)
                articlesQuery = articlesQuery.Include(a => a.Group);

            var articleDM = articlesQuery.SingleOrDefault();

            // exists?

            if (articleDM == null)
                return null;

            return
                new ArticleVM().InjectFrom(articleDM) as ArticleVM;
        }


        public void UpdateOrCreateArticle(ArticleVM avm, int userId)
        {
            if (avm.ID == 0)
            {
                var adm = ArticleDM.New(_context, userId);
                adm.UpdateFrom(avm);
            }
            else
            {
                var adm = _context.Articles.Find(avm.ID);

                if (adm.Status != ArticleStatus.Current)
                    throw new InvalidOperationException();

                adm.UpdateFrom(avm);
            }

            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }


        public void CloseArticle(int articleId, int userId)
        {
            // optimistic-throws, down-the-chain-delegating

            var articleDM = _context.Articles.Find(articleId);

            articleDM.Close(_context, userId);

            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }


        public void BeginReviewing(int articleId, int userId)
        {
            // optimistic-throws, down-the-chain-delegating

            var articleSummaryDM =
                 _context.ArticleCurrentSummaries
                 .Include(s => s.NewVersion)
                 .SingleOrDefault(s => s.ArticleID == articleId);
            
            articleSummaryDM.NewVersion.BeginReviewing(_context, userId);


            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }


        public void RejectReviewed(int articleId, int userId)
        {
            // optimistic-throws, down-the-chain-delegating

            var articleSummaryDM =
                 _context.ArticleCurrentSummaries
                 .Include(s => s.NewVersion)
                 .Include(s => s.NewVersion.ReviewRateable)
                 .SingleOrDefault(s => s.ArticleID == articleId);
            
            articleSummaryDM.NewVersion.RejectReviewed(_context, userId);


            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }


        public void MakeCurrent(int articleId, int userId)
        {
            // optimistic-throws, down-the-chain-delegating

            var articleSummaryDM =
                 _context.ArticleCurrentSummaries
                 .Include(s => s.NewVersion)
                 .Include(s => s.NewVersion.ReviewRateable)
                 .Include(s => s.CurrentVersion)
                 .Include(s => s.CurrentVersion.ReviewRateable)         // when archived closes ReviewRateable
                 .Include(s => s.CurrentVersion.PublicationRateable)    // when archived closes PublicationRateable
                 .SingleOrDefault(s => s.ArticleID == articleId);
            
            // checks

            if (!(articleSummaryDM.CurrentID == null && articleSummaryDM.NewVersion.Status == ArticleVersionStatus.BeingEdited ||
                  articleSummaryDM.CurrentID != null && articleSummaryDM.NewVersion.Status == ArticleVersionStatus.BeingReviewed))
                throw new InvalidOperationException();

            // setting

            articleSummaryDM.NewVersion.MakeCurrent(_context, userId);

            if (articleSummaryDM.CurrentVersion != null)                    // if not exists - not a first version
                articleSummaryDM.CurrentVersion.Archive(_context, userId);


            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }


        public void BeginAltering(int articleId, int userId)
        {
            // optimistic-throws, down-the-chain-delegating

            var articleSummaryDM =
                 _context.ArticleCurrentSummaries
                 .Include(s => s.NewVersion)
                 .Include(s => s.CurrentVersion)
                 .SingleOrDefault(s => s.ArticleID == articleId);

            // checks

            if (articleSummaryDM.NewVersion != null)                    // if exists - ???
                throw new InvalidOperationException();

            articleSummaryDM.CurrentVersion.BeginAltering(_context, userId);


            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }


        public void AddReviewVote(
            int articleId, int voterId, VoteResult voteResult)
        {
            var articleSummaryDM =
                 _context.ArticleCurrentSummaries
                 .Include(s => s.NewReviewRateable)
                 .SingleOrDefault(s => s.ArticleID == articleId);

            // checks

            if (articleSummaryDM.NewStatus != ArticleVersionStatus.BeingReviewed)
                throw new InvalidOperationException();

            VoteDM vote = VoteDM.NewWithChecks(
                _context,
                articleSummaryDM.NewReviewRateable,
                voterId,
                voteResult);


            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }



        [Conditional("DEBUG")]
        public void AddVirtualReviewVotes(
            int articleId, uint numberOfVotes, VoteResult voteResult)
        {
            var articleSummaryDM =
                 _context.ArticleCurrentSummaries
                 .Include(s => s.NewReviewRateable)
                 .SingleOrDefault(s => s.ArticleID == articleId);

            // checks

            if (articleSummaryDM.NewStatus != ArticleVersionStatus.BeingReviewed)
                throw new InvalidOperationException();

            VoteDM.NewVirtualVotesWithChecks(
                _context,
                articleSummaryDM.NewReviewRateable,
                numberOfVotes,
                voteResult);


            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }



        public void ZeroActiveDateCounters(int articleId)
        {
            // enviroment-ok, down-the-chain-delegating

            var articleSummaryDM =
                 _context.ArticleCurrentSummaries
                 .Include(s => s.CurrentVersion)
                 .Include(s => s.NewVersion)
                 .SingleOrDefault(s => s.ArticleID == articleId);

            if (articleSummaryDM.NewStatus == ArticleVersionStatus.BeingReviewed)
            {
                articleSummaryDM.NewVersion.ZeroActiveDateCounters();
            }
            else if (articleSummaryDM.CurrentStatus == ArticleVersionStatus.Current)
            {
                articleSummaryDM.CurrentVersion.ZeroActiveDateCounters();
            }
            else
                throw new InvalidOperationException();


            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }


        public void SetActiveDateCounters(int articleId)
        {
            // enviroment-ok, down-the-chain-delegating

            var articleSummaryDM =
                 _context.ArticleCurrentSummaries
                 .Include(s => s.CurrentVersion)
                 .Include(s => s.NewVersion)
                 .SingleOrDefault(s => s.ArticleID == articleId);

            if (articleSummaryDM.NewStatus == ArticleVersionStatus.BeingReviewed)
            {
                articleSummaryDM.NewVersion.SetActiveDateCounters();
            }
            else if (articleSummaryDM.CurrentStatus == ArticleVersionStatus.Current)
            {
                articleSummaryDM.CurrentVersion.SetActiveDateCounters();
            }
            else
                throw new InvalidOperationException();


            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }


        public int GetPendingChangesNumber()
        {
            var predicateDueSummaries = IsDueSummariesPredicate<ArticleCurrentSummaryDV>();

            int count =
                (from acs in _context.ArticleCurrentSummaries.Where(predicateDueSummaries)
                 where acs.Group.SubdomainID == RequestParameters.SubdomainID
                 select acs).Count();

            return count;
        }


        public void ProcessPendingChanges()
        {
            var predicateDueSummaries = IsDueSummariesPredicate<ArticleCurrentSummaryWithVotesDV>();

            var summaries = 
                (new ArticleCurrentSummariesRepo(_context))
                ._GetArticleCurrentSummariesWithVotes(
                    null, null, predicateDueSummaries,
                    includeArticle: true, includeCurrentVersion: true, includeNewVersion: true);

            AlterationsRepo ar = new AlterationsRepo(_context);

            // cycle through

            foreach (var summary in summaries)
            {
                // New.UnderReview:
                if (summary.NewStatus == ArticleVersionStatus.BeingReviewed)
                {
                    // Is it accepted?
                    if (summary.IsReviewAccepted.Value)
                        MakeCurrent(summary.ArticleID, ConfigRepo.Instance.SystemUserID);
                    else if (summary.IsReviewRejected.Value)
                        RejectReviewed(summary.ArticleID, ConfigRepo.Instance.SystemUserID);
                    else
                        SetActiveDateCounters(summary.ArticleID);
                }

                // Current.Current
                if (summary.CurrentStatus == ArticleVersionStatus.Current)
                {
                    var altsDM =
                        _context.Alterations
                        .Where(a => a.ArticleVersionID == summary.CurrentID)
                        .ToList();

                    var altsVM =
                        _context.fAlterationsWithVotes(/* null ?? -1 */ -1)
                        .Where(a => a.ArticleVersionID == summary.CurrentID)
                        .ToList()
                        .Select(aVDM => (new AlterationWithVotesVM()).InjectFrom(aVDM) as AlterationWithVotesVM);

                    var acceptedAltsDM =
                        altsVM
                        .Where(aVM => aVM.IsAccepted)
                        .Select(aVM =>
                            altsDM.First(a => a.ID == aVM.ID));

                    if (acceptedAltsDM.Any())
                    {
                        foreach (var altVDM in acceptedAltsDM)
                        {
                            if (altVDM.Status == AlterationStatus.Active)
                                altVDM.SetActive(_context, ConfigRepo.Instance.SystemUserID);
                        }

                        // will close alterations rateables
                        BeginAltering(summary.ArticleID, ConfigRepo.Instance.SystemUserID);
                    }
                    else
                    {
                        SetActiveDateCounters(summary.ArticleID);
                    }
                }
            }


            // Commit on success

            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }

    }
}
