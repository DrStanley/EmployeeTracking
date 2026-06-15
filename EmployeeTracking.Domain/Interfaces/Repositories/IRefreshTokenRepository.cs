using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(
            string token, CancellationToken ct = default);
        Task<IReadOnlyList<RefreshToken>> GetActiveByUserAsync(
            string userId, CancellationToken ct = default);
        Task AddAsync(RefreshToken token, CancellationToken ct = default);
        void Update(RefreshToken token);
        Task RevokeAllForUserAsync(
            string userId, string reason, CancellationToken ct = default);
    }
}
