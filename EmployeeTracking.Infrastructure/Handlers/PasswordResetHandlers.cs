using EmployeeTracking.Application.Commands.Auth;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly ILogger<ForgotPasswordCommandHandler> _logger;

        public ForgotPasswordCommandHandler(
            UserManager<ApplicationUser> users,
            IEmailService email,
            IConfiguration config,
            ILogger<ForgotPasswordCommandHandler> logger)
        {
            _users = users;
            _email = email;
            _config = config;
            _logger = logger;
        }

        public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken ct)
        {
            var user = await _users.FindByEmailAsync(request.Email);
            if (user is null || string.IsNullOrWhiteSpace(user.Email))
                return Unit.Value;

            var token = await _users.GeneratePasswordResetTokenAsync(user);
            var frontend = _config["App:FrontendUrl"] ?? "http://localhost:4200";
            var link = $"{frontend.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
            var result = await _email.SendAsync(new EmailMessages(
                user.Email,
                "Reset your EmpTrack password",
                $"<p>Use this link to choose a new password. It expires with your account's reset token.</p><p><a href=\"{link}\">Reset password</a></p>",
                $"Reset your password: {link}"), ct);

            if (!result.Success)
                _logger.LogWarning("Password reset email failed for {Email}: {Error}", user.Email, result.Error);

            return Unit.Value;
        }
    }

    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
    {
        private readonly UserManager<ApplicationUser> _users;

        public ResetPasswordCommandHandler(UserManager<ApplicationUser> users) => _users = users;

        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken ct)
        {
            var user = await _users.FindByEmailAsync(request.Email)
                ?? throw new DomainException("This reset link is invalid or has expired.");
            var result = await _users.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
                throw new DomainException(string.Join(" ", result.Errors.Select(error => error.Description)));
            return Unit.Value;
        }
    }
}
