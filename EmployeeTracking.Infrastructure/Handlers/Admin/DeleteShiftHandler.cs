using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class DeleteShiftHandler : IRequestHandler<DeleteShiftCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public DeleteShiftHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Unit> Handle(
            DeleteShiftCommand request, CancellationToken ct)
        {
            var shift = await _uow.Shifts.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(Shift), request.Id);

            shift.Deactivate();
            _uow.Shifts.Update(shift);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
