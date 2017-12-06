using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(SignalR.StockTicker.Startup))]

namespace SignalR.StockTicker
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }
}