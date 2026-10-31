using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{
    public interface IScreenCaptureRepository
    {
        Task<ScreenCaptureConfig?> GetConfigAsync(CancellationToken ct = default);
        Task AddConfigAsync(ScreenCaptureConfig config, CancellationToken ct = default);
        Task AddReviewAsync(ScreenReview review, CancellationToken ct = default);
        Task AddFrameAsync(ScreenCaptureFrame frame, CancellationToken ct = default);
        Task<IReadOnlyList<ScreenCaptureFrame>> GetFramesAsync(Guid employeeId, DateTimeOffset since, DateTimeOffset until, CancellationToken ct = default);
        Task RemoveFramesAsync(IReadOnlyList<ScreenCaptureFrame> frames, CancellationToken ct = default);
        Task<IReadOnlyList<ScreenReview>> GetRecentReviewsAsync(int take, CancellationToken ct = default);
        Task AddJobAsync(ScreenAnalysisJob job, CancellationToken ct = default);
        Task<ScreenAnalysisJob?> FindOpenJobAsync(Guid employeeId, DateTimeOffset startedAt, CancellationToken ct = default);
        Task<ScreenAnalysisJob?> ClaimNextAsync(DateTimeOffset staleBefore, CancellationToken ct = default);
        void RemoveJob(ScreenAnalysisJob job);
        Task<IReadOnlyList<ScreenAnalysisJob>> GetVisibleJobsAsync(int take, CancellationToken ct = default);
    }
}
