namespace EmployeeTracking.API.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext ctx)
        {
            ctx.Response.Headers.Append(
                "X-Content-Type-Options", "nosniff");
            ctx.Response.Headers.Append(
                "X-Frame-Options", "DENY");
            ctx.Response.Headers.Append(
                "X-XSS-Protection", "1; mode=block");
            ctx.Response.Headers.Append(
                "Referrer-Policy", "strict-origin-when-cross-origin");
            ctx.Response.Headers.Append(
                "Permissions-Policy",
                "geolocation=(), microphone=(), camera=()");
            ctx.Response.Headers.Append(
                "Content-Security-Policy",
                "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'");

            // Remove server header — don't advertise the tech stack
            ctx.Response.Headers.Remove("Server");
            ctx.Response.Headers.Remove("X-Powered-By");

            await _next(ctx);
        }
    }
}
