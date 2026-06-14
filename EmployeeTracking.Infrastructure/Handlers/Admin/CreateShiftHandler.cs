using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class CreateShiftHandler : IRequestHandler<CreateShiftCommand, ShiftDto>
    {
        private readonly IUnitOfWork _uow;
        public CreateShiftHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<ShiftDto> Handle(
            CreateShiftCommand request, CancellationToken ct)
        {
            var shift = Shift.Create(
                request.Request.Name,
                request.Request.PlannedStart,
                request.Request.PlannedEnd,
                request.Request.GracePeriodMinutes);

            await _uow.Shifts.AddAsync(shift, ct);
            await _uow.SaveChangesAsync(ct);

            return AdminMappers.MapShift(shift);
        }
    }
}
