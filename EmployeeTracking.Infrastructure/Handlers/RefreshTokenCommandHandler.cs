using EmployeeTracking.Application.Commands.Auth;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class RefreshTokenCommandHandler
     : IRequestHandler<RefreshTokenCommand, AuthResponse>
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _uow;

        public RefreshTokenCommandHandler(
            IJwtTokenService jwtTokenService,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork uow)
        {
            _jwtTokenService = jwtTokenService;
            _userManager = userManager;
            _uow = uow;
        }

        public async Task<AuthResponse> Handle(
            RefreshTokenCommand request, CancellationToken ct)
        {
            // 1. Validate the expired access token and extract claims
            var principal = _jwtTokenService
                .GetPrincipalFromExpiredToken(request.AccessToken)
                ?? throw new UnauthorizedAccessException(
                    "Invalid access token.");

            var userId = principal.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException(
                    "Invalid access token — missing user ID claim.");

            // 2. Load and validate the refresh token
            var storedToken = await _uow.RefreshTokens
                .GetByTokenAsync(request.RefreshToken, ct)
                ?? throw new UnauthorizedAccessException(
                    "Refresh token not found.");

            if (storedToken.UserId != userId)
                throw new UnauthorizedAccessException(
                    "Refresh token does not match the access token.");

            if (storedToken.IsExpired)
                throw new UnauthorizedAccessException(
                    "Refresh token has expired. Please log in again.");

            if (storedToken.IsRevoked)
            {
                // Token reuse detected — revoke ALL tokens for this user
                // This indicates a potential token theft
                await _uow.RefreshTokens.RevokeAllForUserAsync(
                    userId, "Token reuse detected — possible theft", ct);
                await _uow.SaveChangesAsync(ct);

                throw new UnauthorizedAccessException(
                    "Refresh token has been revoked. " +
                    "All sessions have been terminated for security.");
            }

            // 3. Load the user and verify they are still active
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not found.");

            if (await _userManager.IsLockedOutAsync(user))
                throw new UnauthorizedAccessException(
                    "Account is locked. Please contact your administrator.");

            // 4. Generate new token pair
            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _jwtTokenService.GenerateAccessToken(
                user.Id, user.Email!, user.EmployeeId, roles);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

            // 5. Rotate — revoke the old refresh token and save the new one
            storedToken.Revoke(
                reason: "Rotated on refresh",
                replacedByToken: newRefreshToken.Token);

            _uow.RefreshTokens.Update(storedToken);
            await _uow.RefreshTokens.AddAsync(newRefreshToken, ct);
            await _uow.SaveChangesAsync(ct);

            return new AuthResponse(
                AccessToken: newAccessToken,
                RefreshToken: newRefreshToken.Token,
                Email: user.Email!,
                FullName: user.UserName ?? user.Email!,
                Roles: roles,
                AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(15));
        }
    }
}
