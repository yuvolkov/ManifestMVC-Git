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
    [Authorize(Roles="Admin")]
    public class TechArticlesController : BaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("ArticlesIndex");
        }


        public ActionResult ArticlesIndex()
        {
            var pendingChangesCount =
                ArticlesRepo.UOW()
                    .GetPendingChangesNumber();

            ViewBag.PendingChangesCount = pendingChangesCount;

            return View();
        }


        public ActionResult _ListArticles(bool? isShowClosed)
        {
            var summaries =
                ArticleCurrentSummariesRepo.UOW()
                    .GetArticleCurrentSummaries(
                        showClosed: isShowClosed ?? false,
                        showWithoutCurrentVersion: true,
                        includeGroup: true);

            return PartialView(summaries);
        }


        public ActionResult VersionsIndex(int? id)
        {
            var summary =
                ArticleCurrentSummariesRepo.UOW()
                .FindArticleCurrentSummary(id.Value, includeArticle: true, includeGroup: true);

            return View(summary);
        }


        public ActionResult _ListVersions(int? id, bool? isShowRejected)
        {
            var versions =
                ArticleVersionsRepo.UOW()
                    .GetArticleVersions(
                        id.Value,
                        showRejected: isShowRejected ?? false);

            return PartialView(versions);
        }


        public ActionResult _EditArticle(int? id)
        {
            if (id == null || id == 0)
                return PartialView(new ArticleVM());

            var article = 
                ArticlesRepo.UOW()
                    .FindArticle(id.Value);

            return PartialView(article);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditArticle(ArticleVM avm)
        {
            if (ModelState.IsValid)
            {
                ArticlesRepo.UOW(autocommitOnSuccess: true)
                    .UpdateOrCreateArticle(avm, UserID.Value);
                
                return Json(new { redirectTo = "reload" });
            }

            // not valid!
            return PartialView(avm);
        }


        public ActionResult _CloseArticle(int? id)
        {
            var article =
                ArticlesRepo.UOW()
                    .FindArticle(id.Value);

            return PartialView(article);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CloseArticle(int id)
        {
            ArticlesRepo.UOW(autocommitOnSuccess: true)
                .CloseArticle(id, UserID.Value);

            return Json(new { redirectTo = Url.Action("Index") });
        }


        public ActionResult _ProcessPendingChanges()
        {
            ArticlesRepo.UOW(autocommitOnSuccess: true)
                .ProcessPendingChanges();
            
            return Json(new { redirectTo = "reload" }, JsonRequestBehavior.AllowGet);
        }
    }
}
