using EmployeeTracking.API.Extensions;
using EmployeeTracking.Application.Commands.Activity;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Queries.Activity;
using EmployeeTracking.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeTracking.API.Controllers
{
    /// <summary>Records and reports application usage and hours.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ActivityController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ActivityController(IMediator mediator) => _mediator = mediator;

        /// <summary>Record a finished app session for the signed-in employee.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(RecordAppUsageResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Record(
            [FromBody] RecordAppUsageRequest request, CancellationToken ct)
        {
            var source = AppUsageSource.Manual;
            if (!string.IsNullOrWhiteSpace(request.Source)
                && !Enum.TryParse(request.Source, ignoreCase: true, out source))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation failed",
                    Detail = "Source must be System or Manual."
                });
            }

            var result = await _mediator.Send(new RecordAppUsageCommand(
                EmployeeId: User.GetEmployeeId(),
                AppName: request.AppName,
                StartedAt: request.StartedAt,
                EndedAt: request.EndedAt,
                Source: source), ct);

            return Ok(result);
        }

        /// <summary>
        /// Apps and hours for one employee. Employees see themselves.
        /// Managers can also see direct reports. Admins can see anyone.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(AppActivityReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to,
            [FromQuery] Guid? employeeId,
            CancellationToken ct)
        {
            var range = ResolveRange(from, to);
            return Ok(await _mediator.Send(new GetAppActivityQuery(
                RequesterId: User.GetEmployeeId(),
                IsAdmin: User.IsInRole("Admin"),
                IsManager: User.IsInRole("Manager"),
                EmployeeId: employeeId,
                From: range.From,
                To: range.To), ct));
        }

        /// <summary>
        /// Hours and top app for each visible user.
        /// Admins see every active employee. Managers see direct reports.
        /// </summary>
        [HttpGet("users")]
        [ProducesResponseType(typeof(IReadOnlyList<UserActivitySummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Users(
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to,
            CancellationToken ct)
        {
            var range = ResolveRange(from, to);
            return Ok(await _mediator.Send(new GetUserActivityQuery(
                RequesterId: User.GetEmployeeId(),
                IsAdmin: User.IsInRole("Admin"),
                IsManager: User.IsInRole("Manager"),
                From: range.From,
                To: range.To), ct));
        }

        private static (DateTimeOffset From, DateTimeOffset To) ResolveRange(
            DateTimeOffset? from, DateTimeOffset? to)
        {
            var end = to ?? DateTimeOffset.UtcNow;
            var start = from ?? end.AddDays(-7);
            return (start, end);
        }
    }
}
