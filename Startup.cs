using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Pixurf.Startup))]
namespace Pixurf
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
