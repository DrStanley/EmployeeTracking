using EmployeeTracking.Domain.Common;

namespace EmployeeTracking.Domain.Entities
{
    public class RefreshToken : AuditableEntity
    {
        public Guid Id { get; private set; }
        public string UserId { get; private set; } = string.Empty;
        public string Token { get; private set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; private set; }
        public bool IsRevoked { get; private set; }
        public string? ReplacedByToken { get; private set; }
        public string? RevokedReason { get; private set; }
        public DateTimeOffset? RevokedAt { get; private set; }

        private RefreshToken() { }

        public static RefreshToken Create(string userId, int expiryDays = 7) =>
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = Convert.ToBase64String(
                    System.Security.Cryptography.RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(expiryDays)
            };

        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;

        public void Revoke(string reason, string? replacedByToken = null)
        {
            IsRevoked = true;
            RevokedReason = reason;
            RevokedAt = DateTimeOffset.UtcNow;
            ReplacedByToken = replacedByToken;
        }
    }
}
