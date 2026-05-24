using EmployeeTracking.API.Extensions;
using EmployeeTracking.Application.Commands.ApprovePTORequest;
using EmployeeTracking.Application.Commands.CancelPTORequest;
using EmployeeTracking.Application.Commands.RejectPTORequest;
using EmployeeTracking.Application.Commands.SubmitPTORequest;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Queries.PTO;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeTracking.API.Controllers
{
    /// <summary>Manages PTO requests and balance tracking.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PTOController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PTOController(IMediator mediator) => _mediator = mediator;

        /// <summary>Submit a new PTO request.</summary>
        [HttpPost]
        [Authorize(Roles = "Employee,Manager")]
        [ProducesResponseType(typeof(PTORequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Submit(
            [FromBody] SubmitPTORequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new SubmitPTOCommand(
                EmployeeId: User.GetEmployeeId(),
                StartDate: request.StartDate,
                EndDate: request.EndDate,
                HoursRequested: request.HoursRequested,
                Notes: request.Notes), ct);

            return CreatedAtAction(nameof(MyRequests), result);
        }

        /// <summary>Get all my PTO requests.</summary>
        [HttpGet("my")]
        [Authorize(Roles = "Employee,Manager")]
        [ProducesResponseType(typeof(IReadOnlyList<PTORequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MyRequests(CancellationToken ct)
            => Ok(await _mediator.Send(
                new GetMyPTORequestsQuery(User.GetEmployeeId()), ct));

        /// <summary>Get my current PTO balance.</summary>
        [HttpGet("balance")]
        [Authorize(Roles = "Employee,Manager")]
        [ProducesResponseType(typeof(PTOBalanceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Balance(CancellationToken ct)
            => Ok(await _mediator.Send(
                new GetPTOBalanceQuery(
                    User.GetEmployeeId(),
                    DateTime.UtcNow.Year), ct));

        /// <summary>Get all pending PTO requests for the authenticated manager.</summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(IReadOnlyList<PTORequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Pending(CancellationToken ct)
            => Ok(await _mediator.Send(
                new GetPendingPTOForManagerQuery(User.GetEmployeeId()), ct));

        /// <summary>Approve a pending PTO request.</summary>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(PTORequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Approve(
            Guid id, [FromBody] ApprovePTORequest request, CancellationToken ct)
            => Ok(await _mediator.Send(
                new ApprovePTOCommand(id, User.GetEmployeeId(), request.Notes), ct));

        /// <summary>Reject a pending PTO request.</summary>
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(PTORequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Reject(
            Guid id, [FromBody] RejectPTORequest request, CancellationToken ct)
            => Ok(await _mediator.Send(
                new RejectPTOCommand(id, User.GetEmployeeId(), request.Reason), ct));

        /// <summary>Cancel my own PTO request.</summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Employee,Manager")]
        [ProducesResponseType(typeof(PTORequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
            => Ok(await _mediator.Send(
                new CancelPTOCommand(id, User.GetEmployeeId()), ct));
    }
}
