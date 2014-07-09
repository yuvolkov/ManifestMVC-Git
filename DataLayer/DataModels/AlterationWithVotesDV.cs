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
    [Table("vAlterationsWithVotes")]    // we use TVF for this entity, but EF also joins the view
    internal class AlterationWithVotesDV : BaseAlterationDM
    {
        public string AuthorName { get; set; }


        public RateableStatus RateableStatus { get; protected set; }

        public int TotalAcceptVotes { get; protected set; }

        public int TotalRejectVotes { get; protected set; }



        public int VoterID { get; protected set; }

        public VoteResult? VoteResult { get; protected set; }

        [ForeignKey("VoterID")]
        public UserDM Voter { get; protected set; }
    }
}