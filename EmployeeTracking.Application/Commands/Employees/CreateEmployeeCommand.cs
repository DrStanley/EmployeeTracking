using EmployeeTracking.Domain.Enums;
using MediatR;

namespace EmployeeTracking.Application.Commands.Employees
{
    public record CreateEmployeeCommand(
     string EmployeeNumber,
     string FirstName,
     string LastName,
     string Email,
     string JobTitle,
     Guid DepartmentId,
     Guid ManagerId,
     Guid AttendancePolicyId,
     EmploymentType EmploymentType,
     string CreatedByEmail,
     string? ReferredByEmail
 ) : IRequest<CreateEmployeeResponse>;

    public record CreateEmployeeRequest(
      string EmployeeNumber,
     string FirstName,
     string LastName,
     string Email,
     string JobTitle,
     Guid DepartmentId,
     Guid ManagerId,
     Guid AttendancePolicyId,
     EmploymentType EmploymentType,
     string? ReferredByEmail
   );
    public record CreateEmployeeResponse(
        Guid EmployeeId,
        string FullName,
        string EmployeeNumber,
        string? ReferredBy
    );
}
