using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml.Linq;

using Omu.ValueInjecter;

using DataLayer.DataModels;
using DataLayer.Helpers.ValueInjecter;


namespace DataLayer.Repositories.Settings
{
    public class ConfigRepo
    {
        public static ConfigRepo Instance { get; private set; }

        public string Host { get; protected set; }

        public int SystemUserID { get; protected set; }


        private ConfigRepo(string configPath)
        {
            XDocument xdoc = XDocument.Load(configPath + "/Config.xml");

            this.InjectFrom<XElementToObjectStrict>(xdoc.Root);
        }

        public static void Initialize(string configPath)
        {
            if (Instance != null)
                throw new InvalidOperationException();

            Instance = new ConfigRepo(configPath);
        }
    }
}
