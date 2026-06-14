using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Infrastructure.Handlers.Admin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeTracking.API.Controllers
{
    /// <summary>
    /// Admin-only configuration endpoints for policies,
    /// shifts, holidays, overtime rules, and pay periods.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator) => _mediator = mediator;


        /// <summary>Get all attendance policies.</summary>
        [HttpGet("policies")]
        [ProducesResponseType(typeof(IReadOnlyList<AttendancePolicyDto>),
            StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPolicies(CancellationToken ct)
            => Ok(await _mediator.Send(new GetAllAttendancePoliciesQuery(), ct));

        /// <summary>Get a single attendance policy.</summary>
        [HttpGet("policies/{id}")]
        [ProducesResponseType(typeof(AttendancePolicyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPolicy(Guid id, CancellationToken ct)
            => Ok(await _mediator.Send(new GetAttendancePolicyQuery(id), ct));

        /// <summary>Create a new attendance policy.</summary>
        [HttpPost("policies")]
        [ProducesResponseType(typeof(AttendancePolicyDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreatePolicy(
            [FromBody] CreateAttendancePolicyRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(
                new CreateAttendancePolicyCommand(request), ct);
            return CreatedAtAction(nameof(GetPolicy), new { id = result.Id }, result);
        }

        /// <summary>Update an attendance policy.</summary>
        [HttpPut("policies/{id}")]
        [ProducesResponseType(typeof(AttendancePolicyDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePolicy(
            Guid id, [FromBody] UpdateAttendancePolicyRequest request, CancellationToken ct)
            => Ok(await _mediator.Send(
                new UpdateAttendancePolicyCommand(id, request), ct));

        /// <summary>Delete an attendance policy.</summary>
        [HttpDelete("policies/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> DeletePolicy(Guid id, CancellationToken ct)
        {
            await _mediator.Send(new DeleteAttendancePolicyCommand(id), ct);
            return NoContent();
        }


        /// <summary>Get PTO accrual settings for a policy.</summary>
        [HttpGet("policies/{policyId}/pto-settings")]
        [ProducesResponseType(typeof(PTOAccrualSettingsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPTOSettings(
            Guid policyId, CancellationToken ct)
            => Ok(await _mediator.Send(
                new GetPTOAccrualSettingsQuery(policyId), ct));

        /// <summary>Update PTO accrual settings for a policy.</summary>
        [HttpPut("policies/{policyId}/pto-settings")]
        [ProducesResponseType(typeof(PTOAccrualSettingsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePTOSettings(
            Guid policyId,
            [FromBody] UpdatePTOAccrualSettingsRequest request,
            CancellationToken ct)
            => Ok(await _mediator.Send(
                new UpdatePTOAccrualSettingsCommand(policyId, request), ct));


        /// <summary>Get all overtime rules.</summary>
        [HttpGet("overtime-rules")]
        [ProducesResponseType(typeof(IReadOnlyList<OvertimeRuleDto>),
            StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOvertimeRules(CancellationToken ct)
            => Ok(await _mediator.Send(new GetAllOvertimeRulesQuery(), ct));

        /// <summary>Create an overtime rule for a policy.</summary>
        [HttpPost("overtime-rules")]
        [ProducesResponseType(typeof(OvertimeRuleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateOvertimeRule(
            [FromBody] CreateOvertimeRuleRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(
                new CreateOvertimeRuleCommand(request), ct);
            return CreatedAtAction(nameof(GetOvertimeRules), result);
        }

        /// <summary>Update an overtime rule.</summary>
        [HttpPut("overtime-rules/{id}")]
        [ProducesResponseType(typeof(OvertimeRuleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOvertimeRule(
            Guid id, [FromBody] UpdateOvertimeRuleRequest request, CancellationToken ct)
            => Ok(await _mediator.Send(
                new UpdateOvertimeRuleCommand(id, request), ct));

        /// <summary>Delete an overtime rule.</summary>
        [HttpDelete("overtime-rules/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteOvertimeRule(
            Guid id, CancellationToken ct)
        {
            await _mediator.Send(new DeleteOvertimeRuleCommand(id), ct);
            return NoContent();
        }


        /// <summary>Get all active shifts.</summary>
        [HttpGet("shifts")]
        [ProducesResponseType(typeof(IReadOnlyList<ShiftDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetShifts(CancellationToken ct)
            => Ok(await _mediator.Send(new GetAllShiftsQuery(), ct));

        /// <summary>Create a new shift.</summary>
        [HttpPost("shifts")]
        [ProducesResponseType(typeof(ShiftDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateShift(
            [FromBody] CreateShiftRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new CreateShiftCommand(request), ct);
            return CreatedAtAction(nameof(GetShifts), result);
        }

        /// <summary>Update a shift.</summary>
        [HttpPut("shifts/{id}")]
        [ProducesResponseType(typeof(ShiftDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateShift(
            Guid id, [FromBody] UpdateShiftRequest request, CancellationToken ct)
            => Ok(await _mediator.Send(new UpdateShiftCommand(id, request), ct));

        /// <summary>Deactivate a shift (soft delete).</summary>
        [HttpDelete("shifts/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteShift(Guid id, CancellationToken ct)
        {
            await _mediator.Send(new DeleteShiftCommand(id), ct);
            return NoContent();
        }


        /// <summary>Get all holidays for a given year.</summary>
        [HttpGet("holidays")]
        [ProducesResponseType(typeof(IReadOnlyList<HolidayDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHolidays(
            [FromQuery] int year = 0, CancellationToken ct = default)
            => Ok(await _mediator.Send(
                new GetHolidaysQuery(year == 0 ? DateTime.UtcNow.Year : year), ct));

        /// <summary>Add a holiday to the calendar.</summary>
        [HttpPost("holidays")]
        [ProducesResponseType(typeof(HolidayDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateHoliday(
            [FromBody] CreateHolidayRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new CreateHolidayCommand(request), ct);
            return CreatedAtAction(nameof(GetHolidays), result);
        }

        /// <summary>Update a holiday.</summary>
        [HttpPut("holidays/{id}")]
        [ProducesResponseType(typeof(HolidayDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateHoliday(
            Guid id, [FromBody] UpdateHolidayRequest request, CancellationToken ct)
            => Ok(await _mediator.Send(new UpdateHolidayCommand(id, request), ct));

        /// <summary>Remove a holiday from the calendar.</summary>
        [HttpDelete("holidays/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteHoliday(Guid id, CancellationToken ct)
        {
            await _mediator.Send(new DeleteHolidayCommand(id), ct);
            return NoContent();
        }


        /// <summary>Get all pay periods.</summary>
        [HttpGet("pay-periods")]
        [ProducesResponseType(typeof(IReadOnlyList<PayPeriodDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPayPeriods(CancellationToken ct)
            => Ok(await _mediator.Send(new GetAllPayPeriodsQuery(), ct));

        /// <summary>Create a new pay period.</summary>
        [HttpPost("pay-periods")]
        [ProducesResponseType(typeof(PayPeriodDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreatePayPeriod(
            [FromBody] CreatePayPeriodRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(
                new CreatePayPeriodCommand(request), ct);
            return CreatedAtAction(nameof(GetPayPeriods), result);
        }

        /// <summary>Lock a pay period — prevents further edits or report regeneration.</summary>
        [HttpPost("pay-periods/{id}/lock")]
        [ProducesResponseType(typeof(PayPeriodDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> LockPayPeriod(Guid id, CancellationToken ct)
            => Ok(await _mediator.Send(new LockPayPeriodCommand(id), ct));
    }
}
