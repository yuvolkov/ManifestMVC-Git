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


namespace DataLayer.ViewModels
{
    public class AlterationVM
    {
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


        //public RateableDM Rateable { get; set; }

        //public UserVM Author { get; set; }

        //public IList<ArticleDM> ParentAlterations { get; set; }
    }

}