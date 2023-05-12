using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(MyWeb.Areas.Identity.IdentityHostingStartup))]
namespace MyWeb.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}