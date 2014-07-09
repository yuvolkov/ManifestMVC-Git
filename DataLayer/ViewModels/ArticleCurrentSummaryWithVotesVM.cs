using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using WebSafeLibrary;
using DataLayer;
using DataLayer.DataModels;
using DataLayer.Repositories;
using DataLayer.Repositories.Settings;


namespace DataLayer.ViewModels
{
    public class ArticleCurrentSummaryWithVotesVM : ArticleCurrentSummaryVM
    {
        public int? NewReviewTotalAcceptVotes { get; set; }

        public int? NewReviewTotalRejectVotes { get; set; }

        public VoteResult? NewReviewVoteResult { get; set; }



        #region Calculated Properies

        public float? ReviewConsensus
        {
            get
            {
                if (NewID == null)
                    return null;

                if (NewReviewTotalAcceptVotes + NewReviewTotalRejectVotes == 0)
                    return 0;

                return (float)NewReviewTotalAcceptVotes / (NewReviewTotalAcceptVotes + NewReviewTotalRejectVotes);
            }
        }

        public bool? IsReviewSandbox
        {
            get
            {
                if (NewID == null)
                    return null;

                return (NewReviewTotalAcceptVotes + NewReviewTotalRejectVotes <= RequestParameters.SubdomainParams.MathReviewSandboxYesNoQuorum);
            }
        }

        public bool? IsReviewRejected
        {
            get
            {
                if (NewID == null)
                    return null;

                return
                    !IsReviewSandbox.Value &&
                    ReviewConsensus <= RequestParameters.SubdomainParams.MathReviewRejectConsensus;
            }
        }

        public bool? IsReviewAccepted
        {
            get
            {
                if (NewID == null)
                    return null;

                return
                    NewReviewTotalAcceptVotes >= RequestParameters.SubdomainParams.MathReviewAcceptYesQuorum &&
                    ReviewConsensus >= RequestParameters.SubdomainParams.MathReviewAcceptConsensus;
            }
        }

        #endregion
    }

}