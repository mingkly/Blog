using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MyWeb.Middlewares
{
    public class EnableRequestBuffering
    {
        private readonly RequestDelegate _next;
        public EnableRequestBuffering(RequestDelegate next) => _next = next;
        public Task Invoke(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();
            return _next(httpContext);
        }
    }
    public class BlockUnloginUser
    {
        private readonly RequestDelegate _next;
        public BlockUnloginUser(RequestDelegate next) => _next = next;
        public Task Invoke(HttpContext httpContext)
        {
            if (httpContext.User.Identity.Name == null)
            {
                httpContext.SetEndpoint(null);
               
            }
            return _next(httpContext);
        }
    }
}
