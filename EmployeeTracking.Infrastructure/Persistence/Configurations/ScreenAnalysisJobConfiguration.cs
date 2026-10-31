using EmployeeTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeTracking.Infrastructure.Persistence.Configurations
{
    public class ScreenAnalysisJobConfiguration : IEntityTypeConfiguration<ScreenAnalysisJob>
    {
        public void Configure(EntityTypeBuilder<ScreenAnalysisJob> builder)
        {
            builder.HasKey(j => j.Id);
            builder.Property(j => j.EmployeeName).HasMaxLength(200).IsRequired();
            builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(j => j.LastError).HasMaxLength(1000);
            builder.HasIndex(j => new { j.Status, j.CreatedAt });
            builder.HasIndex(j => new { j.EmployeeId, j.StartedAt });
        }
    }
}
