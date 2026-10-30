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

        [HttpGet("schedule")]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> Schedule(CancellationToken ct)
            => Ok(await _mediator.Send(new GetScheduleQuery(User.GetEmployeeId()), ct));
    }
}
