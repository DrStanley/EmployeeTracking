using EmployeeTracking.Application.Commands.ApprovePTORequest;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class ApprovePTOCommandHandler
      : IRequestHandler<ApprovePTOCommand, PTORequestDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailNotificationService _emailService;
        public ApprovePTOCommandHandler(IUnitOfWork uow, IEmailNotificationService emailService)
        {
            _uow = uow;
            _emailService = emailService;
        }

        public async Task<PTORequestDto> Handle(
            ApprovePTOCommand request, CancellationToken ct)
        {
            // 1. Load the PTO request
            var pto = await _uow.PTORequests.GetByIdAsync(request.RequestId, ct)
                ?? throw new NotFoundException(nameof(PTORequest), request.RequestId);

            // 2. Verify reviewer is the employee's manager
            var employee = await _uow.Employees.GetByIdAsync(pto.EmployeeId, ct)!;
            if (employee!.ManagerId != request.ReviewerId)
                throw new UnauthorizedAccessException(
                    "Only the employee's assigned manager can approve PTO requests.");

            // 3. Re-check balance at approval time
            var balance = await _uow.PTOBalances
                .GetByEmployeeAndYearAsync(
                    pto.EmployeeId, pto.StartDate.Year, ct)
                ?? throw new DomainException("PTO balance record not found.");

            if (pto.HoursRequested > balance.AvailableHours)
                throw new DomainException(
                    $"Insufficient PTO balance at time of approval. " +
                    $"Available: {balance.AvailableHours}h, " +
                    $"Requested: {pto.HoursRequested}h.");

            // 4. Approve and deduct balance
            pto.Approve(request.ReviewerId, request.Notes);
            balance.Deduct(pto.HoursRequested);

            _uow.PTORequests.Update(pto);
            _uow.PTOBalances.Update(balance);
            await _uow.SaveChangesAsync(ct);
            _ = _emailService.SendPTOApprovedAsync(pto, employee!, ct);
            var reviewer = await _uow.Employees.GetByIdAsync(request.ReviewerId, ct);
            return PTOMapHelper.MapToDto(pto, employee, reviewer);
        }
    }
}
