using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EmployeeTracking.Tests.Unit.Domain
{
    public class EmployeeTests
    {
        private static Employee CreateEmployee(string email = "test@company.com") =>
            Employee.Create(
                employeeNumber: "EMP001",
                firstName: "John",
                lastName: "Doe",
                email: email,
                jobTitle: "Developer",
                departmentId: Guid.NewGuid(),
                attendancePolicyId: Guid.NewGuid(),
                employmentType: EmploymentType.FullTime);

        [Fact]
        public void Create_ShouldSetAllProperties()
        {
            var employee = CreateEmployee();

            employee.EmployeeNumber.Should().Be("EMP001");
            employee.FirstName.Should().Be("John");
            employee.LastName.Should().Be("Doe");
            employee.Email.Should().Be("test@company.com");
            employee.IsActive.Should().BeTrue();
        }

        [Fact]
        public void FullName_ShouldCombineFirstAndLastName()
        {
            var employee = CreateEmployee();
            employee.FullName.Should().Be("John Doe");
        }

        [Fact]
        public void Create_ShouldRaiseDomainEvent()
        {
            var employee = CreateEmployee();
            employee.DomainEvents.Should().ContainSingle(
                e => e is EmployeeTracking.Domain.DomainEvents.EmployeeCreatedEvent);
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            var employee = CreateEmployee();
            employee.Deactivate();
            employee.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Activate_ShouldSetIsActiveToTrue()
        {
            var employee = CreateEmployee();
            employee.Deactivate();
            employee.Activate();
            employee.IsActive.Should().BeTrue();
        }

        [Fact]
        public void AssignManager_ShouldSetManagerId()
        {
            var employee = CreateEmployee();
            var managerId = Guid.NewGuid();
            employee.AssignManager(managerId);
            employee.ManagerId.Should().Be(managerId);
        }

        [Fact]
        public void AssignShift_ShouldSetShiftId()
        {
            var employee = CreateEmployee();
            var shiftId = Guid.NewGuid();
            employee.AssignShift(shiftId);
            employee.ShiftId.Should().Be(shiftId);
        }
    }
}
