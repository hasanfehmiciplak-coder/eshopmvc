using Hangfire.Annotations;
using Hangfire.Dashboard;
using System.Security.Claims;

namespace EShopMVC.Infrastructure.Hangfire
{
    public class HangfireDashboardAuthorizationFilter
        : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            return httpContext.User.IsInRole("Admin")
                || httpContext.User.IsInRole("Fraud");
        }
    }
}