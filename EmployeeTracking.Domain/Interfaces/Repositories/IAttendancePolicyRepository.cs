using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{
    public interface IAttendancePolicyRepository
    {
        Task<AttendancePolicy?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<AttendancePolicy>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(AttendancePolicy policy, CancellationToken ct = default);
        void Update(AttendancePolicy policy);
        void Delete(AttendancePolicy policy);
    }
}
