using EmployeeTracking.Application.Commands.SubmitPTORequest;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.ValueObjects;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class SubmitPTOCommandHandler
     : IRequestHandler<SubmitPTOCommand, PTORequestDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IEmailNotificationService _emailService;
        public SubmitPTOCommandHandler(IUnitOfWork uow, IEmailNotificationService emailService)
        {
            _uow = uow;
            _emailService = emailService;
        }

        public async Task<PTORequestDto> Handle(
            SubmitPTOCommand request, CancellationToken ct)
        {
            // Verify employee exists
            var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);

            // Check PTO balance
            var balance = await _uow.PTOBalances
                .GetByEmployeeAndYearAsync(
                    request.EmployeeId, request.StartDate.Year, ct);

            if (balance is null)
                throw new DomainException(
                    $"No PTO balance found for year {request.StartDate.Year}. " +
                    "Please contact your administrator.");

            if (request.HoursRequested > balance.AvailableHours)
                throw new DomainException(
                    $"Insufficient PTO balance. " +
                    $"Available: {balance.AvailableHours}h, " +
                    $"Requested: {request.HoursRequested}h.");

            // Check for overlapping approved or pending requests
            var hasOverlap = await _uow.PTORequests.HasOverlappingRequestAsync(
                request.EmployeeId,
                request.StartDate,
                request.EndDate, ct);

            if (hasOverlap)
                throw new DomainException(
                    "You already have a PTO request overlapping these dates.");

            // Create and save the request
            var dateRange = new DateRange(request.StartDate, request.EndDate);
            var pto = PTORequest.Create(
                request.EmployeeId,
                dateRange,
                request.HoursRequested,
                request.Notes);

            await _uow.PTORequests.AddAsync(pto, ct);
            await _uow.SaveChangesAsync(ct);
            var employeeManager = await _uow.Employees.GetByIdAsync(employee.ManagerId.Value, ct)?? throw new DomainException("No manager assigned to Employee");
            _ = _emailService.SendPTORequestSubmittedAsync(pto, employee!, employeeManager,ct);
            return PTOMapHelper.MapToDto(pto, employee, null);
        }
    }
}
