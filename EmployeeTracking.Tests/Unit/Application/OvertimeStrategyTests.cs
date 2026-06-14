using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Infrastructure.Persistence;
using FluentAssertions;
using Xunit;

namespace EmployeeTracking.Tests.Unit.Application
{
    public class OvertimeStrategyTests
    {
        private readonly StandardOvertimeStrategy _strategy = new();
        private const decimal DailyThreshold = 8m;
        private const decimal WeeklyThreshold = 40m;

        private static DailyHoursDto Day(string date, decimal hours) =>
            new(DateOnly.Parse(date), hours);

        [Fact]
        public void NoOvertime_ShouldReturnAllRegular()
        {
            var days = new List<DailyHoursDto>
        {
            Day("2024-01-01", 8m),
            Day("2024-01-02", 8m),
            Day("2024-01-03", 8m),
            Day("2024-01-04", 8m),
            Day("2024-01-05", 8m)
        };

            var (regular, overtime) = _strategy.Calculate(
                days, DailyThreshold, WeeklyThreshold);

            regular.Should().Be(40m);
            overtime.Should().Be(0m);
        }

        [Fact]
        public void DailyOvertime_ShouldCalculateCorrectly()
        {
            var days = new List<DailyHoursDto>
        {
            Day("2024-01-01", 10m), // 2h OT
            Day("2024-01-02", 10m), // 2h OT
            Day("2024-01-03", 8m),
            Day("2024-01-04", 8m),
            Day("2024-01-05", 8m)
        };

            var (regular, overtime) = _strategy.Calculate(
                days, DailyThreshold, WeeklyThreshold);

            overtime.Should().Be(4m);
            regular.Should().Be(40m);
        }

        [Fact]
        public void WeeklyOvertime_WithNoDailyOvertime_ShouldCalculateCorrectly()
        {
            // 44 hours spread across 5 days — no single day exceeds 8h
            var days = new List<DailyHoursDto>
        {
            Day("2024-01-01", 8m),
            Day("2024-01-02", 8m),
            Day("2024-01-03", 8m),
            Day("2024-01-04", 8m),
            Day("2024-01-05", 8m),
            Day("2024-01-06", 4m) // Saturday
        };

            var (regular, overtime) = _strategy.Calculate(
                days, DailyThreshold, WeeklyThreshold);

            overtime.Should().Be(4m);
            regular.Should().Be(40m);
        }

        [Fact]
        public void DailyAndWeeklyOvertime_ShouldNotDoubleCount()
        {
            // 10h each day = 50h total, 10h daily OT
            // Weekly OT would be 10h but daily already caught it
            var days = new List<DailyHoursDto>
        {
            Day("2024-01-01", 10m),
            Day("2024-01-02", 10m),
            Day("2024-01-03", 10m),
            Day("2024-01-04", 10m),
            Day("2024-01-05", 10m)
        };

            var (regular, overtime) = _strategy.Calculate(
                days, DailyThreshold, WeeklyThreshold);

            overtime.Should().Be(10m);
            regular.Should().Be(40m);
            (regular + overtime).Should().Be(50m);
        }

        [Fact]
        public void EmptyDays_ShouldReturnZeroHours()
        {
            var (regular, overtime) = _strategy.Calculate(
                new List<DailyHoursDto>(), DailyThreshold, WeeklyThreshold);

            regular.Should().Be(0m);
            overtime.Should().Be(0m);
        }

        [Fact]
        public void SingleDayUnderThreshold_ShouldReturnAllRegular()
        {
            var days = new List<DailyHoursDto> { Day("2024-01-01", 6m) };

            var (regular, overtime) = _strategy.Calculate(
                days, DailyThreshold, WeeklyThreshold);

            regular.Should().Be(6m);
            overtime.Should().Be(0m);
        }

        [Fact]
        public void SingleDayOverThreshold_ShouldReturnOvertimeHours()
        {
            var days = new List<DailyHoursDto> { Day("2024-01-01", 12m) };

            var (regular, overtime) = _strategy.Calculate(
                days, DailyThreshold, WeeklyThreshold);

            regular.Should().Be(8m);
            overtime.Should().Be(4m);
        }
    }
}
