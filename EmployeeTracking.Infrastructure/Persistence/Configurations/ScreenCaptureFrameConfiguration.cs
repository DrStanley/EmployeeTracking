using EmployeeTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeTracking.Infrastructure.Persistence.Configurations
{
    public class ScreenCaptureFrameConfiguration : IEntityTypeConfiguration<ScreenCaptureFrame>
    {
        public void Configure(EntityTypeBuilder<ScreenCaptureFrame> builder)
        {
            builder.HasKey(f => f.Id);
            builder.Property(f => f.StorageKey).HasMaxLength(500).IsRequired();
            builder.HasIndex(f => new { f.EmployeeId, f.CapturedAt });
        }
    }
}
