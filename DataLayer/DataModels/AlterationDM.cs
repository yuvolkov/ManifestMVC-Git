using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DataLayer;
using DataLayer.Repositories;
using DataLayer.ViewModels;


namespace DataLayer.DataModels
{
    internal class BaseAlterationDM
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int ArticleVersionID { get; set; }

        public AlterationStatus Status { get; set; }

        public int StatusableID { get; set; }

        public int RateableID { get; set; }

        public int AuthorID { get; set; }

        [Required, StringLength(2000)]
        public string Text { get; set; }

        [Required, StringLength(2000)]
        public string Justification { get; set; }


        [ForeignKey("StatusableID")]
        public StatusableDM Statusable { get; set; }

        [ForeignKey("RateableID")]
        public RateableDM Rateable { get; set; }

        [ForeignKey("AuthorID")]
        public UserDM Author { get; set; }

        [ForeignKey("ArticleVersionID")]
        public ArticleVersionDM Version { get; set; }

        //public IList<ArticleDM> ParentAlterations { get; set; }
    }



    [Table("Alterations")]
    internal class AlterationDM : BaseAlterationDM
    {
        #region Methods

        protected AlterationDM()
        {
        }


        public static AlterationDM New(ManifestDBContext context, int versionID, int authorID)
        {
            var alt = new AlterationDM();
            context.NewObjects.Add(alt);

            alt.ArticleVersionID = versionID;
            alt.AuthorID = authorID;

            alt.Status = AlterationStatus.Active;
            alt.Statusable = new StatusableDM();
            context.NewObjects.Add(alt.Statusable);
            context.NewObjects.Add(
                new StatusChangeDM((byte)alt.Status, alt.Statusable, authorID));

            alt.Rateable = RateableDM.New(context, authorID);

            return alt;
        }


        protected void _ChangeStatusTo(ManifestDBContext context, AlterationStatus newStatus, int userId)
        {
            var allowedChanges = new List<Tuple<AlterationStatus, AlterationStatus>>()
            {
                Tuple.Create(AlterationStatus.Active, AlterationStatus.Accepted)
            };

            var change =
                (from ch in allowedChanges
                 where ch.Item1 == Status && ch.Item2 == newStatus
                 select ch
                ).SingleOrDefault();

            if (change == null)
                throw new NotImplementedException();

            Status = newStatus;

            context.NewObjects.Add(
                new StatusChangeDM((byte)Status, StatusableID, userId));
        }


        public void SetActive(ManifestDBContext context, int userId)
        {
            if (Status != AlterationStatus.Active)
                throw new InvalidOperationException();

            _ChangeStatusTo(context, AlterationStatus.Accepted, userId);
        }


        #endregion
    }

}