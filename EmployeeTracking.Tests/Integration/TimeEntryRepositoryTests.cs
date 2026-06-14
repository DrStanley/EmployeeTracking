using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Xunit;

namespace EmployeeTracking.Tests.Integration
{
    public class TimeEntryRepositoryTests : TestBase
    {
        private readonly TimeEntryRepository _repository;

        public TimeEntryRepositoryTests()
            => _repository = new TimeEntryRepository(Context);

        private static TimeEntry BuildClockIn(Guid employeeId) =>
            TimeEntry.Create(
                employeeId,
                TimeEntryType.ClockIn,
                TimeEntrySource.WebApp,
                DateTimeOffset.UtcNow);

        private static TimeEntry BuildClockOut(Guid employeeId) =>
            TimeEntry.Create(
                employeeId,
                TimeEntryType.ClockOut,
                TimeEntrySource.WebApp,
                DateTimeOffset.UtcNow.AddHours(8));

        [Fact]
        public async Task HasOpenClockInAsync_WhenClockInExists_ShouldReturnTrue()
        {
            var employeeId = Guid.NewGuid();
            await _repository.AddAsync(BuildClockIn(employeeId));
            await Context.SaveChangesAsync();

            var result = await _repository.HasOpenClockInAsync(employeeId);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasOpenClockInAsync_WhenClockOutExists_ShouldReturnFalse()
        {
            var employeeId = Guid.NewGuid();
            await _repository.AddAsync(BuildClockIn(employeeId));
            await _repository.AddAsync(BuildClockOut(employeeId));
            await Context.SaveChangesAsync();

            var result = await _repository.HasOpenClockInAsync(employeeId);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetByEmployeeAndDateRangeAsync_ShouldReturnEntriesInRange()
        {
            var employeeId = Guid.NewGuid();
            var entry = BuildClockIn(employeeId);

            await _repository.AddAsync(entry);
            await Context.SaveChangesAsync();

            var from = DateTimeOffset.UtcNow.AddHours(-1);
            var to = DateTimeOffset.UtcNow.AddHours(1);
            var results = await _repository
                .GetByEmployeeAndDateRangeAsync(employeeId, from, to);

            results.Should().ContainSingle(e => e.Id == entry.Id);
        }

        [Fact]
        public async Task GetLatestOpenClockInAsync_ShouldReturnMostRecentEntry()
        {
            var employeeId = Guid.NewGuid();
            var first = BuildClockIn(employeeId);
            await _repository.AddAsync(first);
            await Context.SaveChangesAsync();

            var result = await _repository.GetLatestOpenClockInAsync(employeeId);

            result.Should().NotBeNull();
            result!.EntryType.Should().Be(TimeEntryType.ClockIn);
        }
    }
}
