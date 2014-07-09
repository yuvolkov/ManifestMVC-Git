using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Omu.ValueInjecter;

//using ManifestMVC.Extensions;
using DataLayer.Repositories;
using DataLayer.ViewModels;


namespace ManifestMVC.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            var summaries =
                ArticleCurrentSummariesRepo.UOW()
                    .GetArticleCurrentSummaries(
                        showClosed: false,
                        showWithoutCurrentVersion: false,
                        includeGroup: true);

            return View(summaries);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult FAQ()
        {
            return View();
        }

        public ActionResult Slides()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
    }
}