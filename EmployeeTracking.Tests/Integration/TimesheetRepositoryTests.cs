using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Xunit;

namespace EmployeeTracking.Tests.Integration
{
    public class TimesheetRepositoryTests : TestBase
    {
        private readonly TimesheetRepository _repository;

        public TimesheetRepositoryTests()
            => _repository = new TimesheetRepository(Context);

        [Fact]
        public async Task AddAsync_ShouldPersistTimesheet()
        {
            var timesheet = Timesheet.CreateForPeriod(
                Guid.NewGuid(), Guid.NewGuid());

            await _repository.AddAsync(timesheet);
            await Context.SaveChangesAsync();

            var saved = await _repository.GetByIdAsync(timesheet.Id);
            saved.Should().NotBeNull();
            saved!.Status.Should().Be(TimesheetStatus.Draft);
        }

        [Fact]
        public async Task GetByEmployeeAndPeriodAsync_ShouldReturnCorrectTimesheet()
        {
            var employeeId = Guid.NewGuid();
            var payPeriodId = Guid.NewGuid();
            var timesheet = Timesheet.CreateForPeriod(employeeId, payPeriodId);

            await _repository.AddAsync(timesheet);
            await Context.SaveChangesAsync();

            var found = await _repository
                .GetByEmployeeAndPeriodAsync(employeeId, payPeriodId);

            found.Should().NotBeNull();
            found!.EmployeeId.Should().Be(employeeId);
            found.PayPeriodId.Should().Be(payPeriodId);
        }

        [Fact]
        public async Task GetByEmployeeAndPeriodAsync_WhenNotFound_ShouldReturnNull()
        {
            var result = await _repository
                .GetByEmployeeAndPeriodAsync(Guid.NewGuid(), Guid.NewGuid());

            result.Should().BeNull();
        }

        [Fact]
        public async Task Update_ShouldPersistStatusChange()
        {
            var timesheet = Timesheet.CreateForPeriod(
                Guid.NewGuid(), Guid.NewGuid());

            await _repository.AddAsync(timesheet);
            await Context.SaveChangesAsync();

            timesheet.Submit();
            _repository.Update(timesheet);
            await Context.SaveChangesAsync();

            var updated = await _repository.GetByIdAsync(timesheet.Id);
            updated!.Status.Should().Be(TimesheetStatus.Submitted);
        }
    }
}
