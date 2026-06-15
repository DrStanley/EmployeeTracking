using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeTracking.Infrastructure.Identity
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config) => _config = config;

        public string GenerateAccessToken(
            string userId,
            string email,
            Guid employeeId,
            IList<string> roles)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name,           email),
            new(ClaimTypes.Email,          email),
            new("employeeId",              employeeId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                                        double.Parse(_config["Jwt:ExpiryMinutes"]!)),
                signingCredentials: new SigningCredentials(
                                        key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken GenerateRefreshToken(string userId)
            => RefreshToken.Create(userId,
                int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7"));

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false, // allow expired tokens here
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = key
            };

            try
            {
                var principal = new JwtSecurityTokenHandler()
                    .ValidateToken(token, parameters, out var validatedToken);

                // Make sure it was signed with HmacSha256
                if (validatedToken is not JwtSecurityToken jwt ||
                    !jwt.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
