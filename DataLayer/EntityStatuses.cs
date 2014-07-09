using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DataLayer
{
    public enum ArticleStatus : byte
    {
        Current = 0,
        Closed = 1
    }

    public enum ArticleVersionStatus : byte
    {
        BeingEdited = 0,       // Not proposed
        BeingReviewed = 1,     // Proposed
        Rejected = 2,          // Rejected, Closed
        Current = 3,           // Current
        BeingAltered = 4,      // Current, Closed
        Archived = 5           // Closed, Closed
    }

    public enum ArticleVersionPublicationStatus : byte
    {
        NotPublished = 0,
        Published = 1,
        Archived = 2
    }

    public enum AlterationStatus : byte
    {
        Active = 0,
        Accepted = 1
    }

    public enum RateableStatus : byte
    {
        Active,
        Closed
    }

    public enum VoteResult : byte
    {
        Accept = 0,
        Reject = 1,
        FlagInapropriate = 2
    }
}
