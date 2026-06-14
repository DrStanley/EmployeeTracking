using EmployeeTracking.Application.Commands.Timesheets;
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
    public class SubmitTimesheetCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly SubmitTimesheetCommandHandler _handler;

        public SubmitTimesheetCommandHandlerTests()
            => _handler = new SubmitTimesheetCommandHandler(_uow.Object);

        [Fact]
        public async Task Handle_ShouldSubmitTimesheet()
        {
            var employeeId = Guid.NewGuid();
            var timesheetId = Guid.NewGuid();
            var timesheet = Timesheet.CreateForPeriod(employeeId, Guid.NewGuid());

            _uow.Setup(u => u.Timesheets.GetByIdAsync(timesheetId, default))
                .ReturnsAsync(timesheet);
            _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

            await _handler.Handle(
                new SubmitTimesheetCommand(timesheetId, employeeId), default);

            timesheet.Status.Should().Be(TimesheetStatus.Submitted);
            _uow.Verify(u => u.Timesheets.Update(timesheet), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenTimesheetNotFound_ShouldThrowNotFoundException()
        {
            _uow.Setup(u => u.Timesheets.GetByIdAsync(It.IsAny<Guid>(), default))
                .ReturnsAsync((Timesheet?)null);

            var act = async () => await _handler.Handle(
                new SubmitTimesheetCommand(Guid.NewGuid(), Guid.NewGuid()), default);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task Handle_WhenWrongEmployee_ShouldThrowUnauthorizedException()
        {
            var timesheet = Timesheet.CreateForPeriod(Guid.NewGuid(), Guid.NewGuid());

            _uow.Setup(u => u.Timesheets.GetByIdAsync(It.IsAny<Guid>(), default))
                .ReturnsAsync(timesheet);

            // Different employee trying to submit
            var act = async () => await _handler.Handle(
                new SubmitTimesheetCommand(timesheet.Id, Guid.NewGuid()), default);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*own timesheet*");
        }
    }
}
