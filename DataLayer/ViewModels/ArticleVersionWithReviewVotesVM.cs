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
    public class ArticleVersionWithReviewVotesVM
    {
        public int ID { get; set; }

        public int ArticleID { get; set; }

        public string VersionString { get; set; }

        public ArticleVersionStatus Status { get; set; }

        public DateTime? DateOfReviewEnding { get; set; }

        public int TotalAcceptVotes { get; set; }

        public int TotalRejectVotes { get; set; }



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

        public bool IsRejected
        {
            get
            {
                return
                    TotalAcceptVotes < ParametersRepo.Instance.MathReviewYesQuorum ||
                    Consensus < ParametersRepo.Instance.MathReviewConsensus;
            }
        }

        public bool IsAccepted
        {
            get
            {
                return
                    TotalAcceptVotes >= ParametersRepo.Instance.MathReviewYesQuorum &&
                    Consensus >= ParametersRepo.Instance.MathReviewConsensus;
            }
        }


        public int DaysLeftBeforeReviewEnding
        {
            get
            {
                if (Status == ArticleVersionStatus.BeingReviewed)
                    return (int)((DateOfReviewEnding.Value - DateTime.Now).TotalDays+1);
                else
                    return 0;
            }
        }

        #endregion
    }
}