using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

//using ManifestMVC.Configurations;
using DataLayer.Repositories.Settings;


namespace ManifestMVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            #region i18n override config
            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null);
            System.Configuration.KeyValueConfigurationElement defaultLanguage = rootWebConfig.AppSettings.Settings["defaultLanguage"];
            if (defaultLanguage != null)
                i18n.LocalizedApplication.Current.DefaultLanguage = defaultLanguage.Value;

            // Change from the of temporary redirects during URL localization
            //i18n.LocalizedApplication.Current.PermanentRedirects = true;

            // This line can be used to disable URL Localization.
            i18n.LocalizedApplication.Current.EarlyUrlLocalizerService = null;

            // Change the URL localization scheme from Scheme1.
            //i18n.UrlLocalizer.UrlLocalizationScheme = i18n.UrlLocalizationScheme.Scheme2;

            // Blacklist certain URLs from being 'localized'.
            //i18n.UrlLocalizer.IncomingUrlFilters += delegate(Uri url)
            //{
            //    if (url.LocalPath.EndsWith("sitemap.xml", StringComparison.OrdinalIgnoreCase))
            //    {
            //        return false;
            //    }
            //    return true;
            //};
            #endregion

            // Disabling WinForms engine (speeds up debug only)
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            // Do not display vulnureable info in headers
            MvcHandler.DisableMvcResponseHeader = true;

            // Global Settings

            ConfigRepo.Initialize(Server.MapPath("App_Data"));
            ParametersRepo.Initialize();

            DataLayer.Reflect.DoReflect();

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }


        protected void Application_BeginRequest()
        {
            // Storing specific (from request) Parameters for reference in DataLayer

            HttpCookie cultureCookie = Request.Cookies["i18n.langtag"];
            if (cultureCookie != null)
                RequestParameters.Language = cultureCookie.Value;
            else
                RequestParameters.Language = i18n.LocalizedApplication.Current.DefaultLanguage;
        }


        protected void Application_PreRequestHandlerExecute()
        {
            RequestParameters.SubdomainName = (string)Request.RequestContext.RouteData.Values["subdomain"] ?? "default";
        }


        protected void Application_EndRequest()
        {
            //here breakpoint
            // under debug mode you can find the exceptions at code: this.Context.AllErrors
            if (this.Context.AllErrors != null)
            {
            }
        }
    }
}
