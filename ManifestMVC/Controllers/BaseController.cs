using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Threading;

//using ManifestMVC.Helpers;
using DataLayer;
using System.Security.Claims;


namespace ManifestMVC.Controllers
{
    public class BaseController : Controller
    {
        public int? UserID { get; set; }

        protected string Subdomain
        {
            get { return (string)Request.RequestContext.RouteData.Values["subdomain"] ?? "default"; }
        }
        
        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            //string cultureName = null;

            //// Attempt to read the culture cookie from Request
            //HttpCookie cultureCookie = Request.Cookies["language"];
            //if (cultureCookie != null)
            //    cultureName = cultureCookie.Value;
            //else
            //    cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0 ?
            //            Request.UserLanguages[0] :  // obtain it from HTTP header AcceptLanguages
            //            null;
            //// Validate culture name
            //cultureName = CultureHelper.GetImplementedCulture(cultureName); // This is safe

            //// Modify current thread's cultures            
            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            //Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            if (User.Identity.IsAuthenticated)
            {
                UserID =
                    (from c in ((ClaimsPrincipal)User).Claims
                     where c.Type == ClaimTypes.NameIdentifier
                     select int.Parse(c.Value)).SingleOrDefault();
            }

            ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;

            ViewBag.Subdomain = Subdomain;

            return base.BeginExecuteCore(callback, state);
        }


        //public static ActionResult UnsuccessfulActionResultFrom(ReqResultStatus status)
        //{
        //    switch(status)
        //    {
        //        case ReqResultStatus.BadRequest:
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //        case ReqResultStatus.NotFound:
        //            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        //        case ReqResultStatus.InvalidOperation:
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //        case ReqResultStatus.AccessDenied:
        //            return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        //        case ReqResultStatus.Success:
        //            throw new InvalidOperationException("Only for Unsuccessful results!");
        //        default:
        //            throw new NotImplementedException();
        //    }
        //}


        //public static ActionResult ActionResultFrom(ReqResultStatus status, Func<ViewResult> viewFunc)
        //{
        //    if (status == ReqResultStatus.Success)
        //        return viewFunc();
        //    else
        //        return UnsuccessfulActionResultFrom(status);
        //}


        //public ActionResult UnsuccessfulActionResultFrom(RequestResult reqResult)
        //{
        //    return UnsuccessfulActionResultFrom(reqResult.Status);
        //}


        //public ActionResult ActionResultFrom<T1>(RequestResult<T1> reqResult, Func<object,ViewResult> viewFunc)
        //{
        //    return ActionResultFrom(reqResult.Status, () => viewFunc(reqResult.Data1));
        //}
    }
}