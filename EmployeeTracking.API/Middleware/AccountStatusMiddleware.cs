using System.Security.Claims;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace EmployeeTracking.API.Middleware
{
    public class AccountStatusMiddleware
    {
        private readonly RequestDelegate _next;

        public AccountStatusMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext ctx, UserManager<ApplicationUser> users)
        {
            if (ctx.User.Identity?.IsAuthenticated == true && !IsLogout(ctx.Request))
            {
                var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = userId is null ? null : await users.FindByIdAsync(userId);
                if (user is null || user.AccountStatus == AccountStatus.Deleted)
                {
                    await Write(ctx, StatusCodes.Status401Unauthorized, "Unauthorized", AccountStatusMessages.Deleted);
                    return;
                }

                if (user.AccountStatus == AccountStatus.Suspended)
                {
                    await Write(ctx, StatusCodes.Status403Forbidden, "Account suspended", AccountStatusMessages.Suspended);
                    return;
                }
            }

            await _next(ctx);
        }

        private static bool IsLogout(HttpRequest request)
            => HttpMethods.IsPost(request.Method)
               && request.Path.StartsWithSegments("/api/auth/logout");

        private static async Task Write(HttpContext ctx, int status, string title, string detail)
        {
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/problem+json";
            await ctx.Response.WriteAsJsonAsync(new
            {
                status,
                title,
                detail
            });
        }
    }
}
