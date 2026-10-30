namespace EmployeeTracking.Application.DTOs
{
    public record AppUsageItemDto(
        string AppName,
        decimal Hours,
        int SessionCount,
        DateTimeOffset LastUsedAt,
        decimal Percent);

    public record AppUsageSessionDto(
        Guid Id,
        string AppName,
        DateTimeOffset StartedAt,
        DateTimeOffset EndedAt,
        decimal Hours,
        string Source);

    public record AppActivityReportDto(
        Guid EmployeeId,
        string EmployeeName,
        DateTimeOffset From,
        DateTimeOffset To,
        decimal TotalHours,
        int AppCount,
        int SessionCount,
        IReadOnlyList<AppUsageItemDto> Apps,
        IReadOnlyList<AppUsageSessionDto> Sessions);

    public record UserActivitySummaryDto(
        Guid EmployeeId,
        string EmployeeName,
        string JobTitle,
        decimal TotalHours,
        int AppCount,
        string? TopApp,
        decimal TopAppHours);

    public record RecordAppUsageRequest(
        string AppName,
        DateTimeOffset StartedAt,
        DateTimeOffset EndedAt,
        string? Source);

    public record RecordAppUsageResult(Guid Id, decimal Hours);
}
