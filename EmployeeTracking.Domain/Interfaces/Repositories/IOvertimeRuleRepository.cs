using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{
    public interface IOvertimeRuleRepository
    {
        Task<OvertimeRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<OvertimeRule>> GetAllAsync(CancellationToken ct = default);
        Task<OvertimeRule?> GetByPolicyAsync(
            Guid policyId, CancellationToken ct = default);
        Task AddAsync(OvertimeRule rule, CancellationToken ct = default);
        void Update(OvertimeRule rule);
        void Delete(OvertimeRule rule);
    }
}
