using EmployeeTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeTracking.Infrastructure.Persistence.Configurations
{
    public class AppUsageSessionConfiguration : IEntityTypeConfiguration<AppUsageSession>
    {
        public void Configure(EntityTypeBuilder<AppUsageSession> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.AppName)
                .HasMaxLength(AppUsageSession.MaxAppNameLength)
                .IsRequired();

            builder.Property(a => a.Source)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(a => a.DurationSeconds)
                .IsRequired();

            builder.HasOne<Employee>()
                .WithMany()
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => new { a.EmployeeId, a.StartedAt });
        }
    }
}
