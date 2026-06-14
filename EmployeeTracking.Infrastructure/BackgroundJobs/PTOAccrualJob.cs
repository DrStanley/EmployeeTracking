using EmployeeTracking.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmployeeTracking.Infrastructure.BackgroundJobs
{

    /// <summary>
    /// Runs on the first day of each month and accrues PTO
    /// for all active employees based on their attendance policy.
    /// </summary>
    public class PTOAccrualJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PTOAccrualJob> _logger;

        public PTOAccrualJob(
            IServiceScopeFactory scopeFactory,
            ILogger<PTOAccrualJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("PTOAccrualJob started.");

            while (!ct.IsCancellationRequested)
            {
                var now = DateTime.Today;
                var nextRun = new DateTime(
                    now.Year, now.Month, 1).AddMonths(1);
                var delay = nextRun - now;

                _logger.LogInformation(
                    "PTOAccrualJob next run at {NextRun}", nextRun);

                await Task.Delay(delay, ct);

                try
                {
                    await RunAsync(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PTOAccrualJob failed.");
                }
            }
        }

        private async Task RunAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var employees = await uow.Employees.GetAllAsync(ct);
            var year = DateTime.Now.Year;
            var accrued = 0;

            foreach (var employee in employees)
            {
                var accrualRate = employee.AttendancePolicy?
                    .PTOAccrualRatePerPayPeriod ?? 4m;

                var balance = await uow.PTOBalances
                    .GetByEmployeeAndYearAsync(employee.Id, year, ct);

                if (balance is null)
                {
                    // Create a balance record if it doesn't exist yet
                    balance = Domain.Entities.PTOBalance
                        .CreateForYear(employee.Id, year);
                    await uow.PTOBalances.AddAsync(balance, ct);
                }

                balance.Accrue(accrualRate);
                uow.PTOBalances.Update(balance);
                accrued++;
            }

            await uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "PTOAccrualJob completed — accrued for {Count} employees.", accrued);
        }
    }
}
