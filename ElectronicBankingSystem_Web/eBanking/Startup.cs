using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(eBanking.Startup))]
namespace eBanking
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
