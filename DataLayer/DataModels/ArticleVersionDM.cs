using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DataLayer;
using DataLayer.Repositories;
using DataLayer.Repositories.Settings;
using DataLayer.ViewModels;


namespace DataLayer.DataModels
{
    [Table("ArticleVersions")]
    internal class ArticleVersionDM
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int ArticleID { get; set; }

        public int? SourceVersionID { get; set; }

        public int ReviewRateableID { get; set; }

        public int PublicationRateableID { get; set; }

        public ArticleVersionStatus Status { get; set; }

        public int StatusableID { get; set; }

        public ArticleVersionPublicationStatus PublicationStatus { get; set; }

        public int PublicationStatusableID { get; set; }

        [Required, StringLength(10)]
        public string VersionString { get; set; }

        [Required, StringLength(20000)]
        public string Text { get; set; }

        [Required, StringLength(40000)]
        public string TextWithMarkup { get; set; }

        [StringLength(40000)]
        public string SourceTextWithMarkup { get; set; }

        public DateTime? DateOfReviewEnding { get; set; }

        public DateTime? DateOfAltering { get; set; }


        [ForeignKey("ArticleID")]
        public ArticleDM Article { get; set; }

        [ForeignKey("SourceVersionID")]
        public ArticleVersionDM SourceVersion { get; set; }

        [ForeignKey("ReviewRateableID")]
        public RateableDM ReviewRateable { get; set; }

        [ForeignKey("PublicationRateableID")]
        public RateableDM PublicationRateable { get; set; }

        [ForeignKey("StatusableID")]
        public StatusableDM Statusable { get; set; }

        [ForeignKey("PublicationStatusableID")]
        public StatusableDM PublicationStatusable { get; set; }


        #region Methods

        public ArticleVersionDM()
        {
        }


        public static ArticleVersionDM New(ManifestDBContext context, int articleID, int? sourceVersionID, string versionString, int userId)
        {
            var avdm = new ArticleVersionDM();
            context.NewObjects.Add(avdm);

            avdm.Status = ArticleVersionStatus.BeingEdited;
            avdm.Statusable = new StatusableDM();
            context.NewObjects.Add(avdm.Statusable);
            context.NewObjects.Add(
                    new StatusChangeDM((byte)avdm.Status, avdm.Statusable, userId));

            avdm.PublicationStatus = ArticleVersionPublicationStatus.NotPublished;
            avdm.PublicationStatusable = new StatusableDM();
            context.NewObjects.Add(avdm.PublicationStatusable);
            context.NewObjects.Add(
                    new StatusChangeDM((byte)avdm.PublicationStatus, avdm.PublicationStatusable, userId));

            avdm.ReviewRateable = RateableDM.New(context, userId);
            avdm.PublicationRateable = RateableDM.New(context, userId);

            avdm.ArticleID = articleID;
            avdm.SourceVersionID = sourceVersionID;
            avdm.VersionString = versionString;

            return avdm;
        }


        public void UpdateFrom(ArticleVersionVM avm, ArticleVersionDM sourceVersion)
        {
            if (Status != ArticleVersionStatus.BeingEdited)
                throw new InvalidOperationException("Should be BeingEdited!"); // not found!

            Text = avm.Text;

            if (sourceVersion != null)
                SourceTextWithMarkup =
                    "<span class='delete'>" + 
                    sourceVersion.Text + 
                    "</span>";

            TextWithMarkup =
                "<span class='add'>" + 
                Text + 
                "</span>";
        }


        protected void _ChangeStatusTo(ManifestDBContext context, ArticleVersionStatus newStatus, int userId)
        {
            var allowedChanges = new List<Tuple<ArticleVersionStatus, ArticleVersionStatus>>()
            {
                Tuple.Create(ArticleVersionStatus.BeingEdited, ArticleVersionStatus.Current),   // only as 1st version!
                Tuple.Create(ArticleVersionStatus.BeingEdited, ArticleVersionStatus.BeingReviewed),
                Tuple.Create(ArticleVersionStatus.BeingReviewed, ArticleVersionStatus.Rejected),
                Tuple.Create(ArticleVersionStatus.BeingReviewed, ArticleVersionStatus.Current),
                Tuple.Create(ArticleVersionStatus.Current, ArticleVersionStatus.BeingAltered),
                Tuple.Create(ArticleVersionStatus.BeingAltered, ArticleVersionStatus.Archived)
            };

            var change =
                (from ch in allowedChanges
                 where ch.Item1 == Status && ch.Item2 == newStatus
                 select ch
                ).SingleOrDefault();

            if (change == null)
                throw new NotImplementedException();

            if (change.Equals(Tuple.Create(ArticleVersionStatus.BeingEdited, ArticleVersionStatus.Current)) &&
                VersionString != "1.0")
                throw new NotImplementedException();

            Status = newStatus;

            context.NewObjects.Add(
                new StatusChangeDM((byte)Status, StatusableID, userId));
        }


        protected void _CloseAlterations(ManifestDBContext context, int userId)
        {
            var alts = context.Alterations
                .Where(alt => alt.ArticleVersionID == ID)
                .Include(alt => alt.Rateable)
                .ToList();

            foreach (var alt in alts.Where(alt => alt.Rateable.Status != RateableStatus.Closed))
            {
                alt.Rateable.Status = RateableStatus.Closed;
                context.NewObjects.Add(
                    new StatusChangeDM((byte)alt.Rateable.Status, alt.Rateable.StatusableID, userId));
            }
        }


        public void ZeroActiveDateCounters()
        {
            if (Status == ArticleVersionStatus.BeingReviewed &&
                DateOfReviewEnding.Value > DateTime.Now)
            {
                DateOfReviewEnding = DateTime.Now.AddMinutes(-1);
            }
            else if (Status == ArticleVersionStatus.Current &&
                     DateOfAltering.Value > DateTime.Now)
            {
                DateOfAltering = DateTime.Now.AddMinutes(-1);
            }
            else
                throw new InvalidOperationException();
        }


        public void SetActiveDateCounters()
        {
            if (Status == ArticleVersionStatus.BeingReviewed)
            {
                DateOfReviewEnding = DateTime.Now.AddDays(RequestParameters.SubdomainParams.DaysBeforeReviewEnding);
            }
            else if (Status == ArticleVersionStatus.Current)
            {
                DateOfAltering = DateTime.Now.AddDays(RequestParameters.SubdomainParams.DaysBeforeAltering);
            }
            else
                throw new InvalidOperationException();
        }


        public void BeginReviewing(ManifestDBContext context, int userId)
        {
            if (Status != ArticleVersionStatus.BeingEdited)
                throw new InvalidOperationException();

            _ChangeStatusTo(context, ArticleVersionStatus.BeingReviewed, userId);

            DateOfReviewEnding = DateTime.Now.AddDays(RequestParameters.SubdomainParams.DaysBeforeReviewEnding);
        }


        public void RejectReviewed(ManifestDBContext context, int userId)
        {
            if (Status != ArticleVersionStatus.BeingReviewed ||
                DateOfReviewEnding.Value > DateTime.Now)
                throw new InvalidOperationException();

            _ChangeStatusTo(context, ArticleVersionStatus.Rejected, userId);

            ReviewRateable.Close(context, userId);
        }


        public void MakeCurrent(ManifestDBContext context, int userId)
        {
            // enviroment-ok

            if (!(Status == ArticleVersionStatus.BeingReviewed && DateOfReviewEnding.Value <= DateTime.Now ||   // either was reviewed
                  Status == ArticleVersionStatus.BeingEdited))                                                  // or was edited (as first version)
                throw new InvalidOperationException();

            _ChangeStatusTo(context, ArticleVersionStatus.Current, userId);

            ReviewRateable.Close(context, userId);

            DateOfAltering = DateTime.Now.AddDays(RequestParameters.SubdomainParams.DaysBeforeAltering);
        }


        public void BeginAltering(ManifestDBContext context, int userId)
        {
            if (Status != ArticleVersionStatus.Current ||
                DateOfAltering.Value > DateTime.Now)
                throw new InvalidOperationException();

            Status = ArticleVersionStatus.BeingAltered;

            // alterations

            _CloseAlterations(context, userId);
        }


        public void Archive(ManifestDBContext context, int userId)
        {
            // rules: optimistic, throws, simplistic, enviroment-ok, down-the-chain-delegating, no-db-optimizing

            if (Status == ArticleVersionStatus.Rejected || Status == ArticleVersionStatus.Archived) // already terminal states
                throw new InvalidOperationException();

            // forcibly archive
            Status = ArticleVersionStatus.Archived;

            ReviewRateable.Close(context, userId);
            PublicationRateable.Close(context, userId);

            // alterations

            _CloseAlterations(context, userId);
        }
        
        #endregion
    }
}