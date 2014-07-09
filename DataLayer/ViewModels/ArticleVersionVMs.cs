using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DataLayer;
using DataLayer.DataModels;


namespace DataLayer.ViewModels
{
    public class ArticleVersionVM //ArticleVersionHeaderVM
    {
        public int ID { get; set; }

        public int ArticleID { get; set; }

        public int? SourceVersionID { get; set; }

        public string VersionString { get; set; }

        public ArticleVersionStatus Status { get; set; }

        public ArticleVersionPublicationStatus PublicationStatus { get; set; }

        public DateTime? DateOfReviewEnding { get; set; }

        public DateTime? DateOfAltering { get; set; }



        #region Calculated Properies

        public bool CanEdit
        {
            get
            {
                return
                    Status == ArticleVersionStatus.BeingEdited;      // Article not closed
            }
        }


        public string DisplayVersionString
        {
            get
            {
                return VersionString ?? "[[[New version]]]";
            }
        }


        public int DaysLeftBeforeReviewEnding
        {
            get
            {
                if (Status == ArticleVersionStatus.BeingReviewed)
                    return (int)((DateOfReviewEnding.Value - DateTime.Now).TotalDays+1);
                else
                    return 0;
            }
        }

        public int DaysLeftBeforeAltering
        {
            get
            {
                if (Status == ArticleVersionStatus.Current)
                    return (int)((DateOfAltering.Value - DateTime.Now).TotalDays+1);
                else
                    return 0;
            }
        }


        public string DisplayStatus
        {
            get
            {
                if (Status == ArticleVersionStatus.BeingEdited)
                    return "[[[Preparing new version]]]";

                if (Status == ArticleVersionStatus.BeingReviewed)
                    return
                        "[[[Under review]]] - " +
                        String.Format("[[[%0 days left before review ending|||{0}]]]", DaysLeftBeforeReviewEnding);

                if (Status == ArticleVersionStatus.Current)
                    return
                        "[[[Current]]] - " +
                        String.Format("[[[%0 days left before altering|||{0}]]]", DaysLeftBeforeAltering);

                if (Status == ArticleVersionStatus.BeingAltered)
                    return "[[[Being altered]]]";

                if (Status == ArticleVersionStatus.Rejected)
                    return "[[[Rejected]]]";

                if (Status == ArticleVersionStatus.Archived)
                    return "[[[Archived]]]";

                throw new ApplicationException("???");
            }
        }

        #endregion


        #region Methods

        //protected ArticleVersionHeaderVM()
        //{
        //}

        //public ArticleVersionHeaderVM(
        //    int id, int articleID,
        //    ArticleVersionStatus status, ArticleVersionPublicationStatus publicationStatus,
        //    string versionString)
        //{
        //    ID = id;
        //    ArticleID = articleID;
        //    Status = status;
        //    PublicationStatus = publicationStatus;
        //    VersionString = versionString;
        //}

        #endregion
    //}


    //public class ArticleVersionVM : ArticleVersionHeaderVM
    //{
        [Required(ErrorMessage = "[[[Required]]]")]
        [StringLength(20000, ErrorMessage = "[[[Too long]]]")]
        public string Text { get; set; }

        public string TextWithMarkup { get; set; }

        public string SourceTextWithMarkup { get; set; }



        public ArticleVersionVM() : base()
        {
        }


    }
}