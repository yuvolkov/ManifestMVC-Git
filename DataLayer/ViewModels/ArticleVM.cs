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
    public class ArticleVM
    {
        public int ID { get; set; }

        [Display(Name = "[[[Group]]]")]
        public int GroupID { get; set; }

        [Display(Name = "[[[Title]]]")]
        [Required(ErrorMessage = "[[[Required]]]")]
        [StringLength(100, ErrorMessage = "[[[Too long]]]")]
        public string Title { get; set; }

        [Display(Name = "[[[Description]]]")]
        [StringLength(1000, ErrorMessage = "[[[Description too Long!]]]")]
        public string Description { get; set; }

        public ArticleStatus Status { get; set; }

        public int SortOrder { get; set; }
    }
}