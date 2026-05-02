using Microsoft.AspNetCore.Authentication;

namespace EmployeeTracking.API.Extensions
{
    public class UnauthorizedResponseHandler : IAuthenticationHandler
    {
        // We only need this for the response — we delegate actual auth to JwtBearer
        public Task<AuthenticateResult> AuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        public Task ChallengeAsync(AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }

        public Task ForbidAsync(AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
