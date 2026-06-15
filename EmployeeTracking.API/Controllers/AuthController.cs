using EmployeeTracking.Application.Commands.Auth;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Queries.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace EmployeeTracking.API.Controllers
{
    /// <summary>Authentication — register, login, token refresh, and logout.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator) => _mediator = mediator;

        /// <summary>Register a new user account.</summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register(
            [FromBody] RegisterCommand command, CancellationToken ct)
            => Ok(await _mediator.Send(command, ct));

        /// <summary>Login and receive an access token and refresh token.</summary>
        /// <remarks>
        /// Access tokens expire after 15 minutes.
        /// Use the refresh token at POST /api/auth/refresh to get a new pair.
        ///
        /// Accounts are locked for 5 minutes after 5 failed attempts.
        /// </remarks>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand command, CancellationToken ct)
            => Ok(await _mediator.Send(command, ct));

        /// <summary>
        /// Exchange an expired access token and valid refresh token
        /// for a new token pair.
        /// </summary>
        /// <remarks>
        /// Send both the expired access token and the current refresh token.
        /// A new access token and rotated refresh token are returned.
        /// The old refresh token is immediately invalidated.
        ///
        /// If a previously used refresh token is submitted, ALL sessions
        /// for that user are terminated as a security precaution.
        /// </remarks>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshTokenRequest request, CancellationToken ct)
            => Ok(await _mediator.Send(
                new RefreshTokenCommand(request.AccessToken, request.RefreshToken), ct));

        /// <summary>Logout — revoke the current refresh token.</summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout(
            [FromBody] RevokeTokenRequest request, CancellationToken ct)
        {
            await _mediator.Send(new RevokeTokenCommand(request.RefreshToken), ct);
            return NoContent();
        }

        /// <summary>
        /// Admin only — revoke ALL sessions for a specific user.
        /// Use when an account is compromised or an employee is terminated.
        /// </summary>
        [HttpPost("revoke-all/{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeAll(string userId, CancellationToken ct)
        {
            await _mediator.Send(new RevokeAllTokensCommand(userId), ct);
            return NoContent();
        }

        /// <summary>Returns the current authenticated user's info.</summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Me(CancellationToken ct)
            => Ok(await _mediator.Send(
                new GetCurrentUserQuery(User.FindFirstValue(ClaimTypes.Email)!), ct));
    }
}
//Admin test data
//    Username: vosedi8744@cadinr.com
//    Password: Vosedi8744@cadinr.com
//Manager test data
//    Username: s6zrp0jc6c@lnovic.com
//    Password: S6zrp0jc6c@lnovic.com