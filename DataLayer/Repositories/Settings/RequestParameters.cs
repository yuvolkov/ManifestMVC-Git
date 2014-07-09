using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using WebSafeLibrary;


namespace DataLayer.Repositories.Settings
{
    public class RequestParameters
    {
        private static string _languageKey = "Language";
        private static string _subdomainNameKey = "SubdomainName";
        //private static string _subdomainIdKey = "SubdomainID";
        //private static string _subdomainParamsKey = "SubdomainParams";

        public static string Language
        {
            get { return (string)WebSafeCallContext.Get(_languageKey); }
            set { WebSafeCallContext.Set(_languageKey, value); }
        }
        public static string SubdomainName
        {
            get { return (string)WebSafeCallContext.Get(_subdomainNameKey); }
            set { WebSafeCallContext.Set(_subdomainNameKey, value); }
        }

        //public static int SubdomainID
        //{
        //    //get { return (int) WebSafeCallContext.Get(_subdomainIdKey); }
        //    //set { WebSafeCallContext.Set(_subdomainIdKey, value); }
        //}
        //public static SubdomainParameters SubdomainParams
        //{
        //    get { return (SubdomainParameters) WebSafeCallContext.Get(_subdomainParamsKey); }
        //    set { WebSafeCallContext.Set(_subdomainParamsKey, value); }
        //}

        public static int SubdomainID
        {
            get { return ParametersRepo.Instance.SubdomainsIds[SubdomainName]; }
        }
        public static SubdomainParameters SubdomainParams
        {
            get { return ParametersRepo.Instance.SubdomainsParameters[SubdomainName]; }
        }

    }
}
