using EmployeeTracking.Application.DTOs;
using MediatR;

namespace EmployeeTracking.Application.Queries.GetPayrollReport
{
    public record GeneratePayrollReportCommand(
        Guid PayPeriodId
    ) : IRequest<PayrollReportDto>;
}
