using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using DataLayer;
using DataLayer.Repositories;
using DataLayer.Repositories.Settings;
using DataLayer.ViewModels;


namespace DataLayer.ViewModels
{
    public class AlterationWithVotesVM : AlterationVM, IComparable<AlterationWithVotesVM>
    {
        public string AuthorName { get; set; }

        public RateableStatus RateableStatus { get; set; }

        public int TotalAcceptVotes { get; set; }

        public int TotalRejectVotes { get; set; }

        public VoteResult? VoteResult { get; set; }


        #region Calculated Properies

        public float Consensus
        {
            get
            {
                if (TotalAcceptVotes + TotalRejectVotes == 0)
                    return 0;

                return (float)TotalAcceptVotes / (TotalAcceptVotes + TotalRejectVotes);
            }
        }

        //public float Quorum
        //{
        //    get
        //    {
        //        return TotalAcceptVotes;
        //    }
        //}


        public bool IsSandbox
        {
            get
            {
                return (TotalAcceptVotes + TotalRejectVotes <= RequestParameters.SubdomainParams.MathAlterationSandboxYesNoQuorum);
            }
        }

        public bool IsRejected
        {
            get
            {
                return
                    !IsSandbox &&
                    Consensus <= RequestParameters.SubdomainParams.MathAlterationRejectConsensus;
            }
        }

        public bool IsAccepted
        {
            get
            {
                return
                    TotalAcceptVotes >= RequestParameters.SubdomainParams.MathAlterationAcceptYesQuorum &&
                    Consensus >= RequestParameters.SubdomainParams.MathAlterationAcceptConsensus;
            }
        }

        public double SyntheticOrder
        {
            get
            {
                return
                    TotalAcceptVotes / RequestParameters.SubdomainParams.MathAlterationAcceptYesQuorum +
                    (Consensus - RequestParameters.SubdomainParams.MathAlterationRejectConsensus) / (RequestParameters.SubdomainParams.MathAlterationAcceptConsensus - RequestParameters.SubdomainParams.MathAlterationRejectConsensus);
            }
        }

        public int GroupOrder
        {
            get
            {
                if (IsRejected)
                    return 0;
                if (IsSandbox)
                    return 1;
                if (IsAccepted)
                    return 3;
                return 2;
            }
        }

        #endregion


        public int CompareTo(AlterationWithVotesVM other)
        {
            if (this.GroupOrder != other.GroupOrder)
                return (this.GroupOrder - other.GroupOrder);
            else
                return Math.Sign(this.SyntheticOrder - other.SyntheticOrder);
        }

    }

}