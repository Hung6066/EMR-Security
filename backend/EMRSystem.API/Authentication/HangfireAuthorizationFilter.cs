using Hangfire.Annotations;
using Hangfire.Dashboard;
using System.Diagnostics.CodeAnalysis;

namespace EMRSystem.API.Authentication
{
    /// <summary>
    /// Custom authorization filter for Hangfire Dashboard.
    /// Ensures that only users with the "Admin" role can access the dashboard.
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _requiredRole;

        public HangfireAuthorizationFilter(string requiredRole = "Admin")
        {
            _requiredRole = requiredRole;
        }

        /// <summary>
        /// This method is called by Hangfire to determine if a user is authorized to view the dashboard.
        /// </summary>
        /// <param name="context">The Hangfire dashboard context, which includes the HttpContext.</param>
        /// <returns>True if the user is authorized, otherwise false.</returns>
        public bool Authorize([NotNull] DashboardContext context)
        {
            // Get the HttpContext from the dashboard context.
            var httpContext = context.GetHttpContext();

            // 1. Check if the user is authenticated.
            var isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;
            if (!isAuthenticated)
            {
                // If not authenticated, deny access.
                // Hangfire will typically handle this by showing a login prompt or a 401 Unauthorized response.
                return false;
            }

            // 2. Check if the authenticated user has the required role.
            // This is case-sensitive by default.
            var hasRequiredRole = httpContext.User.IsInRole(_requiredRole);
            if (hasRequiredRole)
            {
                // User has the required role, grant access.
                return true;
            }

            // If the user is authenticated but does not have the required role, deny access.
            // You can log this attempt for security monitoring.
            var user = httpContext.User.Identity?.Name ?? "Unknown";
            Console.WriteLine($"Hangfire Dashboard Access Denied: User '{user}' attempted to access without a role '{_requiredRole}'.");

            return false;
        }
    }
}