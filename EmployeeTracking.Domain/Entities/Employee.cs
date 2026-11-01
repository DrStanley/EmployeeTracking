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
        public Gender Gender { get; private set; } = Gender.Unspecified;
        public DateOnly? DateOfBirth { get; private set; }
        public MaritalStatus MaritalStatus { get; private set; } = MaritalStatus.Unspecified;
        public string? Phone { get; private set; }
        public string? AddressLine { get; private set; }
        public string? City { get; private set; }
        public string? State { get; private set; }
        public string? PostalCode { get; private set; }
        public string? Country { get; private set; }
        public bool IsActive { get; private set; } = true;
        public AccountStatus AccountStatus { get; private set; } = AccountStatus.Active;

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
            Guid managerId,
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
                ManagerId = managerId == Guid.Empty ? null : managerId,
                AttendancePolicyId = attendancePolicyId,
                EmploymentType = employmentType,
                CreatedByUserId = createdByUserId,
                ReferredByEmployeeId = referredByEmployeeId,
                IsActive = true
            };

            employee.AddDomainEvent(new EmployeeCreatedEvent(employee.Id));
            return employee;
        }

        public void UpdatePersonalDetails(
            string firstName,
            string lastName,
            Gender gender,
            DateOnly? dateOfBirth,
            MaritalStatus maritalStatus,
            string? phone,
            string? addressLine,
            string? city,
            string? state,
            string? postalCode,
            string? country)
        {
            firstName = firstName.Trim();
            lastName = lastName.Trim();
            if (firstName.Length is < 1 or > 100 || lastName.Length is < 1 or > 100)
                throw new DomainException("Enter a first and last name.");
            if (dateOfBirth is { } born && (born > DateOnly.FromDateTime(DateTime.UtcNow) || born.Year < 1900))
                throw new DomainException("Enter a valid date of birth.");

            FirstName = firstName;
            LastName = lastName;
            Gender = gender;
            DateOfBirth = dateOfBirth;
            MaritalStatus = maritalStatus;
            Phone = Clean(phone, 30, "Phone");
            AddressLine = Clean(addressLine, 200, "Address");
            City = Clean(city, 100, "City");
            State = Clean(state, 100, "State");
            PostalCode = Clean(postalCode, 20, "Postal code");
            Country = Clean(country, 100, "Country");
        }

        private static string? Clean(string? value, int max, string label)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var text = value.Trim();
            if (text.Length > max)
                throw new DomainException($"{label} is too long.");
            return text;
        }

        public void AssignManager(Guid managerId) => ManagerId = managerId;
        public void AssignShift(Guid shiftId) => ShiftId = shiftId;
        public void UpdateLocation(string location) => Location = location;
        public void Deactivate()
        {
            IsActive = false;
            if (AccountStatus == Enums.AccountStatus.Active)
                AccountStatus = Enums.AccountStatus.Suspended;
        }

        public void Activate()
        {
            IsActive = true;
            AccountStatus = Enums.AccountStatus.Active;
        }

        public void SetAccountStatus(Enums.AccountStatus status)
        {
            AccountStatus = status;
            IsActive = status == Enums.AccountStatus.Active;
        }

        public string FullName => $"{FirstName} {LastName}";
    }
}
