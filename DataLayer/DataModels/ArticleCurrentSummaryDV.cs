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
    /// <summary>
    /// Interface to build predicates against
    /// </summary>
    internal interface IArticleCurrentSummaryDV
    {
        ArticleStatus ArticleStatus { get; }

        ArticleVersionStatus? CurrentStatus { get; }
        DateTime? CurrentDateOfAltering { get; }

        ArticleVersionStatus? NewStatus { get; }
        DateTime? NewDateOfReviewEnding { get; }
    }




    internal class BaseArticleCurrentSummaryDV
    {
        [Key]
        public int ArticleID { get; protected set; }

        public int GroupID { get; set; }

        public ArticleStatus ArticleStatus { get; protected set; }

        public string Title { get; protected set; }

        public int? CurrentID { get; protected set; }
        public ArticleVersionStatus? CurrentStatus { get; protected set; }
        public string CurrentVersionString { get; protected set; }
        public DateTime? CurrentDateOfAltering { get; protected set; }

        //public int? CurrentPublicationRateableID { get; protected set; }

        public int? NewID { get; protected set; }
        public ArticleVersionStatus? NewStatus { get; protected set; }
        public string NewVersionString { get; protected set; }
        public int? NewReviewRateableID { get; protected set; }
        public DateTime? NewDateOfReviewEnding { get; protected set; }

        public int RejectedCount { get; protected set; }

        [ForeignKey("ArticleID")]
        public ArticleDM Article { get; protected set; }

        [ForeignKey("GroupID")]
        public ArticleGroupDM Group { get; set; }

        [ForeignKey("CurrentID")]
        public ArticleVersionDM CurrentVersion { get; protected set; }

        //[ForeignKey("CurrentPublicationRateableID")]
        //public RateableDM CurrentPublicationRateable { get; set; }


        [ForeignKey("NewID")]
        public ArticleVersionDM NewVersion { get; protected set; }

        [ForeignKey("NewReviewRateableID")]
        public RateableDM NewReviewRateable { get; set; }
    }



    [Table("vArticleCurrentSummaries")]
    internal class ArticleCurrentSummaryDV : BaseArticleCurrentSummaryDV, IArticleCurrentSummaryDV
    {
    }
}
