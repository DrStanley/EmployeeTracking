using EmployeeTracking.Application.Commands.Auth;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponse> Handle(
            LoginCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            var result = await _signInManager.CheckPasswordSignInAsync(
                user, request.Password, lockoutOnFailure: false);

            if (!result.Succeeded)
                throw new UnauthorizedAccessException("Invalid email or password.");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenService.GenerateToken(user.Id, user.Email!, user.EmployeeId, roles);

            return new AuthResponse(
                Token: token,
                Email: user.Email!,
                FullName: user.UserName ?? user.Email!,
                Roles: roles,
                ExpiresAt: DateTime.UtcNow.AddMinutes(60));
        }
    }
}
