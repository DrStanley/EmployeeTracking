using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.Timesheets;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetTodayClockDetailsHandler
    : IRequestHandler<GetTodayClockDetails, IEnumerable<ClockTodayResponse>>
    {
        private readonly IUnitOfWork _uow;

        public GetTodayClockDetailsHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IEnumerable<ClockTodayResponse>> Handle(
            GetTodayClockDetails request, CancellationToken ct)
        {
            var from = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
            var to = from.AddDays(1).AddTicks(-1);

            var entries = (await _uow.TimeEntries
                .GetByEmployeeAndDateRangeAsync(request.EmployeeId, from, to, ct)).Select(e => new ClockTodayResponse(

                    e.Id,
                   e.EntryType,
                     e.Source,
                     e.Timestamp,
                    e.Notes
                ));

            return entries;
        }
    }
}
