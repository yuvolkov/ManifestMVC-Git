using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

using DataLayer;


namespace ManifestMVC.Extensions
{
    public static class RequestResultExtensions
    {
        public static ActionResult ToActionResult(this RequestResult reqResult)
        {
            switch (reqResult.Status)
            {
                case ReqResultStatus.BadRequest:
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                case ReqResultStatus.NotFound:
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                case ReqResultStatus.InvalidOperation:
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                case ReqResultStatus.AccessDenied:
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                case ReqResultStatus.Success:
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                default:
                    throw new NotImplementedException();
            }
        }


        public static ActionResult ToActionResult(this RequestResult reqResult, Func<ActionResult> onSuccess)
        {
            if (reqResult.Status == ReqResultStatus.Success)
                return onSuccess();
            else
                return reqResult.ToActionResult();
        }


        public static ActionResult ToActionResult<T1>(this RequestResult<T1> reqResult, Func<T1,ActionResult> onSuccess)
        {
            return reqResult.ToActionResult(() => onSuccess(reqResult.Data1));
        }

    }
}