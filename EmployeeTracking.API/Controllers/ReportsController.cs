using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Queries.GetPayrollReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeTracking.API.Controllers
{
    /// <summary>Payroll reporting for managers and admins.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager,Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Get the existing payroll report for a pay period.
        /// Call generate first if no report exists yet.
        /// </summary>
        [HttpGet("payroll")]
        [ProducesResponseType(typeof(PayrollReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(
            [FromQuery] Guid payPeriodId, CancellationToken ct)
            => Ok(await _mediator.Send(
                new GetPayrollReportQuery(payPeriodId), ct));

        /// <summary>
        /// Generate or regenerate the payroll report for a pay period.
        /// Calculates regular hours, overtime, PTO, and exceptions
        /// for every active employee.
        /// </summary>
        [HttpPost("payroll/generate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PayrollReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Generate(
            [FromQuery] Guid payPeriodId, CancellationToken ct)
            => Ok(await _mediator.Send(
                new GeneratePayrollReportCommand(payPeriodId), ct));
    }
}
