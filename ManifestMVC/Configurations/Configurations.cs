using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ManifestMVC.Configurations
{
    public class LanguageSection : ConfigurationSection
    {
        private static ConfigurationProperty _languages;
        private static ConfigurationProperty _defaultLanguage;

        private static ConfigurationPropertyCollection _properties;

        [ConfigurationProperty("languages", IsRequired = true)]
        public KeyValueConfigurationCollection Languages
        {
            get { return (KeyValueConfigurationCollection)base[_languages]; }
        }

        [ConfigurationProperty("defaultLanguage", IsRequired = true)]
        public string DefaultLanguage
        {
            get { return (string)base[_defaultLanguage]; }
        }


        /// <summary>
        /// Predefines the valid properties and prepares
        /// the property collection.
        /// </summary>
        static LanguageSection()
        {
            _languages = new ConfigurationProperty(
                "languages",
                typeof(KeyValueConfigurationCollection),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

            _defaultLanguage = new ConfigurationProperty(
                "defaultLanguage",
                typeof(string),
                null,
                ConfigurationPropertyOptions.IsRequired
            );

            _properties = new ConfigurationPropertyCollection();
            
            _properties.Add(_languages);
            _properties.Add(_defaultLanguage);
        }

         
        /// <summary>
        /// Override the Properties collection and return our custom one.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }
    }
}