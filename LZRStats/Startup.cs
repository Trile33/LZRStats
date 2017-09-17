using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LZRStats.Startup))]
namespace LZRStats
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
