using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{
    public interface IPayPeriodRepository
    {
        Task<PayPeriod?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PayPeriod?> GetCurrentAsync(CancellationToken ct = default);
        Task<IReadOnlyList<PayPeriod>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(PayPeriod payPeriod, CancellationToken ct = default);
        void Update(PayPeriod payPeriod);
    }
}
