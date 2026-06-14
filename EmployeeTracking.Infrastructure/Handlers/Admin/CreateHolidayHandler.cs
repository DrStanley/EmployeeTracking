using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class CreateHolidayHandler : IRequestHandler<CreateHolidayCommand, HolidayDto>
    {
        private readonly IUnitOfWork _uow;
        public CreateHolidayHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<HolidayDto> Handle(
            CreateHolidayCommand request, CancellationToken ct)
        {
            var holiday = Holiday.Create(
                request.Request.Name,
                request.Request.Date,
                request.Request.IsRecurringAnnually);

            await _uow.Holidays.AddAsync(holiday, ct);
            await _uow.SaveChangesAsync(ct);

            return AdminMappers.MapHoliday(holiday);
        }
    }
}
