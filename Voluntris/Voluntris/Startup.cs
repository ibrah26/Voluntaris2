using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Voluntris.Startup))]
namespace Voluntris
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
