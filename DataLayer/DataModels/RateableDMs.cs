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
    [Table("Rateables")]
    internal class RateableDM
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        
        public RateableStatus Status { get; set; }

        public int StatusableID { get; set; }


        [ForeignKey("StatusableID")]
        public virtual StatusableDM Statusable { get; set; }



        public virtual ICollection<VoteDM> Votes { get; set; }

        

        #region Methods

        protected RateableDM()
        {
        }


        public static RateableDM New(ManifestDBContext context, int userId)
        {
            var rdm = new RateableDM();
            context.NewObjects.Add(rdm);

            rdm.Status = RateableStatus.Active;
            rdm.Statusable = new StatusableDM();
            context.NewObjects.Add(rdm.Statusable);
            context.NewObjects.Add(
                new StatusChangeDM((byte)rdm.Status, rdm.Statusable, userId));

            return rdm;
        }


        public void Close(ManifestDBContext context, int userId)
        {
            if (Status != RateableStatus.Closed)
            {
                Status = RateableStatus.Closed;
                context.NewObjects.Add(
                    new StatusChangeDM((byte)Status, StatusableID, userId));
            }
        }

        #endregion
    }


    [Table("Votes")]
    internal class VoteDM
    {
        [Key, ForeignKey("Rateable"), Column(Order = 0)]
        public int RateableID { get; set; }

        [Key, ForeignKey("Voter"), Column(Order = 1)]
        public int VoterID { get; set; }

        public VoteResult Result { get; set; }


        [ForeignKey("RateableID")]
        public RateableDM Rateable { get; set; }

        [ForeignKey("VoterID")]
        public UserDM Voter { get; set; }



        #region Methods

        protected VoteDM()
        {
        }

        public static VoteDM NewWithChecks(ManifestDBContext context, RateableDM rateableDM, int voterId, VoteResult result)
        {
            //var rateableDM = context.Rateables.Find(rateableId);
            var voter = context.Users.Find(voterId);

            if (rateableDM.Status != RateableStatus.Active)   // || !voter.IsLoginEnabled)  - voting should be fast, minimizing
                throw new InvalidOperationException();

            var vdm = new VoteDM();
            context.NewObjects.Add(vdm);

            vdm.RateableID = rateableDM.ID;
            vdm.VoterID = voterId;
            vdm.Result = result;

            return vdm;
        }

        public static IEnumerable<VoteDM> NewVirtualVotesWithChecks(ManifestDBContext context, RateableDM rateableDM, uint numberOfVotes, VoteResult result)
        {
            List<VoteDM> virtualVotes = new List<VoteDM>();

            var namePrefix = "VirtualUser";

            var usersToVote =
                (from u in context.Users
                 let voteCount = context.Votes.Where(v => v.RateableID == rateableDM.ID &&
                                                          v.VoterID == u.ID)
                                               .Count()
                 where u.UserName.StartsWith(namePrefix) && voteCount == 0
                 select u)
                .Take((int)numberOfVotes)
                .ToList();

            foreach (var user in usersToVote)
            {
                var vote = 
                    NewWithChecks(
                        context,
                        rateableDM,
                        user.ID,
                        result);
                virtualVotes.Add(vote);
            }

            return virtualVotes;
        }

        #endregion
    }
}