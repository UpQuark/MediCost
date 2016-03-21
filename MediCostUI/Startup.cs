using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MediCostUI.Startup))]
namespace MediCostUI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
