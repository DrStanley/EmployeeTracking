using EmployeeTracking.Application.Commands.Auth;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
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
            if (existing is not null)
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

            // Assign all requested roles
            foreach (var role in request.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

                await _userManager.AddToRoleAsync(user, role);
            }

            // link to Employee record if Admin already created one for this email
            var employeeRecord = await _uow.Employees.GetByEmailAsync(request.Email, ct);
            if (employeeRecord is not null)
            {
                user.EmployeeId = employeeRecord.Id;
                await _userManager.UpdateAsync(user);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenService.GenerateToken(user.Id, user.Email!, user.EmployeeId, roles);

            return new AuthResponse(
                Token: token,
                Email: user.Email!,
                FullName: $"{request.FirstName} {request.LastName}",
                Roles: request.Roles,
                ExpiresAt: DateTime.Now.AddMinutes(60));
        }
    }
}
