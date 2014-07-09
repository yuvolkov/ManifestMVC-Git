using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;


namespace ManifestMVC
{
    public class SubdomainRoute : Route
    {
        public SubdomainRoute(string url) : base(url, new MvcRouteHandler()) { }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var routeData = base.GetRouteData(httpContext);
            if (routeData == null) return null; // Only look at the subdomain if this route matches in the first place.
            string subdomain = httpContext.Request.Params["subdomain"]; // A subdomain specified as a query parameter takes precedence over the hostname.
            if (subdomain == null)
            {
                string host = httpContext.Request.Headers["Host"];

                // Compare Host to what should be and extracting subdomain

                string host_template = DataLayer.Repositories.Settings.ConfigRepo.Instance.Host;
                if (host != host_template)
                {   
                    int index = host.IndexOf('.');
                    if (index == -1 || host.Substring(index + 1) != host_template)
                        throw new InvalidOperationException("Should be " + host_template + ", got " + host);
                    subdomain = host.Substring(0, index);
                }
            }
            if (subdomain != null)
                routeData.Values["subdomain"] = subdomain;
            return routeData;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            object subdomainParam = requestContext.HttpContext.Request.Params["subdomain"];
            if (subdomainParam != null)
                values["subdomain"] = subdomainParam;
            return base.GetVirtualPath(requestContext, values);
        }
    }


    public static class RouteCollectionExtensions
    {
        public static void MapSubdomainRoute(this RouteCollection routes, string name, string url, object defaults = null, object constraints = null)
        {
            routes.Add(name, new SubdomainRoute(url)
            {
                Defaults = new RouteValueDictionary(defaults),
                Constraints = new RouteValueDictionary(constraints),
                DataTokens = new RouteValueDictionary()
            });
        }
    }
}