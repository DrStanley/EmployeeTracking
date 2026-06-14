using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class CreatePayPeriodHandler
        : IRequestHandler<CreatePayPeriodCommand, PayPeriodDto>
    {
        private readonly IUnitOfWork _uow;
        public CreatePayPeriodHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<PayPeriodDto> Handle(
            CreatePayPeriodCommand request, CancellationToken ct)
        {
            var req = request.Request;

            if (req.EndDate <= req.StartDate)
                throw new DomainException("End date must be after start date.");

            var period = PayPeriod.Create(req.Name, req.StartDate, req.EndDate);

            await _uow.PayPeriods.AddAsync(period, ct);
            await _uow.SaveChangesAsync(ct);

            return AdminMappers.MapPayPeriod(period);
        }
    }
}
