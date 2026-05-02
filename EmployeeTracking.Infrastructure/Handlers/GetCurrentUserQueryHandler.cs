using EmployeeTracking.Application.Queries.Auth;
using EmployeeTracking.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetCurrentUserQueryHandler
    : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetCurrentUserQueryHandler(UserManager<ApplicationUser> userManager)
            => _userManager = userManager;

        public async Task<CurrentUserResponse> Handle(
            GetCurrentUserQuery request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email)
                ?? throw new UnauthorizedAccessException("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            return new CurrentUserResponse(user.Email!, user.EmployeeId, roles);
        }
    }
}
