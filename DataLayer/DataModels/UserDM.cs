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
    [Table("Users")]
    internal class UserDM
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]  // this table is from Asp.Net Identity 2.0, beyound our control, so we use their naming covention
        public int ID { get; set; }

        [Required]
        public string UserName { get; set; }

        //public bool IsLoginEnabled { get; set; }

        //public bool IsAdmin { get; set; }

        [Required]
        public string Email { get; set; }

        //[Required, StringLength(68)]
        //public string PasswordHash { get; set; }

        //public DateTime DateRegistered { get; set; }


        ////public static Tuple<UserDM, List<object>> New()
        //public static UserDM New()
        //{
        //    //var createdObjects = new List<object>();

        //    var user = new UserDM();

        //    //createdObjects.Add(user);

        //    //return
        //    //    Tuple.Create(
        //    //        user,
        //    //        createdObjects);
        //    return user;
        //}
    
    }
}