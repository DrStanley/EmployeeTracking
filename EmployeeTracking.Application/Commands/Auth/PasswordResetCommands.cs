using MediatR;

namespace EmployeeTracking.Application.Commands.Auth
{
    public record ForgotPasswordCommand(string Email) : IRequest<Unit>;

    public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Unit>;
}
