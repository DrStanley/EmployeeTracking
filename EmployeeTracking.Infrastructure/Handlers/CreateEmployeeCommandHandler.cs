using EmployeeTracking.Application.Commands.Employees;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class CreateEmployeeCommandHandler
     : IRequestHandler<CreateEmployeeCommand, CreateEmployeeResponse>
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateEmployeeCommandHandler(
            IUnitOfWork uow,
            UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
        }

        public async Task<CreateEmployeeResponse> Handle(
            CreateEmployeeCommand request, CancellationToken ct)
        {
            // 1. No duplicate employee number
            var all = await _uow.Employees.GetAllAsync(ct);
            if (all.Any(e => e.EmployeeNumber == request.EmployeeNumber))
                throw new DomainException(
                    $"Employee number '{request.EmployeeNumber}' is already in use.");

            // 2. No duplicate email
            var existingEmail = await _uow.Employees.GetByEmailAsync(request.Email, ct);
            if (existingEmail is not null)
                throw new DomainException(
                    $"An employee with email '{request.Email}' already exists.");

            // 3. Resolve referral by email
            Employee? referrer = null;
            if (!string.IsNullOrWhiteSpace(request.ReferredByEmail))
            {
                referrer = await _uow.Employees.GetByEmailAsync(request.ReferredByEmail, ct)
                    ?? throw new NotFoundException(
                        "Referral employee", request.ReferredByEmail);

                if (referrer.Email == request.Email)
                    throw new DomainException("An employee cannot refer themselves.");
            }

            // 4. Resolve the Identity user who is creating this record
            var createdByUser = await _userManager.FindByEmailAsync(request.CreatedByEmail) ?? throw new NotFoundException("User", request.CreatedByEmail);

            // 5. Create the domain entity
            var employee = Employee.Create(
                request.EmployeeNumber,
                request.FirstName,
                request.LastName,
                request.Email,
                request.JobTitle,
                request.DepartmentId,
                request.AttendancePolicyId,
                request.EmploymentType,
                createdByUser.Id,
                referrer?.Id);

            await _uow.Employees.AddAsync(employee, ct);
            await _uow.SaveChangesAsync(ct);

            var createdUser = await _userManager.FindByEmailAsync(request.Email);
            // 6. Link the Identity user to the new employee record if it exists
            if (createdUser is not null)
            {
                createdUser.EmployeeId = employee.Id;
                await _userManager.UpdateAsync(createdUser);
            }

            return new CreateEmployeeResponse(
                EmployeeId: employee.Id,
                FullName: employee.FullName,
                EmployeeNumber: employee.EmployeeNumber,
                ReferredBy: referrer?.FullName);
        }
    }
}
