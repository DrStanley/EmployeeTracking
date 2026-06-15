using EmployeeTracking.Application.Commands.SubmitPTORequest;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Infrastructure.Handlers;
using FluentAssertions;
using Moq;
using Xunit;

namespace EmployeeTracking.Tests.Unit.Application
{
    public class SubmitPTOCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly SubmitPTOCommandHandler _handler;
        private readonly Mock<IEmailNotificationService> _emailSevices = new();

        public SubmitPTOCommandHandlerTests()
            => _handler = new SubmitPTOCommandHandler(_uow.Object, _emailSevices.Object);

        private static Employee BuildEmployee() =>
            Employee.Create(
                "EMP001", "Jane", "Doe", "jane@test.com",
                "Developer", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                EmploymentType.FullTime);

        [Fact]
        public async Task Handle_WithSufficientBalance_ShouldCreatePTORequest()
        {
            var employeeId = Guid.NewGuid();
            var employee = BuildEmployee();
            var manager = BuildEmployee();
            var balance = PTOBalance.CreateForYear(employeeId, DateTime.UtcNow.Year);
            balance.Accrue(80m);

            _uow.Setup(u => u.Employees.GetByIdAsync(employeeId, default))
                .ReturnsAsync(employee);
            _uow.Setup(u => u.Employees.GetByIdAsync(employee.ManagerId.Value, default))
                .ReturnsAsync(manager);
            _uow.Setup(u => u.PTOBalances.GetByEmployeeAndYearAsync(
                employeeId, DateTime.UtcNow.Year, default))
                .ReturnsAsync(balance);
            _uow.Setup(u => u.PTORequests.HasOverlappingRequestAsync(
                It.IsAny<Guid>(), It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(), default))
                .ReturnsAsync(false);
            _uow.Setup(u => u.PTORequests.AddAsync(
                It.IsAny<PTORequest>(), default))
                .Returns(Task.CompletedTask);
            _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
            _emailSevices.Setup(e=>e.SendPTORequestSubmittedAsync(
                It.IsAny<PTORequest>(), It.IsAny<Employee>(), It.IsAny<Employee>(), default)).Returns(Task.CompletedTask);
            var start = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            var end = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            var command = new SubmitPTOCommand(employeeId, start, end, 40m, "Vacation");

            var result = await _handler.Handle(command, default);

            result.Should().NotBeNull();
            result.HoursRequested.Should().Be(40m);
            result.Status.Should().Be(PTORequestStatus.Pending);
        }

        [Fact]
        public async Task Handle_WithInsufficientBalance_ShouldThrowDomainException()
        {
            var employeeId = Guid.NewGuid();
            var employee = BuildEmployee();
            var balance = PTOBalance.CreateForYear(employeeId, DateTime.UtcNow.Year);
            balance.Accrue(20m); // only 20h available

            _uow.Setup(u => u.Employees.GetByIdAsync(employeeId, default))
                .ReturnsAsync(employee);
            _uow.Setup(u => u.PTOBalances.GetByEmployeeAndYearAsync(
                employeeId, DateTime.UtcNow.Year, default))
                .ReturnsAsync(balance);

            var start = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            var end = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            var command = new SubmitPTOCommand(employeeId, start, end, 40m, null);

            var act = async () => await _handler.Handle(command, default);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("*Insufficient*");
        }

        [Fact]
        public async Task Handle_WithOverlappingRequest_ShouldThrowDomainException()
        {
            var employeeId = Guid.NewGuid();
            var employee = BuildEmployee();
            var balance = PTOBalance.CreateForYear(employeeId, DateTime.UtcNow.Year);
            balance.Accrue(80m);

            _uow.Setup(u => u.Employees.GetByIdAsync(employeeId, default))
                .ReturnsAsync(employee);
            _uow.Setup(u => u.PTOBalances.GetByEmployeeAndYearAsync(
                employeeId, DateTime.UtcNow.Year, default))
                .ReturnsAsync(balance);
            _uow.Setup(u => u.PTORequests.HasOverlappingRequestAsync(
                It.IsAny<Guid>(), It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(), default))
                .ReturnsAsync(true); // overlap!

            var start = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
            var end = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            var command = new SubmitPTOCommand(employeeId, start, end, 40m, null);

            var act = async () => await _handler.Handle(command, default);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("*overlapping*");
        }
    }
}
