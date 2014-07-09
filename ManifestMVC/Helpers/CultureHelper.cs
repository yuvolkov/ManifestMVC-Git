using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Threading;

using ManifestMVC.Configurations;


namespace ManifestMVC.Helpers
{
    public static class CultureHelper
    {
        /// <summary>
        /// Returns a valid culture name based on "name" parameter. If "name" is not valid, it returns the default culture "en-US"
        /// </summary>
        /// <param name="name" />Culture's name (e.g. en-US)</param>
        public static string GetImplementedCulture(string name)
        {
            // Loading configuration

            LanguageSection languageSection = ConfigurationManager.GetSection("language") as LanguageSection;

            // make sure it's not null
            if (string.IsNullOrEmpty(name))
                return languageSection.DefaultLanguage; // return Default culture
            // if it is implemented, accept it
            if (languageSection.Languages[name] != null)
                return name; // accept it
            // Find a close match. For example, if you have "en-US" defined and the user requests "en-GB",
            // the function will return closes match that is "en-US" because at least the language is the same (ie English)
            var neutral = GetNeutralCulture(name);
            if (languageSection.Languages[neutral] != null)
                return neutral; // accept it
            // else 
            // It is not implemented
            return languageSection.DefaultLanguage; // return Default culture as no match found
        }

        public static string GetNeutralCulture(string name)
        {
            if (name.Length < 2)
                return name;
            return name.Substring(0, 2); // Read first two chars only. E.g. "en", "es"
        }
    }
}