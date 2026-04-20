using EmployeeTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeTracking.Infrastructure.Persistence.Configurations
{
    public class PTORequestConfiguration : IEntityTypeConfiguration<PTORequest>
    {
        public void Configure(EntityTypeBuilder<PTORequest> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.HoursRequested).HasPrecision(6, 2);
            builder.Property(p => p.Notes).HasMaxLength(1000);
            builder.Property(p => p.ReviewerNotes).HasMaxLength(1000);

            builder.HasOne(p => p.Employee)
                .WithMany()
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Ignore(p => p.DomainEvents);
        }
    }
}
