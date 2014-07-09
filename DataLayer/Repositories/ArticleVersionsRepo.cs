using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using System.Reflection;
using System.Linq.Expressions;

using Omu.ValueInjecter;

using DataLayer.DataModels;
using DataLayer.ViewModels;


namespace DataLayer.Repositories
{
    public class ArticleVersionsRepo : UOWRepo<ArticleVersionsRepo>
    {
        public ArticleVersionsRepo()
            : base()
        {
        }

        internal ArticleVersionsRepo(ManifestDBContext context)
            : base(context)
        {
        }


        //public RequestResult<ArticleVersionHeaderVM> FindArticleVersionHeader(int? id)
        //{
        //    Func<ReqResultStatus,
        //        ArticleVersionHeaderVM,
        //        RequestResult<ArticleVersionHeaderVM>> retValue =
        //        (reqResult, data) => new RequestResult<ArticleVersionHeaderVM>(reqResult, data);

        //    // preconditions
        //    if (id == null)
        //        return retValue(ReqResultStatus.BadRequest, null);

        //    var query =
        //        (from v in _context.ArticleVersions
        //            where v.ID == id
        //            select new {
        //                ID                = v.ID,
        //                ArticleID         = v.ArticleID,
        //                Status            = v.Status,
        //                PublicationStatus = v.PublicationStatus,
        //                VersionString     = v.VersionString });

        //    return
        //        retValue(
        //            ReqResultStatus.Success,
        //            (from v in query.ToList()
        //             select (new ArticleVersionHeaderVM()).InjectFrom(v) as ArticleVersionHeaderVM
        //            ).SingleOrDefault());
        //}


        public ArticleVersionVM FindArticleVersion(
            int id, bool includeArticle = false)
        {
            // retrieving existing data
            var query =
                (from v in _context.ArticleVersions
                 where v.ID == id
                 select v);

            if (includeArticle)
                query = query.Include(v => v.Article);

            var versionVM = 
                (from v in query.ToList()
                    select (new ArticleVersionVM()).InjectFrom(v) as ArticleVersionVM
                ).SingleOrDefault();

            // exists?

            if (versionVM == null)
                return null;

            return versionVM;
        }


        public IEnumerable<ArticleVersionVM> GetArticleVersions(
            int articleId, bool showRejected)
        {
            IQueryable<ArticleVersionDM> query;

            if (!showRejected)
                query =
                    from v in _context.ArticleVersions
                    where v.ArticleID == articleId && 
                          v.Status != ArticleVersionStatus.Rejected
                    select v;
            else // all
                query =
                    from v in _context.ArticleVersions
                    where v.ArticleID == articleId
                    select v;

            query = query.OrderByDescending(v => v.ID);

            return 
                (from v in query.ToList()
                 select (ArticleVersionVM)new ArticleVersionVM().InjectFrom(v));
        }


        //public RequestResult<ArticleVersionVM> GetPreviousNotRejectedVersionFor(int? id, int? articleId)
        //{
        //    // id can be null (for a new version), then we get the current version

        //    // preconditions
        //    if (articleId == null)
        //        return RequestResult.Create<ArticleVersionVM>(ReqResultStatus.BadRequest, null);
                
        //    // retrieving existing data
        //    var query =
        //        (from v in _context.ArticleVersions
        //         let maxPrevId = (from vPrev in _context.ArticleVersions
        //                          where vPrev.ArticleID == articleId.Value &&
        //                                (id == null || vPrev.ID < id) &&
        //                                vPrev.Status != ArticleVersionStatus.Rejected
        //                          select vPrev.ID).Max()
        //         where v.ID == maxPrevId
        //         select v);

        //    var versionVM =
        //        (from v in query.ToList()
        //         select (new ArticleVersionVM()).InjectFrom(v) as ArticleVersionVM
        //        ).SingleOrDefault();

        //    if (versionVM == null)
        //        return RequestResult.Create<ArticleVersionVM>(ReqResultStatus.NotFound, null);

        //    return
        //        RequestResult.Create(
        //            ReqResultStatus.Success,
        //            versionVM);
        //}



        //public RequestResult<IEnumerable<AlterationWithVotesVM>> GetAcceptedSourceAlterationsFor(int? versionId, int? articleId)
        //{
        //    // id can be null (for a new version), then we get the current version

        //    // preconditions
        //    if (versionId == null && articleId == null)
        //        return RequestResult.Create(ReqResultStatus.BadRequest, (IEnumerable<AlterationWithVotesVM>)null);

        //    // obtaining source version

        //    int? sourceVersionId;

        //    if (versionId == null)
        //    {
        //        var artCurSumm = (from acs in _context.ArticleCurrentSummaries
        //                          where acs.ArticleID == articleId.Value
        //                          select acs)
        //                         .SingleOrDefault();

        //        if (artCurSumm == null)
        //            return RequestResult.Create(ReqResultStatus.NotFound, (IEnumerable<AlterationWithVotesVM>)null);

        //        sourceVersionId = artCurSumm.CurrentID;
        //    }
        //    else
        //    {
        //        var ver = (from v in _context.ArticleVersions
        //                   where v.ID == versionId.Value
        //                   select v)
        //                  .SingleOrDefault(); ;

        //        if (ver == null)
        //            return RequestResult.Create(ReqResultStatus.NotFound, (IEnumerable<AlterationWithVotesVM>)null);

        //        sourceVersionId = ver.SourceVersionID;
        //    }

        //    if (sourceVersionId == null)
        //        return RequestResult.Create(ReqResultStatus.NotFound, (IEnumerable<AlterationWithVotesVM>)null);

        //    // obtaining source alterations

        //    return
        //        (new AlterationsRepo(_context))
        //        .GetAlterationsWithVotes(versionId, null, acceptedOnly: true);
        //}



        public void UpdateOrCreateArticleVersion(ArticleVersionVM avvm, int userId)
        {
            // retrieving Article Summary
            var articleSummary = 
                (new ArticleCurrentSummariesRepo(_context))
                    .FindArticleCurrentSummary(avvm.ArticleID, includeArticle: false);

            if (articleSummary.ArticleStatus != ArticleStatus.Current)
                throw new InvalidOperationException();

            // retrieving previous Version
            ArticleVersionDM sourceAvdm = null;
            if (articleSummary.CurrentID != null)
                sourceAvdm = _context.ArticleVersions.Find(articleSummary.CurrentID);

            if (avvm.ID == 0)
            {
                // new one
                if (articleSummary.NewID > 0)
                    throw new InvalidOperationException();

                string versionString;
                if (articleSummary.CurrentID == null)
                    versionString = "1.";
                else
                    versionString = String.Format("{0:d}.", int.Parse( articleSummary.CurrentVersionString.Split('.')[0] ) + 1);
                versionString += articleSummary.RejectedCount.ToString();

                var avdm = ArticleVersionDM.New(_context, avvm.ArticleID, articleSummary.CurrentID, versionString, userId);

                // will validate all
                avdm.UpdateFrom(avvm, sourceAvdm);

                // retrieving Id

                _context.OnCommit += () => { avvm.ID = avdm.ID; };
            }
            else
            {
                var avdm = _context.ArticleVersions.Find(avvm.ID);

                // will validate all
                avdm.UpdateFrom(avvm, sourceAvdm);
            }


            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }


    }
}
