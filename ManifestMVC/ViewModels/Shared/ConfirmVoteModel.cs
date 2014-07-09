using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DataLayer;


namespace ManifestMVC.ViewModels.Shared
{
    public class ConfirmVoteModel
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public bool VotingIsOpen { get; set; }
        public bool AlreadyVoted { get; set; }
        public int ItemId { get; set; }
        public VoteResult Vote { get; set; }
    }
}