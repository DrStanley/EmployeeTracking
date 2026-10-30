using EmployeeTracking.Application.Activity;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EmployeeTracking.Tests.Unit.Application
{
    public class AppActivityCalculatorTests
    {
        [Fact]
        public void Summarize_GroupsTheSameAppIgnoringCase()
        {
            var employeeId = Guid.NewGuid();
            var end = DateTimeOffset.UtcNow.AddMinutes(-1);
            var excel = AppUsageSession.Record(
                employeeId, "Excel", end.AddHours(-2), end.AddHours(-1), AppUsageSource.Manual);
            var excelAgain = AppUsageSession.Record(
                employeeId, "excel", end.AddHours(-1), end.AddMinutes(-30), AppUsageSource.Manual);
            var slack = AppUsageSession.Record(
                employeeId, "Slack", end.AddMinutes(-30), end, AppUsageSource.System);

            var (total, apps) = AppActivityCalculator.Summarize(new[] { excel, excelAgain, slack });

            total.Should().Be(2.00m);
            apps.Should().HaveCount(2);
            apps[0].AppName.Should().Be("excel");
            apps[0].Hours.Should().Be(1.50m);
            apps[0].SessionCount.Should().Be(2);
            apps[0].Percent.Should().Be(75);
            apps[1].AppName.Should().Be("Slack");
            apps[1].Hours.Should().Be(0.50m);
        }
    }
}
