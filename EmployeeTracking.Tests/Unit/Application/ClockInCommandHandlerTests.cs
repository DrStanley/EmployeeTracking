using EmployeeTracking.Application.Commands.ClockIn;
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
    public class ClockInCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly Mock<ITimeEntryFactory> _factory;
        private readonly ClockInCommandHandler _handler;

        private static readonly Guid EmployeeId = Guid.NewGuid();

        public ClockInCommandHandlerTests()
        {
            _uow = new Mock<IUnitOfWork>();
            _factory = new Mock<ITimeEntryFactory>();
            _handler = new ClockInCommandHandler(_uow.Object, _factory.Object);
        }

        private static Employee BuildActiveEmployee()
        {
            var emp = Employee.Create(
                "EMP001", "John", "Doe", "john@test.com",
                "Developer", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                EmploymentType.FullTime);
            return emp;
        }

        private ClockInCommand BuildCommand() =>
            new(EmployeeId, TimeEntrySource.WebApp, null, null, null);

        [Fact]
        public async Task Handle_WithValidEmployee_ShouldReturnClockInResponse()
        {
            // Arrange
            var employee = BuildActiveEmployee();
            var entry = TimeEntry.Create(
                EmployeeId, TimeEntryType.ClockIn,
                TimeEntrySource.WebApp, DateTimeOffset.UtcNow);

            _uow.Setup(u => u.Employees.GetByIdAsync(It.IsAny<Guid>(), default))
                .ReturnsAsync(employee);

            _uow.Setup(u => u.TimeEntries.HasOpenClockInAsync(It.IsAny<Guid>(), default))
                .ReturnsAsync(false);

            _factory.Setup(f => f.CreateClockIn(
                It.IsAny<Guid>(), It.IsAny<TimeEntrySource>(),
                It.IsAny<DateTimeOffset>(), null))
                .Returns(entry);

            _uow.Setup(u => u.TimeEntries.AddAsync(entry, default))
                .Returns(Task.CompletedTask);

            _uow.Setup(u => u.SaveChangesAsync(default))
                .ReturnsAsync(1);

            // Act
            var result = await _handler.Handle(BuildCommand(), default);

            // Assert
            result.Should().NotBeNull();
            result.EntryId.Should().Be(entry.Id);
            result.Message.Should().Contain("Clocked in");
        }

        [Fact]
        public async Task Handle_WhenEmployeeNotFound_ShouldThrowNotFoundException()
        {
            _uow.Setup(u => u.Employees.GetByIdAsync(It.IsAny<Guid>(), default))
                .ReturnsAsync((Employee?)null);

            var act = async () => await _handler.Handle(BuildCommand(), default);

            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Employee*");
        }

        [Fact]
        public async Task Handle_WhenEmployeeInactive_ShouldThrowDomainException()
        {
            var employee = BuildActiveEmployee();
            employee.Deactivate();

            _uow.Setup(u => u.Employees.GetByIdAsync(It.IsAny<Guid>(), default))
                .ReturnsAsync(employee);

            var act = async () => await _handler.Handle(BuildCommand(), default);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("*Inactive*");
        }

        [Fact]
        public async Task Handle_WhenAlreadyClockedIn_ShouldThrowDomainException()
        {
            var employee = BuildActiveEmployee();

            _uow.Setup(u => u.Employees.GetByIdAsync(It.IsAny<Guid>(), default))
                .ReturnsAsync(employee);

            _uow.Setup(u => u.TimeEntries.HasOpenClockInAsync(It.IsAny<Guid>(), default))
                .ReturnsAsync(true);

            var act = async () => await _handler.Handle(BuildCommand(), default);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("*open clock-in*");
        }

        [Fact]
        public async Task Handle_ShouldCallSaveChanges()
        {
            var employee = BuildActiveEmployee();
            var entry = TimeEntry.Create(
                EmployeeId, TimeEntryType.ClockIn,
                TimeEntrySource.WebApp, DateTimeOffset.UtcNow);

            _uow.Setup(u => u.Employees.GetByIdAsync(It.IsAny<Guid>(), default))
                .ReturnsAsync(employee);
            _uow.Setup(u => u.TimeEntries.HasOpenClockInAsync(It.IsAny<Guid>(), default))
                .ReturnsAsync(false);
            _factory.Setup(f => f.CreateClockIn(
                It.IsAny<Guid>(), It.IsAny<TimeEntrySource>(),
                It.IsAny<DateTimeOffset>(), null))
                .Returns(entry);
            _uow.Setup(u => u.TimeEntries.AddAsync(entry, default))
                .Returns(Task.CompletedTask);
            _uow.Setup(u => u.SaveChangesAsync(default))
                .ReturnsAsync(1);

            await _handler.Handle(BuildCommand(), default);

            _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }
    }
}
