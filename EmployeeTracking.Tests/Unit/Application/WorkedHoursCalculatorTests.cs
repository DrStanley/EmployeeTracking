using EmployeeTracking.Application.Time;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EmployeeTracking.Tests.Unit.Application
{
    public class WorkedHoursCalculatorTests
    {
        [Fact]
        public void Total_SubtractsAClosedBreak()
        {
            var employeeId = Guid.NewGuid();
            var start = DateTimeOffset.UtcNow.AddHours(-8);
            var entries = new[]
            {
                TimeEntry.Create(employeeId, TimeEntryType.ClockIn, TimeEntrySource.WebApp, start),
                TimeEntry.Create(employeeId, TimeEntryType.BreakStart, TimeEntrySource.WebApp, start.AddHours(3)),
                TimeEntry.Create(employeeId, TimeEntryType.BreakEnd, TimeEntrySource.WebApp, start.AddHours(4)),
                TimeEntry.Create(employeeId, TimeEntryType.ClockOut, TimeEntrySource.WebApp, start.AddHours(8))
            };

            WorkedHoursCalculator.Total(entries).Should().Be(7m);
        }

        [Fact]
        public void Total_IgnoresAnOpenPunch()
        {
            var employeeId = Guid.NewGuid();
            var entries = new[]
            {
                TimeEntry.Create(employeeId, TimeEntryType.ClockIn, TimeEntrySource.WebApp, DateTimeOffset.UtcNow.AddHours(-2))
            };

            WorkedHoursCalculator.Total(entries).Should().Be(0m);
        }
    }
}
