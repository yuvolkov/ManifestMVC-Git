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
    [Table("Articles")]
    internal class ArticleDM
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int GroupID { get; set; }

        public ArticleStatus Status { get; set; }

        public int StatusableID { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        public int SortOrder { get; set; }


        [ForeignKey("GroupID")]
        public ArticleGroupDM Group { get; set; }

        [ForeignKey("StatusableID")]
        public StatusableDM Statusable { get; set; }

        //public IList<ArticleDM> ParentArticles { get; set; }



        public virtual ICollection<ArticleVersionDM> ArticleVersions { get; set; }

        
        #region Methods

        private ArticleDM()
        {
        }


        public static ArticleDM New(ManifestDBContext context, int userId)
        {
            var adm = new ArticleDM();
            adm.Status = ArticleStatus.Current;
            adm.Statusable = new StatusableDM();    // new one, ID is zero

            adm.SortOrder = context.Articles.Max(a => a.SortOrder) + 1;

            context.NewObjects.Add(adm);
            context.NewObjects.Add(adm.Statusable);
            context.NewObjects.Add(
                new StatusChangeDM((byte)adm.Status, adm.Statusable, userId));

            return adm;
        }


        public void UpdateFrom(ArticleVM avm)
        {
            if (Status != ArticleStatus.Current)
                throw new InvalidOperationException("Should be Current!"); // not found!

            Title = avm.Title;
            Description = avm.Description;
            GroupID = avm.GroupID;

            if (avm.SortOrder > 0)          // if new then we in .New it was already set
                SortOrder = avm.SortOrder;
        }


        /// <summary>
        /// Can move both Current and Closed Articles
        /// </summary>
        /// <param name="newGroupID"></param>
        public void MoveToGroup(int newGroupID)
        {
            GroupID = newGroupID;
        }


        protected void _ChangeStatusTo(ManifestDBContext context, ArticleStatus newStatus, int userId)
        {
            var allowedChanges = new List<Tuple<ArticleStatus, ArticleStatus>>()
            {
                Tuple.Create(ArticleStatus.Current, ArticleStatus.Closed)
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


        public void Close(ManifestDBContext context, int userId)
        {
            // rules: optimistic, throws, simplistic, enviroment-ok, down-the-chain-delegating, no-db-optimizing

            _ChangeStatusTo(context, ArticleStatus.Closed, userId);

            var summary = context
                .ArticleCurrentSummaries
                .Include(s => s.CurrentVersion)
                    .Include(s => s.CurrentVersion.ReviewRateable)
                    .Include(s => s.CurrentVersion.PublicationRateable)
                .Include(s => s.NewVersion)
                    .Include(s => s.NewVersion.ReviewRateable)
                    .Include(s => s.NewVersion.PublicationRateable)
                .First(s => s.ArticleID == ID);

            // closing current & new    

            if (summary.CurrentVersion != null)
                summary.CurrentVersion.Archive(context, userId);

            if (summary.NewVersion != null)
                summary.NewVersion.Archive(context, userId);
        }

        #endregion
    }
}