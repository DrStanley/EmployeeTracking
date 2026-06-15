using EmployeeTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeTracking.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.UserId)
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(r => r.Token)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(r => r.RevokedReason)
                .HasMaxLength(200);

            builder.Property(r => r.ReplacedByToken)
                .HasMaxLength(200);

            // Fast lookups by token value and by user
            builder.HasIndex(r => r.Token).IsUnique();
            builder.HasIndex(r => r.UserId);
        }
    }
}
