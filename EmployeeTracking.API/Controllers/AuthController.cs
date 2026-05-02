using EmployeeTracking.Application.Commands.Auth;
using EmployeeTracking.Application.Queries.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeTracking.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator) => _mediator = mediator;

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] RegisterCommand command, CancellationToken ct)
            => Ok(await _mediator.Send(command, ct));

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand command, CancellationToken ct)
            => Ok(await _mediator.Send(command, ct));

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? throw new UnauthorizedAccessException("Email claim missing from token.");

            return Ok(await _mediator.Send(new GetCurrentUserQuery(email), ct));
        }
    }
}
//Admin test data
//    Username: vosedi8744@cadinr.com
//    Password: Vosedi8744@cadinr.com
//Manager test data
//    Username: s6zrp0jc6c@lnovic.com
//    Password: S6zrp0jc6c@lnovic.com