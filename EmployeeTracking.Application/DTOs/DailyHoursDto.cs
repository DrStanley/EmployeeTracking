namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Daily hours breakdown used for overtime calculation.</summary>
    public record DailyHoursDto(
        DateOnly Date,
        decimal WorkedHours
    );
}
