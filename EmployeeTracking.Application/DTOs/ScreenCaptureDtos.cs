namespace EmployeeTracking.Application.DTOs
{
    public record ScreenCaptureConfigDto(bool Enabled, int IntervalMinutes);

    public record UpdateScreenCaptureConfigRequest(bool Enabled, int IntervalMinutes);

    public record CompleteScreenReviewRequest(DateTimeOffset StartedAt, DateTimeOffset EndedAt);

    public record ScreenFrameDto(Guid Id, string StorageKey);

    public record CompleteScreenReviewResult(Guid Id, int FrameCount);

    public record ScreenReviewDto(
        Guid Id,
        Guid EmployeeId,
        string EmployeeName,
        DateTimeOffset StartedAt,
        DateTimeOffset EndedAt,
        int FrameCount,
        IReadOnlyList<string> Findings,
        string Status);
}
