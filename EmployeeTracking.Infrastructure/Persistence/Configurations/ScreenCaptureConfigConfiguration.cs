using EmployeeTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeTracking.Infrastructure.Persistence.Configurations
{
    public class ScreenCaptureConfigConfiguration : IEntityTypeConfiguration<ScreenCaptureConfig>
    {
        public void Configure(EntityTypeBuilder<ScreenCaptureConfig> builder)
        {
            builder.HasKey(c => c.Id);
            builder.Property(c => c.IntervalMinutes).IsRequired();
        }
    }
}
