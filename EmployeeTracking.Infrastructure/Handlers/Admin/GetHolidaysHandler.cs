using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class GetHolidaysHandler
        : IRequestHandler<GetHolidaysQuery, IReadOnlyList<HolidayDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetHolidaysHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<HolidayDto>> Handle(
            GetHolidaysQuery request, CancellationToken ct)
            => (await _uow.Holidays.GetByYearAsync(request.Year, ct))
                .Select(AdminMappers.MapHoliday).ToList();
    }
}
