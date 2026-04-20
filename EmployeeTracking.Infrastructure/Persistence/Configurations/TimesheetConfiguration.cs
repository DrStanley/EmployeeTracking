using EmployeeTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeTracking.Infrastructure.Persistence.Configurations
{
    public class TimesheetConfiguration : IEntityTypeConfiguration<Timesheet>
    {
        public void Configure(EntityTypeBuilder<Timesheet> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(t => t.TotalRegularHours).HasPrecision(8, 2);
            builder.Property(t => t.TotalOvertimeHours).HasPrecision(8, 2);
            builder.Property(t => t.TotalPTOHours).HasPrecision(8, 2);
            builder.Property(t => t.TotalUnpaidHours).HasPrecision(8, 2);
            builder.Property(t => t.RejectionReason).HasMaxLength(1000);

            builder.HasMany(t => t.Lines)
                .WithOne()
                .HasForeignKey(l => l.TimesheetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.ApprovalActions)
                .WithOne()
                .HasForeignKey(a => a.TimesheetId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => new { t.EmployeeId, t.PayPeriodId })
                .IsUnique();

            builder.Ignore(t => t.DomainEvents);
        }
    }
}
