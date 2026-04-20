using EmployeeTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeTracking.Infrastructure.Persistence.Configurations
{
    public class OvertimeRuleConfiguration : IEntityTypeConfiguration<OvertimeRule>
    {
        public void Configure(EntityTypeBuilder<OvertimeRule> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.DailyThresholdHours).HasPrecision(5, 2);
            builder.Property(o => o.WeeklyThresholdHours).HasPrecision(5, 2);
            builder.Property(o => o.PremiumMultiplier).HasPrecision(4, 2);
            builder.Property(o => o.DoubleTimeDailyThreshold).HasPrecision(5, 2);

            builder.HasOne(o => o.AttendancePolicy)
                .WithMany()
                .HasForeignKey(o => o.AttendancePolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(o => o.DomainEvents);
        }
    }
}
