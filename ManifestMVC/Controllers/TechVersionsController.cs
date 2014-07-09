using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

using Omu.ValueInjecter;

//using ManifestMVC.Extensions;
using ManifestMVC.ViewModels.Root;
using DataLayer;
using DataLayer.Repositories;
using DataLayer.ViewModels;


namespace ManifestMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TechVersionsController : BaseController
    {
        /// <summary>
        /// Выдает заголовок статьи вообще без текстов
        /// </summary>
        /// <param name="id"></param>
        /// <param name="articleID"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        public ActionResult Version(int? id, int? articleID, string act)
        {
            // parameter preconditions
            if ((id == null || id == 0) && articleID == null ||
                !(new string[]{"view", "edit"}).Contains(act))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // retrieving Version Header

            ArticleVersionVM articleVersion = null;

            if (id == null || id == 0)
            {
                // obtaining current article
                var artCurSumm =
                    ArticleCurrentSummariesRepo.UOW()
                    .FindArticleCurrentSummary(articleID.Value, includeGroup: true);

                // we can create new version

                articleVersion = new ArticleVersionVM()
                    {
                        ID = 0,
                        ArticleID = articleID.Value,
                        SourceVersionID = artCurSumm.CurrentID
                    };
            }
            else
            {
                articleVersion =
                    ArticleVersionsRepo.UOW()
                        .FindArticleVersion(id.Value);
            }

            // retrieving alterations

            IEnumerable<AlterationWithVotesVM> alterationsWithVotes = new List<AlterationWithVotesVM>();

            if (articleVersion.SourceVersionID != null)
            {
                alterationsWithVotes =
                    AlterationsRepo.UOW()
                    .GetAlterationsWithVotes(articleVersion.SourceVersionID.Value, null, acceptedOnly: true);
            }

            // retrieving existing data
            return
                View(
                    ModelWithAction.Create(
                        act, Tuple.Create(articleVersion, alterationsWithVotes)));
        }


        public ActionResult _ViewVersion(int? id, int? articleID)
        {
            if ((id == 0 || id == null) && articleID != null)
            {
                // maybe we have some version?

                var artCurSumm =
                    ArticleCurrentSummariesRepo.UOW()
                    .FindArticleCurrentSummary(articleID.Value, includeCurrentVersion: true, includeNewVersion: true, includeGroup: true);

                if (artCurSumm.NewVersion != null)
                    return PartialView(artCurSumm.NewVersion);

                if (artCurSumm.CurrentVersion != null)
                    return PartialView(artCurSumm.NewVersion);

                // no version? then redirrect to the begining

                return Json(new { redirectTo = Url.Action("VersionsIndex", "TechArticles", new { id = articleID }) }, JsonRequestBehavior.AllowGet);
            }

            var version =
                ArticleVersionsRepo.UOW()
                    .FindArticleVersion(id.Value);

            return PartialView(version);
        }


        public ActionResult _EditVersion(int? id, int? articleID)
        {
            // new
            if (id == null || id == 0)
            {
                if (articleID == null)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                var artCurSumm =
                    ArticleCurrentSummariesRepo.UOW()
                    .FindArticleCurrentSummary(articleID.Value, includeCurrentVersion: true, includeGroup: true);

                if (!artCurSumm.CanAddNewVersion)
                    throw new InvalidOperationException();

                // Create New
                return PartialView(new ArticleVersionVM() {
                    ArticleID            = articleID.Value,
                    Text                 = (artCurSumm.CurrentVersion != null ? artCurSumm.CurrentVersion.Text : ""),
                    SourceTextWithMarkup = (artCurSumm.CurrentVersion != null ? artCurSumm.CurrentVersion.Text : "")
                });
            }

            // retrieving existing data
            var version =
                ArticleVersionsRepo.UOW()
                    .FindArticleVersion(id.Value);

            if (!version.CanEdit)
                throw new InvalidOperationException();

            return PartialView(version);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditVersion(ArticleVersionVM verVM)
        {
            if (ModelState.IsValid)
            {
                ArticleVersionsRepo.UOW(autocommitOnSuccess: true)
                    .UpdateOrCreateArticleVersion(verVM, UserID.Value);

                return RedirectToAction("_ViewVersion", new { id = verVM.ID});
            }

            // not valid!
            return PartialView(verVM);
        }


        public ActionResult _ConfirmAction(int? id, string act)
        {
            // parameter preconditions
            if (!(new string[] { "review", "updateCurrent", "zero-review-date", "zero-altering-date" }).Contains(act))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var version =
                ArticleVersionsRepo.UOW()
                    .FindArticleVersion(id.Value);

            return PartialView(ModelWithAction.Create(act, version));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ConfirmAction(ModelWithAction<ArticleVersionVM> data)
        {
            ArticleVersionVM articleVM = data.VM;
            string act = data.Action;

            switch (act)
            {
                case "review":
                    ArticlesRepo.UOW(autocommitOnSuccess: true)
                        .BeginReviewing(articleVM.ArticleID, UserID.Value);
                    break;
                //case "reject":
                //    reqResult =
                //        ArticlesRepo.UOW(autocommitOnSuccess: true)
                //            .RejectReviewed(headerVM.ArticleID);
                //    break;
                case "updateCurrent":
                    ArticlesRepo.UOW(autocommitOnSuccess: true)
                        .MakeCurrent(articleVM.ArticleID, UserID.Value);
                    break;
                //case "alter":
                //    reqResult =
                //        ArticlesRepo.UOW(autocommitOnSuccess: true)
                //            .BeginAltering(headerVM.ArticleID);
                //    break;
                case "zero-review-date":
                    ArticlesRepo.UOW(autocommitOnSuccess: true)
                        .ZeroActiveDateCounters(articleVM.ArticleID);
                    break;
                case "zero-altering-date":
                    ArticlesRepo.UOW(autocommitOnSuccess: true)
                        .ZeroActiveDateCounters(articleVM.ArticleID);
                    break;
                default:
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return Json(new { redirectTo = Url.Action("Index", "TechArticles") });
        }
    }
}
