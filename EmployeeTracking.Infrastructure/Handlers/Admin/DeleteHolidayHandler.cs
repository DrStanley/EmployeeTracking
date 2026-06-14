using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class DeleteHolidayHandler : IRequestHandler<DeleteHolidayCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public DeleteHolidayHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Unit> Handle(
            DeleteHolidayCommand request, CancellationToken ct)
        {
            var holiday = await _uow.Holidays.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(Holiday), request.Id);

            holiday.Deactivate();
            _uow.Holidays.Update(holiday);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
