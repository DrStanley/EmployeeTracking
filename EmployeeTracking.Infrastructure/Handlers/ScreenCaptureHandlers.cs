using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.ScreenCapture;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetScreenCaptureConfigHandler : IRequestHandler<GetScreenCaptureConfigQuery, ScreenCaptureConfigDto>
    {
        private readonly IUnitOfWork _uow;
        public GetScreenCaptureConfigHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<ScreenCaptureConfigDto> Handle(GetScreenCaptureConfigQuery request, CancellationToken ct)
        {
            var config = await EnsureConfig(ct);
            return new ScreenCaptureConfigDto(config.Enabled, config.IntervalMinutes);
        }

        private async Task<ScreenCaptureConfig> EnsureConfig(CancellationToken ct)
        {
            var config = await _uow.ScreenCapture.GetConfigAsync(ct);
            if (config != null) return config;
            config = ScreenCaptureConfig.CreateDefault();
            await _uow.ScreenCapture.AddConfigAsync(config, ct);
            await _uow.SaveChangesAsync(ct);
            return config;
        }
    }

    public class UpdateScreenCaptureConfigHandler : IRequestHandler<UpdateScreenCaptureConfigCommand, ScreenCaptureConfigDto>
    {
        private readonly IUnitOfWork _uow;
        public UpdateScreenCaptureConfigHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<ScreenCaptureConfigDto> Handle(UpdateScreenCaptureConfigCommand request, CancellationToken ct)
        {
            var config = await _uow.ScreenCapture.GetConfigAsync(ct);
            if (config == null)
            {
                config = ScreenCaptureConfig.CreateDefault();
                config.Update(request.Enabled, request.IntervalMinutes);
                await _uow.ScreenCapture.AddConfigAsync(config, ct);
            }
            else
            {
                config.Update(request.Enabled, request.IntervalMinutes);
            }

            await _uow.SaveChangesAsync(ct);
            return new ScreenCaptureConfigDto(config.Enabled, config.IntervalMinutes);
        }
    }

    public class UploadScreenFrameHandler : IRequestHandler<UploadScreenFrameCommand, ScreenFrameDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IScreenImageStorage _storage;

        public UploadScreenFrameHandler(IUnitOfWork uow, IScreenImageStorage storage)
        {
            _uow = uow;
            _storage = storage;
        }

        public async Task<ScreenFrameDto> Handle(UploadScreenFrameCommand request, CancellationToken ct)
        {
            var key = await _storage.SaveAsync(request.EmployeeId, request.Content, request.ContentType, ct);
            var frame = ScreenCaptureFrame.Record(request.EmployeeId, request.CapturedAt, key);
            await _uow.ScreenCapture.AddFrameAsync(frame, ct);
            await _uow.SaveChangesAsync(ct);
            return new ScreenFrameDto(frame.Id, key);
        }
    }

    public class CompleteScreenReviewHandler : IRequestHandler<CompleteScreenReviewCommand, CompleteScreenReviewResult>
    {
        private readonly IUnitOfWork _uow;

        public CompleteScreenReviewHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<CompleteScreenReviewResult> Handle(CompleteScreenReviewCommand request, CancellationToken ct)
        {
            var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);

            var existing = await _uow.ScreenCapture.FindOpenJobAsync(employee.Id, request.StartedAt, ct);
            var frames = await _uow.ScreenCapture.GetFramesAsync(employee.Id, request.StartedAt, request.EndedAt, ct);
            if (existing != null)
                return new CompleteScreenReviewResult(existing.Id, frames.Count);

            var job = ScreenAnalysisJob.Schedule(
                employee.Id,
                employee.FullName,
                request.StartedAt,
                request.EndedAt,
                frames.Count);
            await _uow.ScreenCapture.AddJobAsync(job, ct);
            await _uow.SaveChangesAsync(ct);
            return new CompleteScreenReviewResult(job.Id, job.FrameCount);
        }
    }

    public class GetScreenReviewsHandler : IRequestHandler<GetScreenReviewsQuery, IReadOnlyList<ScreenReviewDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetScreenReviewsHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<ScreenReviewDto>> Handle(GetScreenReviewsQuery request, CancellationToken ct)
        {
            var reviews = await _uow.ScreenCapture.GetRecentReviewsAsync(50, ct);
            var jobs = await _uow.ScreenCapture.GetVisibleJobsAsync(50, ct);
            var ready = reviews.Select(review => new ScreenReviewDto(
                review.Id,
                review.EmployeeId,
                review.EmployeeName,
                review.StartedAt,
                review.EndedAt,
                review.FrameCount,
                review.FindingLines(),
                "Ready"));
            var waiting = jobs.Select(job => new ScreenReviewDto(
                job.Id,
                job.EmployeeId,
                job.EmployeeName,
                job.StartedAt,
                job.EndedAt,
                job.FrameCount,
                Array.Empty<string>(),
                job.Status == ScreenAnalysisStatus.Failed ? "Failed" : "Running"));
            return ready.Concat(waiting).OrderByDescending(item => item.EndedAt).Take(50).ToList();
        }
    }
}
