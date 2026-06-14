using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class LockPayPeriodHandler
        : IRequestHandler<LockPayPeriodCommand, PayPeriodDto>
    {
        private readonly IUnitOfWork _uow;
        public LockPayPeriodHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<PayPeriodDto> Handle(
            LockPayPeriodCommand request, CancellationToken ct)
        {
            var period = await _uow.PayPeriods.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(PayPeriod), request.Id);

            if (period.IsLocked)
                throw new DomainException("This pay period is already locked.");

            period.Lock();
            _uow.PayPeriods.Update(period);
            await _uow.SaveChangesAsync(ct);

            return AdminMappers.MapPayPeriod(period);
        }
    }
}
