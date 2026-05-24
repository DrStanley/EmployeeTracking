using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.PTO;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetPTOBalanceQueryHandler
        : IRequestHandler<GetPTOBalanceQuery, PTOBalanceDto>
    {
        private readonly IUnitOfWork _uow;

        public GetPTOBalanceQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<PTOBalanceDto> Handle(
            GetPTOBalanceQuery request, CancellationToken ct)
        {
            var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);

            var balance = await _uow.PTOBalances
                .GetByEmployeeAndYearAsync(request.EmployeeId, request.Year, ct)
                ?? throw new NotFoundException(
                    "PTOBalance",
                    $"{request.EmployeeId}/{request.Year}");

            return new PTOBalanceDto(
                EmployeeId: employee.Id,
                EmployeeFullName: employee.FullName,
                Year: balance.Year,
                AvailableHours: balance.AvailableHours,
                UsedHours: balance.UsedHours,
                AccruedHours: balance.AccruedHours);
        }
    }
}
