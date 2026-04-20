using EmployeeTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeTracking.Infrastructure.Persistence.Configurations
{
    public class PayrollReportConfiguration : IEntityTypeConfiguration<PayrollReport>
    {
        public void Configure(EntityTypeBuilder<PayrollReport> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.RegularHours).HasPrecision(8, 2);
            builder.Property(p => p.OvertimeHours).HasPrecision(8, 2);
            builder.Property(p => p.PTOHours).HasPrecision(8, 2);
            builder.Property(p => p.UnpaidHours).HasPrecision(8, 2);
            builder.Property(p => p.TotalPayableHours).HasPrecision(8, 2);
            builder.Property(p => p.ExceptionNotes).HasMaxLength(2000);

            builder.HasOne(p => p.Employee)
                .WithMany()
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.PayPeriod)
                .WithMany()
                .HasForeignKey(p => p.PayPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Ignore(p => p.DomainEvents);
        }
    }
}
