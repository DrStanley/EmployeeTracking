using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EmployeeTracking.Tests.Unit.Domain
{
    public class PTOBalanceTests
    {
        private static PTOBalance CreateBalance(decimal initial = 0m)
        {
            var balance = PTOBalance.CreateForYear(Guid.NewGuid(), DateTime.UtcNow.Year);
            if (initial > 0) balance.Accrue(initial);
            return balance;
        }

        [Fact]
        public void Accrue_ShouldIncreaseAvailableAndAccruedHours()
        {
            var balance = CreateBalance();
            balance.Accrue(40m);
            balance.AvailableHours.Should().Be(40m);
            balance.AccruedHours.Should().Be(40m);
        }

        [Fact]
        public void Accrue_WithZeroHours_ShouldThrowDomainException()
        {
            var balance = CreateBalance();
            var act = () => balance.Accrue(0m);
            act.Should().Throw<DomainException>()
                .WithMessage("*must be positive*");
        }

        [Fact]
        public void Deduct_ShouldDecreaseAvailableAndIncreaseUsed()
        {
            var balance = CreateBalance(80m);
            balance.Deduct(40m);
            balance.AvailableHours.Should().Be(40m);
            balance.UsedHours.Should().Be(40m);
        }

        [Fact]
        public void Deduct_WhenInsufficientBalance_ShouldThrowDomainException()
        {
            var balance = CreateBalance(20m);
            var act = () => balance.Deduct(40m);
            act.Should().Throw<DomainException>()
                .WithMessage("*Insufficient*");
        }

        [Fact]
        public void Restore_ShouldIncreaseAvailableAndDecreaseUsed()
        {
            var balance = CreateBalance(80m);
            balance.Deduct(40m);
            balance.Restore(40m);
            balance.AvailableHours.Should().Be(80m);
            balance.UsedHours.Should().Be(0m);
        }
    }
}
