using FluentValidation;

namespace EmployeeTracking.Application.Commands.Auth
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        private static readonly string[] ValidRoles = { "Employee", "Manager", "Admin" };

        public RegisterCommandValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            RuleFor(x => x.Roles)
           .NotEmpty()
           .WithMessage("At least one role is required.");

            RuleForEach(x => x.Roles)
                .Must(r => ValidRoles.Contains(r))
                .WithMessage("Each role must be Employee, Manager, or Admin.");
        }
    }
}
