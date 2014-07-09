using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace WebSafeLibrary
{
    public sealed class WebSafeCallContext
    {
        private WebSafeCallContext()
        {
            throw new NotSupportedException("must not be instantiated");
        }

        public static object Get(string name)
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null)
            {
                return CallContext.GetData(name);
            }
            else
            {
                return ctx.Items[name];
            }
        }

        public static void Set(string name, object value)
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null)
            {
                CallContext.SetData(name, value);
            }
            else
            {
                ctx.Items[name] = value;
            }
        }

        public static void Remove(string name)
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx == null)
            {
                CallContext.FreeNamedDataSlot(name);
            }
            else
            {
                ctx.Items.Remove(name);
            }
        }
    }
}
