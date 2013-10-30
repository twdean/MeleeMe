using Microsoft.Owin;
using Owin;
[assembly: OwinStartup(typeof(Melee.Me.SignalR.Startup))]
namespace Melee.Me.SignalR
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}