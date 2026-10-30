using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.Timesheets;
using EmployeeTracking.Application.Time;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetClockStatusHandler
    : IRequestHandler<GetClockStatus, ClockStatusDto>
    {
        private readonly IUnitOfWork _uow;

        public GetClockStatusHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<ClockStatusDto> Handle(
            GetClockStatus request, CancellationToken ct)
        {
            var entries = await _uow.TimeEntries.GetByEmployeeAsync(request.EmployeeId, ct);
            return ClockStateReader.Read(entries);
        }
    }
}
