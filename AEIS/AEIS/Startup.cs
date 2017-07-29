using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AEIS.Startup))]
namespace AEIS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
        }
    }
}
