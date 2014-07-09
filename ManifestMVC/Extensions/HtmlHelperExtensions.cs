using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ManifestMVC.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static bool IsDebug(this HtmlHelper htmlHelper)
        {
#if DEBUG
            return true;
#else
      return false;
#endif
        }

    }
}