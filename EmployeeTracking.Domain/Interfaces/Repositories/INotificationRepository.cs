using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{
    public interface INotificationRepository
    {
        Task<IReadOnlyList<Notification>> GetByEmployeeAsync(
            Guid employeeId, CancellationToken ct = default);
        Task<IReadOnlyList<Notification>> GetUnreadByEmployeeAsync(
            Guid employeeId, CancellationToken ct = default);
        Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Notification>> GetByTypeSinceAsync(
            NotificationType type, DateTimeOffset since, CancellationToken ct = default);
        Task AddAsync(Notification notification, CancellationToken ct = default);
        void Update(Notification notification);
    }
}
