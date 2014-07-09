using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Omu.ValueInjecter;

using DataLayer.Helpers.ValueInjecter;


namespace DataLayer.Repositories.Settings
{
    public class SubdomainParameters
    {
        public int DaysBeforeAltering { get; protected set; }

        public int DaysBeforeReviewEnding { get; protected set; }

        public int MathAlterationSandboxYesNoQuorum { get; protected set; }
        //public int MathSandboxTotalQuorum { get; protected set; }

        public int MathAlterationAcceptYesQuorum { get; protected set; }

        public double MathAlterationRejectConsensus { get; protected set; }

        public double MathAlterationAcceptConsensus { get; protected set; }

        public int MathReviewSandboxYesNoQuorum { get; protected set; }

        public int MathReviewAcceptYesQuorum { get; protected set; }

        public double MathReviewRejectConsensus { get; protected set; }

        public double MathReviewAcceptConsensus { get; protected set; }


        public SubdomainParameters(IEnumerable<KeyValuePair<string,string>> kvps)
        {
            this.InjectFrom<NestedKeyValuesToObjectStrict>(kvps);
        }
    }


    public class ParametersRepo
    {
        public static ParametersRepo Instance { get; private set; }


        public Dictionary<string, int> SubdomainsIds;

        public Dictionary<string, string> SubdomainsTitles;

        public Dictionary<string, SubdomainParameters> SubdomainsParameters;


        protected class SubdomainEntry
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Title { get; set; }
        }


        private ParametersRepo()
        {
            using (var context = new ManifestDBContext())
            {
                var subdomainsInfo = context.Database.SqlQuery<SubdomainEntry>("SELECT ID, Name, Title FROM Subdomains").ToList();
                SubdomainsIds = subdomainsInfo.ToDictionary(x => x.Name, x => x.ID);
                SubdomainsTitles = subdomainsInfo.ToDictionary(x => x.Name, x => x.Title);

                var kvps = context.Parameters.ToList().Select(p => new KeyValuePair<string, string>(p.Key, p.Value));

                SubdomainsParameters =
                    (from grp in 
                        (from kvp in kvps
                         let first_dot_pos = kvp.Key.IndexOf('.')
                         let subdomain = kvp.Key.Substring(0, first_dot_pos)
                         let rest_of_key = kvp.Key.Substring(first_dot_pos+1)
                         group new KeyValuePair<string,string>(rest_of_key, kvp.Value) by subdomain)
                     let subdomainParams = new SubdomainParameters(grp)
                     select new {Subdomain = grp.Key, Params = subdomainParams})
                    .ToDictionary(x => x.Subdomain, x => x.Params);
            }
        }

        public static void Initialize()
        {
            if (Instance != null)
                throw new InvalidOperationException();

            Instance = new ParametersRepo();
        }
    }
}
