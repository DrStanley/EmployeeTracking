using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{

    public interface IPTORepository
    {
        Task<PTORequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<PTORequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);
        Task<IReadOnlyList<PTORequest>> GetPendingByManagerAsync(Guid managerId, CancellationToken ct = default);
        Task<bool> HasOverlappingRequestAsync(
            Guid employeeId, DateOnly start, DateOnly end, CancellationToken ct = default);
        Task AddAsync(PTORequest request, CancellationToken ct = default);
        void Update(PTORequest request);
    }
}
