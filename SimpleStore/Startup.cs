using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SimpleStore.Startup))]
namespace SimpleStore
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
     //       ConfigureAuth(app);
        }
    }
}
