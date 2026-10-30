using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EmployeeTracking.Tests.Unit.Domain
{
    public class AppUsageSessionTests
    {
        [Fact]
        public void Record_ShouldStoreHoursFromTheTimeSpan()
        {
            var started = DateTimeOffset.UtcNow.AddHours(-2);
            var ended = started.AddMinutes(90);

            var session = AppUsageSession.Record(
                Guid.NewGuid(), "Excel", started, ended, AppUsageSource.Manual);

            session.AppName.Should().Be("Excel");
            session.DurationSeconds.Should().Be(90 * 60);
            session.DurationHours.Should().Be(1.50m);
            session.Source.Should().Be(AppUsageSource.Manual);
        }

        [Fact]
        public void Record_ShouldTrimTheAppName()
        {
            var started = DateTimeOffset.UtcNow.AddMinutes(-30);
            var session = AppUsageSession.Record(
                Guid.NewGuid(), "  Slack  ", started, DateTimeOffset.UtcNow, AppUsageSource.System);

            session.AppName.Should().Be("Slack");
        }

        [Fact]
        public void Record_WhenEndIsNotAfterStart_ShouldThrow()
        {
            var at = DateTimeOffset.UtcNow.AddMinutes(-5);
            var act = () => AppUsageSession.Record(
                Guid.NewGuid(), "Excel", at, at, AppUsageSource.Manual);

            act.Should().Throw<DomainException>().WithMessage("*after start*");
        }

        [Fact]
        public void Record_WhenLongerThanADay_ShouldThrow()
        {
            var ended = DateTimeOffset.UtcNow;
            var act = () => AppUsageSession.Record(
                Guid.NewGuid(), "Excel", ended.AddHours(-25), ended, AppUsageSource.Manual);

            act.Should().Throw<DomainException>().WithMessage("*24 hours*");
        }

        [Fact]
        public void Record_WhenAppNameIsMissing_ShouldThrow()
        {
            var act = () => AppUsageSession.Record(
                Guid.NewGuid(), "   ", DateTimeOffset.UtcNow.AddMinutes(-10), DateTimeOffset.UtcNow, AppUsageSource.Manual);

            act.Should().Throw<DomainException>().WithMessage("*App name*");
        }
    }
}
