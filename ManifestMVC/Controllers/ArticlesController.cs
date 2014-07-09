using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

using Omu.ValueInjecter;

//using ManifestMVC.Extensions;
using ManifestMVC.ViewModels;
using ManifestMVC.ViewModels.Root;
using ManifestMVC.ViewModels.Shared;
using DataLayer;
using DataLayer.Repositories;
using DataLayer.ViewModels;
using System.Security.Claims;


namespace ManifestMVC.Controllers
{
    public class ArticlesController : BaseController
    {
        //public ActionResult Index()
        //{
        //    return View( ArticlesRepo.UOW().GetArticles(showClosed: false, includeGroup: true) );
        //}


        public ActionResult Current(int? id)
        {
            var articleCurrSummary = ArticleCurrentSummariesRepo.UOW()
                .FindArticleCurrentSummaryWithVotes(
                    id.Value, UserID, 
                    includeArticle: true, includeCurrentVersion: true, includeNewVersion: true,
                    includeGroup: true);

            List<AlterationWithVotesVM> alterationsWithVotesList = null;

            ModelWithAction<AlterationVM> newAlteration = null;

            if (articleCurrSummary.CurrentVersion.Status == ArticleVersionStatus.Current)
            {
                alterationsWithVotesList =
                    AlterationsRepo.UOW()
                    .GetAlterationsWithVotes(articleCurrSummary.CurrentID.Value, UserID)
                    .ToList();

                newAlteration = ModelWithAction.Create("new", new AlterationVM() { ArticleVersionID = articleCurrSummary.CurrentID.Value });
            }
            else if (articleCurrSummary.CurrentVersion.Status == ArticleVersionStatus.BeingAltered)
            {
                alterationsWithVotesList =
                    AlterationsRepo.UOW()
                        .GetAlterationsWithVotes(articleCurrSummary.CurrentID.Value, UserID, acceptedOnly: true)
                        .ToList();
            }

            alterationsWithVotesList.Sort(new Comparison<AlterationWithVotesVM>((x1, x2) => x2.CompareTo(x1)));
            var alterationsWithVotes = (IEnumerable<AlterationWithVotesVM>) alterationsWithVotesList;

            return View(Tuple.Create(articleCurrSummary, alterationsWithVotes, newAlteration));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditAlteration(ModelWithAction<AlterationVM> mwaAlt, string submitButton)
        {
            string newAction = submitButton;

            switch (newAction)
            {
                case "new":
                    ModelState.Clear();
                    mwaAlt.VM = new AlterationVM() { ArticleVersionID = mwaAlt.VM.ArticleVersionID };
                    break;

                case "edit":
                    break;      // nothing to do

                case "confirm":
                    if (!ModelState.IsValid)
                    {
                        // back to edit
                        newAction = "edit";
                        goto case "edit";
                    }

                    // valid
                    break;

                case "save":
                    if (ModelState.IsValid)
                    {                        
                        AlterationsRepo.UOW(autocommitOnSuccess: true)
                            .UpdateOrCreateAlteration(mwaAlt.VM, UserID.Value);

                        return Json(new { redirectTo = "reload" });
                    }

                    // not valid!
                    break;

                default:
                    throw new InvalidOperationException();
            }

            // preparing View

            mwaAlt.Action = newAction;

            return PartialView(mwaAlt);
        }


        public ActionResult _ConfirmVote(bool? votingIsOpen, bool? alreadyVoted, int? id, string vote)
        {
            // preconditions
            if (votingIsOpen == null || alreadyVoted == null || id == null || vote == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            VoteResult voteResult;

            switch (vote)
            {
                case "accept":
                    voteResult = VoteResult.Accept;
                    break;
                case "reject":
                    voteResult = VoteResult.Reject;
                    break;
                default:
                    throw new NotImplementedException();
            }

            //return PartialView(Tuple.Create("Articles", "_ConfirmVote", votingAllowed.Value, alreadyVoted.Value, id.Value, voteResult));
            return PartialView(new ConfirmVoteModel() { 
                Controller = "Articles", 
                Action = "_ConfirmVote",
                VotingIsOpen = votingIsOpen.Value, 
                AlreadyVoted = alreadyVoted.Value, 
                ItemId = id.Value,
                Vote = voteResult 
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ConfirmVote(int? itemId, VoteResult? vote, string testVotesNumber)
        {            
            var repo = AlterationsRepo.UOW(autocommitOnSuccess: false);
            
            repo.AddVote(itemId.Value, UserID.Value, vote.Value);

#if DEBUG
            if (testVotesNumber != null)
            {
                uint value;
                if (uint.TryParse(testVotesNumber, out value))
                    repo.AddVirtualVotes(itemId.Value, value, vote.Value);
            }
#endif

            repo.Commit();

            return Json(new { redirectTo = "reload" });
        }



        public ActionResult _ConfirmReviewVote(bool? votingIsOpen, bool? alreadyVoted, int? id, string vote)
        {
            // preconditions
            if (votingIsOpen == null || alreadyVoted == null || id == null || vote == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            VoteResult voteResult;

            switch (vote)
            {
                case "accept":
                    voteResult = VoteResult.Accept;
                    break;
                case "reject":
                    voteResult = VoteResult.Reject;
                    break;
                default:
                    throw new NotImplementedException();
            }

            //return PartialView(Tuple.Create("Articles", "_ConfirmVote", votingAllowed.Value, alreadyVoted.Value, id.Value, voteResult));
            return PartialView(
                "_ConfirmVote",
                new ConfirmVoteModel()
                {
                    Controller = "Articles",
                    Action = "_ConfirmReviewVote",
                    VotingIsOpen = votingIsOpen.Value,
                    AlreadyVoted = alreadyVoted.Value,
                    ItemId = id.Value,
                    Vote = voteResult
                });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _ConfirmReviewVote(int? itemId, VoteResult? vote, string testVotesNumber)
        {
            var repo = ArticlesRepo.UOW(autocommitOnSuccess: false);

            repo.AddReviewVote(itemId.Value, UserID.Value, vote.Value);

#if DEBUG
            if (testVotesNumber != null)
            {
                uint value;
                if (uint.TryParse(testVotesNumber, out value))
                    repo.AddVirtualReviewVotes(itemId.Value, value, vote.Value);
            }
#endif

            repo.Commit();

            return Json(new { redirectTo = "reload" });
        }

    }
}
