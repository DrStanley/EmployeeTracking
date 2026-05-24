using EmployeeTracking.API.Extensions;
using EmployeeTracking.Application.Commands.Timesheets;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Queries.Timesheets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeTracking.API.Controllers
{
    /// <summary>Manages employee timesheets — creation, submission, and approval.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TimesheetsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TimesheetsController(IMediator mediator) => _mediator = mediator;

        /// <summary>Create or retrieve a timesheet for a pay period.</summary>
        [HttpPost]
        [Authorize(Roles = "Employee,Manager")]
        [ProducesResponseType(typeof(TimesheetDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create(
            [FromBody] CreateTimesheetCommand command, CancellationToken ct)
            => Ok(await _mediator.Send(command, ct));

        /// <summary>Get a specific timesheet by employee and pay period.</summary>
        [HttpGet("{employeeId}/{payPeriodId}")]
        [ProducesResponseType(typeof(TimesheetDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(
            Guid employeeId, Guid payPeriodId, CancellationToken ct)
            => Ok(await _mediator.Send(
                new GetTimesheetQuery(employeeId, payPeriodId), ct));

        /// <summary>Submit a timesheet for manager approval.</summary>
        [HttpPost("{id}/submit")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
        {
            await _mediator.Send(
                new SubmitTimesheetCommand(id, User.GetEmployeeId()), ct);
            return NoContent();
        }

        /// <summary>Approve a submitted timesheet.</summary>
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Approve(
            Guid id, [FromBody] ApproveTimesheetRequest request, CancellationToken ct)
        {
            await _mediator.Send(
                new ApproveTimesheetCommand(id, User.GetEmployeeId(), request.Notes), ct);
            return NoContent();
        }

        /// <summary>Reject a submitted timesheet with a reason.</summary>
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Reject(
            Guid id, [FromBody] RejectTimesheetRequest request, CancellationToken ct)
        {
            await _mediator.Send(
                new RejectTimesheetCommand(id, User.GetEmployeeId(), request.Reason), ct);
            return NoContent();
        }

        /// <summary>Get all timesheets pending approval for the authenticated manager.</summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(IReadOnlyList<TimesheetDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Pending(CancellationToken ct)
            => Ok(await _mediator.Send(
                new GetPendingApprovalsQuery(User.GetEmployeeId()), ct));
    }
}
