using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class GetAllPayPeriodsHandler
        : IRequestHandler<GetAllPayPeriodsQuery, IReadOnlyList<PayPeriodDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetAllPayPeriodsHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<PayPeriodDto>> Handle(
            GetAllPayPeriodsQuery request, CancellationToken ct)
            => (await _uow.PayPeriods.GetAllAsync(ct))
                .Select(AdminMappers.MapPayPeriod).ToList();
    }
}
