using Hangfire.Dashboard;
using System.Net;

namespace BankerDeskOps.Api.Hangfire
{
    public sealed class LocalOrAdminDashboardFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            if (httpContext.Connection.RemoteIpAddress is null)
                return false;

            if (IPAddress.IsLoopback(httpContext.Connection.RemoteIpAddress))
                return true;

            // Allow when running behind a reverse proxy that forwards the admin header
            if (httpContext.Request.Headers.TryGetValue("X-Admin-Key", out var key))
                return key == httpContext.RequestServices.GetRequiredService<IConfiguration>()["Hangfire:AdminKey"];

            return false;
        }
    }
}
