using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Entity;
using System.Reflection;
using System.Diagnostics;

using Omu.ValueInjecter;

using DataLayer.DataModels;
using DataLayer.ViewModels;


namespace DataLayer.Repositories
{
    public class AlterationsRepo : UOWRepo<AlterationsRepo>
    {
        public AlterationsRepo()
            : base()
        {
        }

        internal AlterationsRepo(ManifestDBContext context)
            : base(context)
        {
        }



        public AlterationVM FindAlteration(int id)
        {
            var alt =
                (from v in _context.Alterations
                 where v.ID == id
                 select v)
                .ToList()
                .Select(a => (new AlterationVM()).InjectFrom(a) as AlterationVM)
                .SingleOrDefault();

            if (alt == null)
                return null;

            return alt;
        }



        public IEnumerable<AlterationVM> GetAlterations(int versionId)
        {
            var alts =
                (from a in _context.Alterations
                 where a.ArticleVersionID == versionId
                 select a)
                .ToList()
                .Select(a => (new AlterationVM()).InjectFrom(a) as AlterationVM);

            return alts;
        }



        public IEnumerable<AlterationWithVotesVM> GetAlterationsWithVotes(
            int versionId, int? voterId, bool acceptedOnly = false)
        {
            IQueryable<AlterationWithVotesDV> query;

            query = (from a in _context.fAlterationsWithVotes(voterId ?? -1)
                        where a.ArticleVersionID == versionId
                        select a);

            if (acceptedOnly)
                query = query.Where(a => a.Status == AlterationStatus.Accepted);

            var altsWithVotes = 
                query
                .ToList()
                .Select(a => (new AlterationWithVotesVM()).InjectFrom(a) as AlterationWithVotesVM);

            return altsWithVotes;
        }



        public void UpdateOrCreateAlteration(AlterationVM avm, int userId)
        {
            // retrieving version
            ArticleVersionDM versionDM = _context.ArticleVersions.Find(avm.ArticleVersionID);

            ArticleDM articleDM = _context.Articles.Find(versionDM.ArticleID);

            if (articleDM.Status != ArticleStatus.Current ||
                versionDM.Status != ArticleVersionStatus.Current)
                throw new InvalidProgramException();

            if (avm.ID == 0)
            {
                var adm = AlterationDM.New(_context, avm.ArticleVersionID, userId);

                adm.Text = avm.Text;
                adm.Justification = avm.Justification;

                // retrieving Id from DB
                _context.OnCommit += () => { avm.ID = adm.ID; };
            }
            else
            {
                throw new NotImplementedException();
            }


            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }



        public void AddVote(
            int alterationId, int voterId, VoteResult voteResult)
        {
            var alteration = 
                _context.Alterations
                    .Include(alt => alt.Rateable)
                    .Include(alt => alt.Version)
                    .Single(alt => alt.ID == alterationId);

            // checks

            if (alteration.Status != AlterationStatus.Active ||
                alteration.Version.Status != ArticleVersionStatus.Current)
                throw new InvalidOperationException();

            VoteDM vote = VoteDM.NewWithChecks(
                _context, 
                alteration.Rateable, 
                voterId, 
                voteResult);


            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }



        [Conditional("DEBUG")]
        public void AddVirtualVotes(
            int alterationId, uint numberOfVotes, VoteResult voteResult)
        {
            var alteration =
                _context.Alterations
                    .Include(alt => alt.Rateable)
                    .Include(alt => alt.Version)
                    .Single(alt => alt.ID == alterationId);

            // checks

            if (alteration.Status != AlterationStatus.Active ||
                alteration.Version.Status != ArticleVersionStatus.Current)
                throw new InvalidOperationException();

            VoteDM.NewVirtualVotesWithChecks(
                _context,
                alteration.Rateable,
                numberOfVotes,
                voteResult);


            if (AutocommitOnSuccess)
                _context.SaveChanges();
        }

    }
}
