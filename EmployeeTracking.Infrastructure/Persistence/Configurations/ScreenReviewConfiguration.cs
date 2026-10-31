using EmployeeTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeTracking.Infrastructure.Persistence.Configurations
{
    public class ScreenReviewConfiguration : IEntityTypeConfiguration<ScreenReview>
    {
        public void Configure(EntityTypeBuilder<ScreenReview> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.EmployeeName).HasMaxLength(200).IsRequired();
            builder.Property(r => r.Findings).HasMaxLength(4000).IsRequired();
            builder.HasIndex(r => r.EndedAt);
        }
    }
}
