using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class UpdateHolidayHandler : IRequestHandler<UpdateHolidayCommand, HolidayDto>
    {
        private readonly IUnitOfWork _uow;
        public UpdateHolidayHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<HolidayDto> Handle(
            UpdateHolidayCommand request, CancellationToken ct)
        {
            var holiday = await _uow.Holidays.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(Holiday), request.Id);

            holiday.Update(
                request.Request.Name,
                request.Request.Date,
                request.Request.IsRecurringAnnually);

            _uow.Holidays.Update(holiday);
            await _uow.SaveChangesAsync(ct);

            return AdminMappers.MapHoliday(holiday);
        }
    }
}
