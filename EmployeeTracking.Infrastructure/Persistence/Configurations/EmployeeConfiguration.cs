using EmployeeTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeTracking.Infrastructure.Persistence.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.EmployeeNumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(e => e.EmployeeNumber)
                .IsUnique();

            builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            builder.Property(e => e.Email).HasMaxLength(256).IsRequired();
            builder.HasIndex(e => e.Email).IsUnique();
            builder.Property(e => e.JobTitle).HasMaxLength(100).IsRequired();
            builder.Property(e => e.Location).HasMaxLength(200);

            builder.Property(e => e.EmploymentType)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(e => e.CreatedByUserId)
                .HasMaxLength(450)
                .IsRequired(false);

            builder.Property(e => e.ReferredByEmployeeId)
                .IsRequired(false);

            builder.HasOne(e => e.ReferredByEmployee)
                .WithMany()
                .HasForeignKey(e => e.ReferredByEmployeeId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

            builder.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(e => e.Shift)
                .WithMany()
                .HasForeignKey(e => e.ShiftId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(e => e.AttendancePolicy)
                .WithMany()
                .HasForeignKey(e => e.AttendancePolicyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
