using FluentValidation;

namespace EmployeeTracking.Application.Commands.ClockIn
{
    public class ClockInCommandValidator : AbstractValidator<ClockInCommand>
    {
        public ClockInCommandValidator()
        {
            //RuleFor(x => x.EmployeeId)
            //    .NotEmpty().WithMessage("Employee ID is required.");

            RuleFor(x => x.Source)
                .IsInEnum().WithMessage("Invalid clock source.");
        }
    }
}
