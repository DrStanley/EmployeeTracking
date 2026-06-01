using EmployeeTracking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{
    public interface IPayrollReportRepository
    {
        Task<IReadOnlyList<PayrollReport>> GetByPayPeriodAsync(
            Guid payPeriodId, CancellationToken ct = default);
        Task<PayrollReport?> GetByEmployeeAndPeriodAsync(
            Guid employeeId, Guid payPeriodId, CancellationToken ct = default);
        Task AddAsync(PayrollReport report, CancellationToken ct = default);
        void Update(PayrollReport report);
    }
}
