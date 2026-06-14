using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class UpdateShiftHandler : IRequestHandler<UpdateShiftCommand, ShiftDto>
    {
        private readonly IUnitOfWork _uow;
        public UpdateShiftHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<ShiftDto> Handle(
            UpdateShiftCommand request, CancellationToken ct)
        {
            var shift = await _uow.Shifts.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(Shift), request.Id);

            shift.Update(
                request.Request.Name,
                request.Request.PlannedStart,
                request.Request.PlannedEnd,
                request.Request.GracePeriodMinutes);

            if (!request.Request.IsActive)
                shift.Deactivate();

            _uow.Shifts.Update(shift);
            await _uow.SaveChangesAsync(ct);

            return AdminMappers.MapShift(shift);
        }
    }
}
