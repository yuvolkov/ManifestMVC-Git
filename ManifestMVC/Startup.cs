using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ManifestMVC.Startup))]
namespace ManifestMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
