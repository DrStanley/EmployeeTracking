using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Full payroll report for a pay period.</summary>
    public record PayrollReportDto(
        Guid PayPeriodId,
        string PayPeriodName,
        DateOnly StartDate,
        DateOnly EndDate,
        IReadOnlyList<PayrollLineDto> Lines,
        decimal TotalRegularHours,
        decimal TotalOvertimeHours,
        decimal TotalPTOHours,
        decimal TotalUnpaidHours,
        int TotalEmployees,
        int TotalExceptions,
        DateTimeOffset GeneratedAt
    );
}
