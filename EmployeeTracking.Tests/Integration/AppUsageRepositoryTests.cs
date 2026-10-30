using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Xunit;

namespace EmployeeTracking.Tests.Integration
{
    public class AppUsageRepositoryTests : TestBase
    {
        [Fact]
        public async Task GetByEmployeeAndRange_ReturnsOnlySessionsThatStartInTheRange()
        {
            var repository = new AppUsageRepository(Context);
            var employeeId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow.AddMinutes(-1);

            var inside = AppUsageSession.Record(
                employeeId, "Excel", now.AddHours(-2), now.AddHours(-1), AppUsageSource.Manual);
            var outside = AppUsageSession.Record(
                employeeId, "Slack", now.AddDays(-10), now.AddDays(-10).AddHours(1), AppUsageSource.System);

            await repository.AddAsync(inside);
            await repository.AddAsync(outside);
            await Context.SaveChangesAsync();

            var result = await repository.GetByEmployeeAndRangeAsync(
                employeeId, now.AddDays(-1), now.AddMinutes(1));

            result.Should().ContainSingle();
            result[0].AppName.Should().Be("Excel");
            result[0].DurationHours.Should().Be(1.00m);
        }
    }
}
