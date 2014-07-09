using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DataLayer.DataModels
{
    [Table("vArticleCurrentSummariesWithVotes")]    // we use TVF for this entity, but EF also joins the view
    internal class ArticleCurrentSummaryWithVotesDV : BaseArticleCurrentSummaryDV, IArticleCurrentSummaryDV
    {
        public RateableStatus? NewReviewRateableStatus { get; protected set; }

        public int? NewReviewTotalAcceptVotes { get; protected set; }

        public int? NewReviewTotalRejectVotes { get; protected set; }



        public int? NewReviewVoterID { get; protected set; }

        public VoteResult? NewReviewVoteResult { get; protected set; }

        [ForeignKey("NewReviewVoterID")]
        public UserDM NewReviewVoter { get; protected set; }
    }
}
