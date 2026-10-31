using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.ScreenCapture;
using EmployeeTracking.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmployeeTracking.Infrastructure.BackgroundJobs
{
    public class ScreenAnalysisWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ScreenAnalysisWorker> _logger;

        public ScreenAnalysisWorker(IServiceScopeFactory scopeFactory, ILogger<ScreenAnalysisWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var worked = false;
                try
                {
                    worked = await ProcessOneAsync(ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Screen analysis worker failed.");
                }

                if (!worked)
                    await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
        }

        private async Task<bool> ProcessOneAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var analyzer = scope.ServiceProvider.GetRequiredService<IScreenAiAnalyzer>();
            var job = await uow.ScreenCapture.ClaimNextAsync(DateTimeOffset.UtcNow.AddMinutes(-15), ct);
            if (job == null) return false;
            await uow.SaveChangesAsync(ct);

            try
            {
                var employee = await uow.Employees.GetByIdAsync(job.EmployeeId, ct)
                    ?? throw new Domain.Common.NotFoundException(nameof(Employee), job.EmployeeId);
                var frames = await uow.ScreenCapture.GetFramesAsync(job.EmployeeId, job.StartedAt, job.EndedAt, ct);
                var findings = await analyzer.AnalyzeAsync(new ScreenAiRequest(
                    employee.Id,
                    employee.FullName,
                    job.StartedAt,
                    job.EndedAt,
                    frames.Select(frame => new StoredScreenImage(frame.StorageKey, frame.CapturedAt)).ToList()), ct);

                var review = ScreenReview.Record(
                    employee.Id,
                    employee.FullName,
                    job.StartedAt,
                    job.EndedAt,
                    frames.Count,
                    findings);

                await uow.ScreenCapture.AddReviewAsync(review, ct);
                await uow.ScreenCapture.RemoveFramesAsync(frames, ct);
                uow.ScreenCapture.RemoveJob(job);
                await uow.SaveChangesAsync(ct);
                _logger.LogInformation("Screen analysis {JobId} saved a review.", job.Id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Screen analysis {JobId} failed.", job.Id);
                job.MarkForRetry(ex.Message);
                await uow.SaveChangesAsync(ct);
                return false;
            }

            return true;
        }
    }
}
