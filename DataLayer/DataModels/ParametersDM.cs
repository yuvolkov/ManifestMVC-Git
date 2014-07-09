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
    [Table("Parameters")]
    internal class ParametersDM
    {
        [Key]
        public string Key { get; set; }

        public string Value { get; set; }
    }
}