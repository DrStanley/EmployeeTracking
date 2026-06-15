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
        private readonly IUnitOfWork _uow;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IJwtTokenService jwtTokenService,
            IUnitOfWork uow)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _uow = uow;
        }

        public async Task<AuthResponse> Handle(
            LoginCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            var result = await _signInManager.CheckPasswordSignInAsync(
                user, request.Password, lockoutOnFailure: true);

            if (result.IsLockedOut)
                throw new UnauthorizedAccessException(
                    "Account is locked due to multiple failed attempts. " +
                    "Try again in 5 minutes.");

            if (!result.Succeeded)
                throw new UnauthorizedAccessException("Invalid email or password.");

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtTokenService.GenerateAccessToken(
                user.Id, user.Email!, user.EmployeeId, roles);
            var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

            // Revoke any old refresh tokens for this user (single session)
            await _uow.RefreshTokens.RevokeAllForUserAsync(
                user.Id, "New login", ct);

            await _uow.RefreshTokens.AddAsync(refreshToken, ct);
            await _uow.SaveChangesAsync(ct);

            return new AuthResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken.Token,
                Email: user.Email!,
                FullName: user.UserName ?? user.Email!,
                Roles: roles,
                AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(15));
        }
    }
}
