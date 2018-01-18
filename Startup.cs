using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(XMLParser.Startup))]
namespace XMLParser
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
