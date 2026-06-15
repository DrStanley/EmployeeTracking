using EmployeeTracking.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EmployeeTracking.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                await HandleAsync(ctx, ex);
            }
        }

        private async Task HandleAsync(HttpContext ctx, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                ctx.Request.Method, ctx.Request.Path);

            var (status, title) = ex switch
            {
                NotFoundException => (404, "Resource not found"),
                DomainException => (422, "Business rule violation"),
                UnauthorizedAccessException => (401, "Unauthorized"),
                FluentValidation.ValidationException
                                            => (400, "Validation failed"),
                _ => (500, "An unexpected error occurred")
            };

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                // Only expose message for known domain exceptions
                // Never expose raw exception details for 500s
                Detail = status == 500
                    ? "An internal error occurred. Please contact support."
                    : ex.Message
            };

            if (ex is FluentValidation.ValidationException ve)
            {
                problem.Extensions["errors"] = ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());
            }

            // Add a correlation ID so support can trace the error in logs
            var correlationId = ctx.TraceIdentifier;
            problem.Extensions["correlationId"] = correlationId;

            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/problem+json";

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
