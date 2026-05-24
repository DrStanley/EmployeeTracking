using EmployeeTracking.Application.Commands.CancelPTORequest;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class CancelPTOCommandHandler
    : IRequestHandler<CancelPTOCommand, PTORequestDto>
    {
        private readonly IUnitOfWork _uow;

        public CancelPTOCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<PTORequestDto> Handle(
            CancelPTOCommand request, CancellationToken ct)
        {
            var pto = await _uow.PTORequests.GetByIdAsync(request.RequestId, ct)
                ?? throw new NotFoundException(nameof(PTORequest), request.RequestId);

            if (pto.EmployeeId != request.EmployeeId)
                throw new UnauthorizedAccessException(
                    "You can only cancel your own PTO requests.");

            // If it was approved, restore the balance
            if (pto.Status == Domain.Enums.PTORequestStatus.Approved)
            {
                var balance = await _uow.PTOBalances
                    .GetByEmployeeAndYearAsync(
                        pto.EmployeeId, pto.StartDate.Year, ct);

                if (balance is not null)
                {
                    balance.Restore(pto.HoursRequested);
                    _uow.PTOBalances.Update(balance);
                }
            }

            pto.Cancel();
            _uow.PTORequests.Update(pto);
            await _uow.SaveChangesAsync(ct);

            var employee = await _uow.Employees.GetByIdAsync(pto.EmployeeId, ct)!;
            return PTOMapHelper.MapToDto(pto, employee!, null);
        }
    }
}
