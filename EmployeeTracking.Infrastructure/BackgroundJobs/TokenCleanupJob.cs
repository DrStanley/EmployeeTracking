using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmployeeTracking.Infrastructure.BackgroundJobs
{
    /// <summary>
    /// Runs daily and removes expired or revoked refresh tokens
    /// older than 20 days to keep the table clean.
    /// </summary>
    public class TokenCleanupJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TokenCleanupJob> _logger;

        public TokenCleanupJob(
            IServiceScopeFactory scopeFactory,
            ILogger<TokenCleanupJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                // Run once per day at midnight UTC
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1);
                await Task.Delay(nextRun - now, ct);

                try
                {
                    await RunAsync(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "TokenCleanupJob failed.");
                }
            }
        }

        private async Task RunAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                .GetRequiredService<Infrastructure.Persistence.AppDbContext>();

            var cutoff = DateTimeOffset.UtcNow.AddDays(-20);
            var deleted = await context.RefreshTokens
                .Where(t => (t.IsRevoked || t.ExpiresAt < DateTimeOffset.UtcNow)
                         && t.CreatedAt < cutoff)
                .ExecuteDeleteAsync(ct);

            _logger.LogInformation(
                "TokenCleanupJob removed {Count} expired/revoked tokens.", deleted);
        }
    }
}
