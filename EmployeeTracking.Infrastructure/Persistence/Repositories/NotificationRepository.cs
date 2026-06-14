using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context) => _context = context;

        public async Task<IReadOnlyList<Notification>> GetByEmployeeAsync(
            Guid employeeId, CancellationToken ct = default)
            => await _context.Notifications
                .Where(n => n.EmployeeId == employeeId)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Notification>> GetUnreadByEmployeeAsync(
            Guid employeeId, CancellationToken ct = default)
            => await _context.Notifications
                .Where(n => n.EmployeeId == employeeId && !n.IsRead)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync(ct);

        public async Task<Notification?> GetByIdAsync(
            Guid id, CancellationToken ct = default)
            => await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id, ct);

        public async Task AddAsync(
            Notification notification, CancellationToken ct = default)
            => await _context.Notifications.AddAsync(notification, ct);

        public void Update(Notification notification)
            => _context.Notifications.Update(notification);
    }
}
