using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.DomainEvents;
using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Domain.Entities
{
    public class Employee : AuditableEntity
    {
        public Guid Id { get; private set; }
        public string EmployeeNumber { get; private set; } = string.Empty;
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string JobTitle { get; private set; } = string.Empty;
        public string? Location { get; private set; }
        public Guid DepartmentId { get; private set; }
        public Guid? ManagerId { get; private set; }
        public Guid? ShiftId { get; private set; }
        public Guid AttendancePolicyId { get; private set; }
        public EmploymentType EmploymentType { get; private set; }
        public bool IsActive { get; private set; } = true;

        public string? CreatedByUserId { get; private set; }

        public Guid? ReferredByEmployeeId { get; private set; }

        public Department Department { get; private set; } = null!;
        public Employee? Manager { get; private set; }
        public Shift? Shift { get; private set; }
        public AttendancePolicy AttendancePolicy { get; private set; } = null!;
        public Employee? ReferredByEmployee { get; private set; }

        private Employee() { }

        public static Employee Create(
            string employeeNumber,
            string firstName,
            string lastName,
            string email,
            string jobTitle,
            Guid departmentId,
            Guid attendancePolicyId,
            EmploymentType employmentType,
            string? createdByUserId = null,
            Guid? referredByEmployeeId = null)
        {
            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                EmployeeNumber = employeeNumber,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                JobTitle = jobTitle,
                DepartmentId = departmentId,
                AttendancePolicyId = attendancePolicyId,
                EmploymentType = employmentType,
                CreatedByUserId = createdByUserId,
                ReferredByEmployeeId = referredByEmployeeId,
                IsActive = true
            };

            employee.AddDomainEvent(new EmployeeCreatedEvent(employee.Id));
            return employee;
        }

        public void AssignManager(Guid managerId) => ManagerId = managerId;
        public void AssignShift(Guid shiftId) => ShiftId = shiftId;
        public void UpdateLocation(string location) => Location = location;
        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;

        public string FullName => $"{FirstName} {LastName}";
    }
}
