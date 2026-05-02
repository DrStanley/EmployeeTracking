using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace EmployeeTracking.API.Extensions
{
    public class ForbiddenResponseHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _default = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Challenged)
            {
                await _default.HandleAsync(next, context, policy, authorizeResult);
                return;
            }

            if (authorizeResult.Forbidden)
            {
                // Show all roles the user currently holds
                var currentRoles = context.User.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                var currentRolesDisplay = currentRoles.Any()
                    ? string.Join(", ", currentRoles)
                    : "none";

                // Extract required roles from endpoint metadata
                var endpoint = context.GetEndpoint();
                var authorizeAttr = endpoint?
                    .Metadata
                    .GetOrderedMetadata<AuthorizeAttribute>();

                var requiredRoles = authorizeAttr?
                    .Where(a => !string.IsNullOrWhiteSpace(a.Roles))
                    .Select(a => a.Roles!)
                    .FirstOrDefault() ?? "a permitted role";

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Access denied",
                    Detail = $"Your role(s) '{currentRolesDisplay}' do not have " +
                             $"permission to access this endpoint. " +
                             $"Required: {requiredRoles}."
                };

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
                return;
            }

            await _default.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
