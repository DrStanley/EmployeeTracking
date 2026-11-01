using EmployeeTracking.Application.Commands.Auth;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUnitOfWork _uow;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenService jwtTokenService,
            IUnitOfWork uow)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenService = jwtTokenService;
            _uow = uow;
        }

        public async Task<AuthResponse> Handle(
            RegisterCommand request, CancellationToken ct)
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);
            var employeeRecord = await _uow.Employees.GetByEmailAsync(request.Email, ct);
            if (existing is not null || employeeRecord?.AccountStatus == AccountStatus.Deleted)
                throw new DomainException("An account with this email already exists.");

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmployeeId = Guid.Empty
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new DomainException($"Registration failed: {errors}");
            }

            const string role = "Employee";
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
            await _userManager.AddToRoleAsync(user, role);

            if (employeeRecord is not null)
            {
                user.EmployeeId = employeeRecord.Id;
                user.AccountStatus = employeeRecord.AccountStatus;
                await _userManager.UpdateAsync(user);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var fullName = $"{request.FirstName} {request.LastName}".Trim();
            if (employeeRecord is not null && !string.IsNullOrWhiteSpace(employeeRecord.FullName))
                fullName = employeeRecord.FullName;
            var accessToken = _jwtTokenService.GenerateAccessToken(
                user.Id, user.Email!, fullName, user.EmployeeId, roles);
            var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

            await _uow.RefreshTokens.AddAsync(refreshToken, ct);
            await _uow.SaveChangesAsync(ct);

            return new AuthResponse(
                AccessToken: accessToken,
                RefreshToken: refreshToken.Token,
                Email: user.Email!,
                FullName: fullName,
                Roles: roles,
                AccessTokenExpiresAt: DateTime.UtcNow.AddMinutes(15),
                AccountStatus: user.AccountStatus.ToString());
        }
    }
}
