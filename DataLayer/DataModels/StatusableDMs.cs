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
    [Table("Statusables")]
    internal class StatusableDM
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
    }


    [Table("StatusChanges")]
    internal class StatusChangeDM
    {
        [Key, Column(Order = 0)]
        public byte Status { get; set; }

        [Key, Column(Order = 1)]
        public int StatusableID { get; set; }

        public int ChangerID { get; set; }

        public DateTime DateChanged { get; set; }

        #region Constructors

        internal StatusChangeDM(byte status, StatusableDM statusableDM, int userId)
        {
            if (statusableDM == null)
                throw new InvalidOperationException();

            Status = status;
            Statusable = statusableDM;
            ChangerID = userId;
            DateChanged = DateTime.Now;
        }

        internal StatusChangeDM(byte status, int statusableId, int userId)
        {
            if (statusableId == 0)
                throw new InvalidOperationException();

            Status = status;
            StatusableID = statusableId;
            ChangerID = userId;
            DateChanged = DateTime.Now;
        }

        #endregion


        [ForeignKey("StatusableID")]
        public StatusableDM Statusable { get; set; }

        [ForeignKey("ChangerID")]
        public UserDM Changer { get; set; }
    }
}