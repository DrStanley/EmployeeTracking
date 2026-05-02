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
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            var (status, title) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound,
                                                "Resource not found"),
                DomainException => (StatusCodes.Status422UnprocessableEntity,
                                                "Business rule violation"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized,
                                                "Unauthorized"),
                FluentValidation.ValidationException
                                            => (StatusCodes.Status400BadRequest,
                                                "Validation failed"),
                _ => (StatusCodes.Status500InternalServerError,
                                                "An unexpected error occurred")
            };

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = ex.Message
            };

            // validation errors 
            if (ex is FluentValidation.ValidationException ve)
            {
                problem.Extensions["errors"] = ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());
            }

            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/problem+json";

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
