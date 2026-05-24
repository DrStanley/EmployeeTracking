using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Infrastructure.Handlers
{
    internal static class PTOMapHelper
    {
        internal static PTORequestDto MapToDto(
            PTORequest pto,
            Employee employee,
            Employee? reviewer) =>
            new(
                Id: pto.Id,
                EmployeeId: pto.EmployeeId,
                EmployeeFullName: employee.FullName,
                StartDate: pto.StartDate,
                EndDate: pto.EndDate,
                HoursRequested: pto.HoursRequested,
                Status: pto.Status,
                Notes: pto.Notes,
                ReviewerNotes: pto.ReviewerNotes,
                ReviewedByFullName: reviewer?.FullName,
                ReviewedAt: pto.ReviewedAt,
                CreatedAt: pto.CreatedAt);
    }
}
