using EmployeeTracking.Application.Commands.RejectPTORequest;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class RejectPTOCommandHandler
    : IRequestHandler<RejectPTOCommand, PTORequestDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailNotificationService _emailService;
        public RejectPTOCommandHandler(IUnitOfWork uow, IEmailNotificationService emailService)
        {
            _uow = uow;
            _emailService = emailService;
        }

        public async Task<PTORequestDto> Handle(
            RejectPTOCommand request, CancellationToken ct)
        {
            var pto = await _uow.PTORequests.GetByIdAsync(request.RequestId, ct)
                ?? throw new NotFoundException(nameof(PTORequest), request.RequestId);

            var employee = await _uow.Employees.GetByIdAsync(pto.EmployeeId, ct)!;
            if (employee!.ManagerId != request.ReviewerId)
                throw new UnauthorizedAccessException(
                    "Only the employee's assigned manager can reject PTO requests.");

            pto.Reject(request.ReviewerId, request.Reason);

            _uow.PTORequests.Update(pto);
            await _uow.SaveChangesAsync(ct);
            _ = _emailService.SendPTOApprovedAsync(pto, employee!, ct);
            var reviewer = await _uow.Employees.GetByIdAsync(request.ReviewerId, ct);
            return PTOMapHelper.MapToDto(pto, employee, reviewer);
        }
    }
}
