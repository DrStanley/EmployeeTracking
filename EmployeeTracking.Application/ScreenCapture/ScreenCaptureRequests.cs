using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.ScreenCapture
{
    public record GetScreenCaptureConfigQuery() : IRequest<ScreenCaptureConfigDto>;

    public record UpdateScreenCaptureConfigCommand(bool Enabled, int IntervalMinutes) : IRequest<ScreenCaptureConfigDto>;

    public record UploadScreenFrameCommand(
        Guid EmployeeId,
        DateTimeOffset CapturedAt,
        Stream Content,
        string ContentType) : IRequest<ScreenFrameDto>;

    public record CompleteScreenReviewCommand(
        Guid EmployeeId,
        DateTimeOffset StartedAt,
        DateTimeOffset EndedAt) : IRequest<CompleteScreenReviewResult>;

    public record GetScreenReviewsQuery() : IRequest<IReadOnlyList<ScreenReviewDto>>;
}
