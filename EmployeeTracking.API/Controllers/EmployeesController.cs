using EmployeeTracking.Application.Commands.Employees;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EmployeeTracking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmployeesController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [FromBody] CreateEmployeeRequest requuest, CancellationToken ct)
            => Ok(await _mediator.Send(new CreateEmployeeCommand(
                requuest.EmployeeNumber,
                requuest.FirstName,
                requuest.LastName,
                requuest.Email,
                requuest.JobTitle,
                requuest.DepartmentId,
                requuest.AttendancePolicyId,
                requuest.EmploymentType,
                User.FindFirstValue(ClaimTypes.Email) ?? throw new UnauthorizedAccessException("Email claim missing from token."),
                requuest.ReferredByEmail
            ), ct));
    }
}
