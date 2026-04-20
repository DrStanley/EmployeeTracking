using EmployeeTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeTracking.Infrastructure.Persistence.Configurations
{
    public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
    {
        public void Configure(EntityTypeBuilder<TimeEntry> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.EntryType)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(t => t.Source)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(t => t.Notes)
                .HasMaxLength(500);

            // Tell EF this is an owned type — no separate table, columns inline
            builder.OwnsOne(t => t.Location, loc =>
            {
                loc.Property(l => l.Latitude).HasColumnName("Latitude");
                loc.Property(l => l.Longitude).HasColumnName("Longitude");
                loc.Property(l => l.DeviceId)
                   .HasMaxLength(100)
                   .HasColumnName("DeviceId");
                loc.Property(l => l.IpAddress)
                   .HasMaxLength(45)
                   .HasColumnName("IpAddress");
            });

            builder.HasQueryFilter(t => !t.IsDeleted);

            builder.HasIndex(t => t.EmployeeId);
            builder.HasIndex(t => t.Timestamp);
        }
    }
}
