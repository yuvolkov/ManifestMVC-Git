using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DataLayer;
using DataLayer.DataModels;
using DataLayer.Repositories;


namespace DataLayer.ViewModels
{
    public class ArticleCurrentSummaryVM
    {
        public int ArticleID { get; set; }
        public ArticleStatus ArticleStatus { get; set; }

        public string Title { get; set; }

        //public int ReviewRateableID { get; set; }
        //public int PublishRateableID { get; set; }

        public int? CurrentID { get; set; }
        public ArticleVersionStatus? CurrentStatus { get; set; }
        public string CurrentVersionString { get; set; }
        public DateTime? CurrentDateOfAltering { get; set; }

        public int? NewID { get; set; }
        public ArticleVersionStatus? NewStatus { get; set; }
        public string NewVersionString { get; set; }
        public DateTime? NewDateOfReviewEnding { get; set; }

        public int RejectedCount { get; set; }


        public ArticleGroupVM Group { get; set; }

        public ArticleVM Article { get; set; }

        public ArticleVersionVM CurrentVersion { get; set; }

        public ArticleVersionVM NewVersion { get; set; }


        #region Calculated Properies

        public bool CanEditArticle
        {
            get
            {
                return
                    ArticleStatus == ArticleStatus.Current;                             // Article not closed
            }
        }

        public int DaysLeftBeforeAltering
        {
            get
            {
                if (ArticleStatus == ArticleStatus.Current &&                           // Article not closed
                    CurrentID != null &&                                                // exists first version
                    CurrentStatus.Value == ArticleVersionStatus.Current)
                    return (int)((CurrentDateOfAltering.Value - DateTime.Now).TotalDays+1);
                else
                    return 0;
            }
        }

        public int DaysLeftBeforeReviewEnding
        {
            get
            {
                if (ArticleStatus == ArticleStatus.Current &&                           // Article not closed
                    NewID != null &&                                                    // exists new version
                    NewStatus.Value == ArticleVersionStatus.BeingReviewed)
                    return (int)((NewDateOfReviewEnding.Value - DateTime.Now).TotalDays+1);
                else
                    return 0;
            }
        }


        public bool CanAddNewVersion
        {
            get
            {
                return
                    ArticleStatus == ArticleStatus.Current &&            // Article not closed
                    NewID == null &&                                               //  and no new version
                    (CurrentID == null ||                                          //  and Either empty article
                     CurrentStatus.Value == ArticleVersionStatus.BeingAltered);    //       or being altered
            }
        }


        public string DisplayArticleStatus
        {
            get
            {
                if (ArticleStatus == ArticleStatus.Current)
                    return "[[[Article is Active]]]";
                
                if (ArticleStatus == ArticleStatus.Closed)
                    return "[[[Article is Closed]]]";

                throw new ApplicationException("???");
            }
        }

        public string DisplayCurrentVersionStatus
        {
            get 
            {
                if (CurrentID == null)
                    return "[[[No current versions]]]";

                if (CurrentStatus == ArticleVersionStatus.Current)
                    return
                        String.Format("[[[Current version %0|||{0}]]] - ", CurrentVersionString) +
                        String.Format("[[[%0 days left before altering|||{0}]]]", DaysLeftBeforeAltering);

                if (CurrentStatus == ArticleVersionStatus.BeingAltered)
                    return String.Format("[[[Altering current version %0|||{0}]]]", CurrentVersionString);

                throw new ApplicationException("???");
            }
        }

        public string DisplayNewVersionStatus
        {
            get
            {
                var rejectedSuffix =
                    (RejectedCount == 0 ? "" : String.Format(" ([[[rejected %0 times|||{0}]]])", RejectedCount));

                if (NewID == null || NewStatus == ArticleVersionStatus.BeingEdited)
                    return "[[[No published new versions]]]" + rejectedSuffix;

                if (NewStatus == ArticleVersionStatus.BeingReviewed)
                    return String.Format("[[[Reviewing new version %0|||{0}]]] - ", NewVersionString) +
                        String.Format("[[[%0 days left before review ending|||{0}]]]", DaysLeftBeforeReviewEnding) +
                        rejectedSuffix;

                throw new ApplicationException("???");
            }
        }


        public string DisplayNewVersionTechnicalStatus
        {
            get
            {
                var rejectedSuffix =
                    (RejectedCount == 0 ? "" : String.Format(" ([[[rejected %0 times|||{0}]]])", RejectedCount));

                if (NewID == null)
                    return "[[[No new versions]]]" + rejectedSuffix;

                if (NewStatus == ArticleVersionStatus.BeingEdited)
                    return String.Format("[[[Preparing new version %0|||{0}]]]", NewVersionString) + rejectedSuffix;

                if (NewStatus == ArticleVersionStatus.BeingReviewed)
                    return String.Format("[[[Reviewing new version %0|||{0}]]]", NewVersionString) + rejectedSuffix;

                throw new ApplicationException("???");
            }
        }

        #endregion
    }

}