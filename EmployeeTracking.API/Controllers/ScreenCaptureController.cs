using EmployeeTracking.API.Extensions;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.ScreenCapture;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeTracking.API.Controllers
{
    [ApiController]
    [Route("api/screen-capture")]
    [Authorize]
    public class ScreenCaptureController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ScreenCaptureController(IMediator mediator) => _mediator = mediator;

        [HttpGet("config")]
        public async Task<IActionResult> Config(CancellationToken ct)
            => Ok(await _mediator.Send(new GetScreenCaptureConfigQuery(), ct));

        [HttpPut("config")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateConfig(
            [FromBody] UpdateScreenCaptureConfigRequest request, CancellationToken ct)
            => Ok(await _mediator.Send(
                new UpdateScreenCaptureConfigCommand(request.Enabled, request.IntervalMinutes), ct));

        [HttpPost("frames")]
        [RequestSizeLimit(15_000_000)]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> UploadFrame(IFormFile image, [FromForm] string? capturedAt, CancellationToken ct)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new ProblemDetails { Title = "Validation failed", Detail = "A screen image is required." });

            var when = DateTimeOffset.TryParse(capturedAt, out var parsed) ? parsed : DateTimeOffset.UtcNow;
            await using var content = image.OpenReadStream();
            return Ok(await _mediator.Send(new UploadScreenFrameCommand(
                User.GetEmployeeId(),
                when,
                content,
                image.ContentType ?? "image/jpeg"), ct));
        }

        [HttpPost("reviews")]
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> Complete(
            [FromBody] CompleteScreenReviewRequest request, CancellationToken ct)
            => Ok(await _mediator.Send(new CompleteScreenReviewCommand(
                User.GetEmployeeId(),
                request.StartedAt,
                request.EndedAt), ct));

        [HttpGet("reviews")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> Reviews(CancellationToken ct)
            => Ok(await _mediator.Send(new GetScreenReviewsQuery(), ct));
    }
}
