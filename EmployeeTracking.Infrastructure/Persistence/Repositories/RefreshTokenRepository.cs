using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;

        public RefreshTokenRepository(AppDbContext context) => _context = context;

        public async Task<RefreshToken?> GetByTokenAsync(
            string token, CancellationToken ct = default)
            => await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token, ct);

        public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserAsync(
            string userId, CancellationToken ct = default)
            => await _context.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync(ct);

        public async Task AddAsync(
            RefreshToken token, CancellationToken ct = default)
            => await _context.RefreshTokens.AddAsync(token, ct);

        public void Update(RefreshToken token)
            => _context.RefreshTokens.Update(token);

        public async Task RevokeAllForUserAsync(
            string userId, string reason, CancellationToken ct = default)
        {
            var tokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ToListAsync(ct);

            foreach (var token in tokens)
                token.Revoke(reason);

            _context.RefreshTokens.UpdateRange(tokens);
        }
    }
}
