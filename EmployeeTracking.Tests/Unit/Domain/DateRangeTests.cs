using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace EmployeeTracking.Tests.Unit.Domain
{
    public class DateRangeTests
    {
        [Fact]
        public void Create_WithValidDates_ShouldSucceed()
        {
            var start = new DateOnly(2024, 1, 1);
            var end = new DateOnly(2024, 1, 31);
            var range = new DateRange(start, end);
            range.Start.Should().Be(start);
            range.End.Should().Be(end);
        }

        [Fact]
        public void Create_WithEndBeforeStart_ShouldThrowDomainException()
        {
            var act = () => new DateRange(
                new DateOnly(2024, 1, 31),
                new DateOnly(2024, 1, 1));
            act.Should().Throw<DomainException>()
                .WithMessage("*End date*");
        }

        [Fact]
        public void TotalDays_ShouldReturnCorrectCount()
        {
            var range = new DateRange(
                new DateOnly(2024, 1, 1),
                new DateOnly(2024, 1, 7));
            range.TotalDays.Should().Be(7);
        }

        [Fact]
        public void Overlaps_WhenRangesOverlap_ShouldReturnTrue()
        {
            var range1 = new DateRange(
                new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 15));
            var range2 = new DateRange(
                new DateOnly(2024, 1, 10), new DateOnly(2024, 1, 20));
            range1.Overlaps(range2).Should().BeTrue();
        }

        [Fact]
        public void Overlaps_WhenRangesDoNotOverlap_ShouldReturnFalse()
        {
            var range1 = new DateRange(
                new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
            var range2 = new DateRange(
                new DateOnly(2024, 1, 11), new DateOnly(2024, 1, 20));
            range1.Overlaps(range2).Should().BeFalse();
        }

        [Fact]
        public void Contains_WhenDateIsInRange_ShouldReturnTrue()
        {
            var range = new DateRange(
                new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31));
            range.Contains(new DateOnly(2024, 1, 15)).Should().BeTrue();
        }

        [Fact]
        public void Contains_WhenDateIsOutsideRange_ShouldReturnFalse()
        {
            var range = new DateRange(
                new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31));
            range.Contains(new DateOnly(2024, 2, 1)).Should().BeFalse();
        }
    }
}
