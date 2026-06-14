using EmployeeTracking.API.Extensions;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Queries.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeTracking.API.Controllers
{
    /// <summary>Manage notifications for the authenticated employee.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator) => _mediator = mediator;

        /// <summary>Get all notifications for the authenticated employee.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>),
            StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool unreadOnly = false, CancellationToken ct = default)
            => Ok(await _mediator.Send(
                new GetMyNotificationsQuery(User.GetEmployeeId(), unreadOnly), ct));

        /// <summary>Mark a single notification as read.</summary>
        [HttpPost("{id}/read")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
        {
            await _mediator.Send(
                new MarkNotificationReadCommand(id, User.GetEmployeeId()), ct);
            return NoContent();
        }

        /// <summary>Mark all notifications as read for authenticated user.</summary>
        [HttpPost("read-all")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> MarkAllRead(CancellationToken ct)
        {
            await _mediator.Send(
                new MarkAllNotificationsReadCommand(User.GetEmployeeId()), ct);
            return NoContent();
        }
    }
}
