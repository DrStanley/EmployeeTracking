namespace EmployeeTracking.Application.ScreenCapture
{
    public record StoredScreenImage(string StorageKey, DateTimeOffset CapturedAt);

    public record ScreenAiRequest(
        Guid EmployeeId,
        string EmployeeName,
        DateTimeOffset StartedAt,
        DateTimeOffset EndedAt,
        IReadOnlyList<StoredScreenImage> Images);

    public interface IScreenAiAnalyzer
    {
        Task<IReadOnlyList<string>> AnalyzeAsync(ScreenAiRequest request, CancellationToken ct = default);
    }
}
