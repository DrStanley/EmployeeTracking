using EmployeeTracking.API.Extensions;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Queries.Workspace;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeTracking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmployeesController(IMediator mediator) => _mediator = mediator;

        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
            => Ok(await _mediator.Send(new GetEmployeeProfileQuery(User.GetEmployeeId()), ct));

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe(
            [FromBody] UpdatePersonalDetailsRequest request, CancellationToken ct)
            => Ok(await _mediator.Send(new UpdatePersonalDetailsCommand(
                User.GetEmployeeId(),
                request.FirstName,
                request.LastName,
                request.Gender,
                request.DateOfBirth,
                request.MaritalStatus,
                request.Phone,
                request.AddressLine,
                request.City,
                request.State,
                request.PostalCode,
                request.Country), ct));

        [HttpGet("schedule")]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> Schedule(CancellationToken ct)
            => Ok(await _mediator.Send(new GetScheduleQuery(User.GetEmployeeId()), ct));
    }
}
