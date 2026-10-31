using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class ScreenCaptureRepository : IScreenCaptureRepository
    {
        private readonly AppDbContext _context;

        public ScreenCaptureRepository(AppDbContext context) => _context = context;

        public Task<ScreenCaptureConfig?> GetConfigAsync(CancellationToken ct = default)
            => _context.ScreenCaptureConfigs.FirstOrDefaultAsync(c => c.Id == ScreenCaptureConfig.DefaultId, ct);

        public async Task AddConfigAsync(ScreenCaptureConfig config, CancellationToken ct = default)
            => await _context.ScreenCaptureConfigs.AddAsync(config, ct);

        public async Task AddReviewAsync(ScreenReview review, CancellationToken ct = default)
            => await _context.ScreenReviews.AddAsync(review, ct);

        public async Task AddFrameAsync(ScreenCaptureFrame frame, CancellationToken ct = default)
            => await _context.ScreenCaptureFrames.AddAsync(frame, ct);

        public async Task<IReadOnlyList<ScreenCaptureFrame>> GetFramesAsync(
            Guid employeeId, DateTimeOffset since, DateTimeOffset until, CancellationToken ct = default)
            => await _context.ScreenCaptureFrames
                .Where(frame => frame.EmployeeId == employeeId && frame.CapturedAt >= since && frame.CapturedAt <= until)
                .OrderBy(frame => frame.CapturedAt)
                .ToListAsync(ct);

        public Task RemoveFramesAsync(IReadOnlyList<ScreenCaptureFrame> frames, CancellationToken ct = default)
        {
            _context.ScreenCaptureFrames.RemoveRange(frames);
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<ScreenReview>> GetRecentReviewsAsync(int take, CancellationToken ct = default)
            => await _context.ScreenReviews
                .OrderByDescending(r => r.EndedAt)
                .Take(take)
                .ToListAsync(ct);

        public async Task AddJobAsync(ScreenAnalysisJob job, CancellationToken ct = default)
            => await _context.ScreenAnalysisJobs.AddAsync(job, ct);

        public Task<ScreenAnalysisJob?> FindOpenJobAsync(Guid employeeId, DateTimeOffset startedAt, CancellationToken ct = default)
            => _context.ScreenAnalysisJobs.FirstOrDefaultAsync(job =>
                job.EmployeeId == employeeId
                && job.StartedAt == startedAt
                && job.Status != ScreenAnalysisStatus.Failed, ct);

        public async Task<ScreenAnalysisJob?> ClaimNextAsync(DateTimeOffset staleBefore, CancellationToken ct = default)
        {
            var job = await _context.ScreenAnalysisJobs
                .Where(item => item.Status == ScreenAnalysisStatus.Pending
                    || (item.Status == ScreenAnalysisStatus.Running && item.UpdatedAt < staleBefore))
                .OrderBy(item => item.CreatedAt)
                .FirstOrDefaultAsync(ct);
            job?.MarkRunning();
            return job;
        }

        public void RemoveJob(ScreenAnalysisJob job) => _context.ScreenAnalysisJobs.Remove(job);

        public async Task<IReadOnlyList<ScreenAnalysisJob>> GetVisibleJobsAsync(int take, CancellationToken ct = default)
            => await _context.ScreenAnalysisJobs
                .OrderByDescending(job => job.EndedAt)
                .Take(take)
                .ToListAsync(ct);
    }
}
