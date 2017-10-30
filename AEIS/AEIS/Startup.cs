using Microsoft.Owin;
using Owin;
using XSockets.Owin.Host;

[assembly: OwinStartupAttribute(typeof(AEIS.Startup))]
namespace AEIS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.UseXSockets();
        }
    }
}
