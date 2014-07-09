using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

using CodeFirstStoreFunctions;

using DataLayer.DataModels;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;


namespace DataLayer
{
    internal class ManifestDBContext : DbContext
    {
        public event Action OnCommit;

        public ManifestDBContext()
        {
            Database.SetInitializer<ManifestDBContext>(null);

            NewObjects = new List<object>();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(new FunctionsConvention<ManifestDBContext>("dbo"));
        } 

        #region Tables

        public DbSet<ArticleGroupDM> ArticleGroups { get; set; }
        public DbSet<ArticleDM> Articles { get; set; }
        public DbSet<ArticleVersionDM> ArticleVersions { get; set; }
        public DbSet<AlterationDM> Alterations { get; set; }
        public DbSet<RateableDM> Rateables { get; set; }
        public DbSet<VoteDM> Votes { get; set; }
        public DbSet<UserDM> Users { get; set; }
        public DbSet<StatusableDM> Statusables { get; set; }
        public DbSet<StatusChangeDM> StatusChanges { get; set; }

        public DbSet<ParametersDM> Parameters { get; set; }

        #endregion

        #region Views

        /// <summary>
        /// never used, only to use TVF
        /// </summary>
        public DbSet<AlterationWithVotesDV> AlterationsWithVotes { get; set; }

        public DbSet<ArticleCurrentSummaryDV> ArticleCurrentSummaries { get; set; }

        /// <summary>
        /// never used, only to use TVF
        /// </summary>
        public DbSet<ArticleCurrentSummaryWithVotesDV> ArticleCurrentSummariesWithVotes { get; set; }

        #endregion

        #region TableValuedFunctions

        [DbFunction("ManifestDBContext", "fAlterationsWithVotes")]
        public IQueryable<AlterationWithVotesDV> fAlterationsWithVotes(int voterID)
        {
            ////var voterIdParameter = voterID != null ?
            //var voterIdParameter = voterID != 0 ?
            //    new ObjectParameter("voterID", voterID) :
            //    new ObjectParameter("voterID", typeof(string));

            var voterIdParameter = new ObjectParameter("voterID", voterID);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<AlterationWithVotesDV>(
                    string.Format("[{0}].{1}", GetType().Name,
                        "[fAlterationsWithVotes](@voterID)"), voterIdParameter);
        }


        [DbFunction("ManifestDBContext", "fArticleCurrentSummariesWithVotes")]
        public IQueryable<ArticleCurrentSummaryWithVotesDV> fArticleCurrentSummariesWithVotes(int voterID)
        {
            ////var voterIdParameter = voterID != null ?
            //var voterIdParameter = voterID != 0 ?
            //    new ObjectParameter("voterID", voterID) :
            //    new ObjectParameter("voterID", typeof(string));

            var voterIdParameter = new ObjectParameter("voterID", voterID);

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<ArticleCurrentSummaryWithVotesDV>(
                    string.Format("[{0}].{1}", GetType().Name,
                        "[fArticleCurrentSummariesWithVotes](@voterID)"), voterIdParameter);
        }

        #endregion


        public List<object> NewObjects;

        
        public static Dictionary<Type, Tuple<PropertyInfo, MethodInfo>> DbSetsReflection;


        public override int SaveChanges()
        {
            if (NewObjects.Any())
                _AddObjectsToSets(NewObjects);

            int count = base.SaveChanges();

            if (OnCommit != null)
                OnCommit();

            return count;
        }


        public static void Reflect()
        {
            DbSetsReflection = new Dictionary<Type,Tuple<PropertyInfo,MethodInfo>>();

            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            Type contextType = typeof(ManifestDBContext);
            PropertyInfo[] properties = contextType.GetProperties(flags);

            foreach (var prop in properties)
            {
                var type = prop.PropertyType;
                if (type.Name == "DbSet`1")
                {
                    var typeArg = type.GetGenericArguments()[0];
                    
                    DbSetsReflection[typeArg] = Tuple.Create( prop, type.GetMethod("Add") );
                }
            }
        }

        protected void _AddObjectsToSets(List<object> createdObjects)
        {
            foreach(var obj in createdObjects)
            {
                Type type = obj.GetType();

                var tupleInfo = DbSetsReflection[type];

                var propInfo = tupleInfo.Item1;
                var methInfo = tupleInfo.Item2;

                object dbSet = propInfo.GetValue(this, null);

                methInfo.Invoke(dbSet, new[] { obj });
            }
        }
    }
}
