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
    public class ArticleGroupVM
    {
        public int ID { get; set; }

        public int SubdomainID { get; set; }

        [Display(Name = "[[[Name]]]")]
        [Required(ErrorMessage = "[[[Required]]]")]
        [StringLength(100, ErrorMessage = "[[[Too long]]]")]
        public string Name { get; set; }

        public int SortOrder { get; set; }
    }
}