using EmployeeTracking.API.Extensions;
using EmployeeTracking.Application.Commands.ClockIn;
using EmployeeTracking.Application.Commands.ClockOut;
using EmployeeTracking.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeTracking.API.Controllers
{
    /// <summary>Handles employee time tracking — clock-in, clock-out, and breaks.</summary>
    [ApiController]
    [Route("api/timeentries")]
    [Authorize]
    public class TimeEntryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TimeEntryController(IMediator mediator) => _mediator = mediator;

        /// <summary>Clock in the authenticated employee.</summary>
        /// <remarks>
        /// Records a clock-in punch for the currently authenticated employee.
        ///
        /// **Source values:**
        /// - `0` = WebApp
        /// - `1` = MobileApp
        /// - `2` = Kiosk
        /// - `3` = ManagerEntry
        /// - `4` = SystemCorrection
        ///
        /// **Common errors:**
        /// | Status | Reason |
        /// |--------|--------|
        /// | 400 | Invalid source value or missing required field |
        /// | 401 | Missing or expired JWT token |
        /// | 403 | Role not permitted (Admin cannot clock in) |
        /// | 422 | Already clocked in — clock out first |
        /// | 422 | Employee profile not found or inactive |
        ///
        /// **Example request:**
        ///
        ///     POST /api/timeentries/clock-in
        ///     {
        ///         "source": 0,
        ///         "latitude": 51.0447,
        ///         "longitude": -114.0719,
        ///         "deviceId": null
        ///     }
        /// </remarks>
        /// <response code="200">Clock-in recorded successfully.</response>
        /// <response code="400">Validation failed — invalid source or missing field.</response>
        /// <response code="401">Unauthorized — token missing or expired.</response>
        /// <response code="403">Forbidden — role not allowed to clock in.</response>
        /// <response code="422">Business rule violation — already clocked in or employee inactive.</response>
        [HttpPost("clock-in")]
        [Authorize(Roles = "Employee,Manager")]
        [ProducesResponseType(typeof(ClockInResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ClockIn(
            [FromBody] ClockInRequest request, CancellationToken ct)
            => Ok(await _mediator.Send(new ClockInCommand(
                EmployeeId: User.GetEmployeeId(),
                Source: request.Source,
                Latitude: request.Latitude,
                Longitude: request.Longitude,
                DeviceId: request.DeviceId), ct));

        /// <summary>Clock out the authenticated employee.</summary>
        /// <remarks>
        /// Records a clock-out punch and returns total hours worked since last clock-in.
        ///
        /// **Source values:**
        /// - `0` = WebApp
        /// - `1` = MobileApp
        /// - `2` = Kiosk
        /// - `3` = ManagerEntry
        /// - `4` = SystemCorrection
        ///
        /// **Common errors:**
        /// | Status | Reason |
        /// |--------|--------|
        /// | 400 | Invalid source value or missing required field |
        /// | 401 | Missing or expired JWT token |
        /// | 403 | Role not permitted |
        /// | 422 | No open clock-in found — clock in first |
        ///
        /// **Example request:**
        ///
        ///     POST /api/timeentries/clock-out
        ///     {
        ///         "source": 0,
        ///         "latitude": 51.0447,
        ///         "longitude": -114.0719,
        ///         "deviceId": null
        ///     }
        /// </remarks>
        /// <response code="200">Clock-out recorded. Returns hours worked.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="403">Forbidden.</response>
        /// <response code="422">No open clock-in found.</response>
        [HttpPost("clock-out")]
        [Authorize(Roles = "Employee,Manager")]
        [ProducesResponseType(typeof(ClockOutResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ClockOut(
            [FromBody] ClockInRequest request, CancellationToken ct)
            => Ok(await _mediator.Send(new ClockOutCommand(
                EmployeeId: User.GetEmployeeId(),
                Source: request.Source,
                Latitude: request.Latitude,
                Longitude: request.Longitude,
                DeviceId: request.DeviceId), ct));
    }
}
