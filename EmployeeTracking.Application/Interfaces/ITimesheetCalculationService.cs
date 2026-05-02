using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Application.Interfaces
{
    public interface ITimesheetCalculationService
    {
        Task<(decimal regular, decimal overtime, decimal pto, decimal unpaid)>
            CalculateTotalsAsync(
                Guid employeeId,
                PayPeriod period,
                CancellationToken ct = default);
    }
}
