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
    [Table("ArticleGroups")]
    internal class ArticleGroupDM
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int SubdomainID { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        public int SortOrder { get; set; }


        public virtual  ICollection<ArticleDM> Articles { get; set; }

        
        #region Methods

        private ArticleGroupDM()
        {
        }


        public static ArticleGroupDM New(ManifestDBContext context, int subdomainID)
        {
            var gdm = new ArticleGroupDM();
            gdm.SubdomainID = subdomainID;

            gdm.SortOrder = context.ArticleGroups.Max(g => g.SortOrder)+1;

            context.NewObjects.Add(gdm);

            return gdm;
        }


        public void UpdateFrom(ArticleGroupVM gvm)
        {
            Name = gvm.Name;

            if (gvm.SortOrder > 0)          // if new then we in .New it was already set
                SortOrder = gvm.SortOrder;
        }


        public void Delete(ManifestDBContext context)
        {
            // can delete?

            if (context.Articles.Any(a => a.GroupID == ID))
                throw new InvalidOperationException("Cannot delete group with articles (open or closed)");

            context.ArticleGroups.Remove(this);
        }

        #endregion
    }
}