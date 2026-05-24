using FluentValidation;

namespace EmployeeTracking.Application.Commands.SubmitPTORequest
{
    public class SubmitPTOCommandValidator : AbstractValidator<SubmitPTOCommand>
    {
        public SubmitPTOCommandValidator()
        {
            RuleFor(x => x.EmployeeId)
                .NotEmpty();

            RuleFor(x => x.StartDate)
                .NotEmpty()
                .Must(d => d >= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Start date must be today or in the future.");

            RuleFor(x => x.EndDate)
                .NotEmpty()
                .Must((cmd, end) => end >= cmd.StartDate)
                .WithMessage("End date must be on or after the start date.");

            RuleFor(x => x.HoursRequested)
                .GreaterThan(0)
                .WithMessage("Hours requested must be greater than zero.");

            RuleFor(x => x.Notes)
                .MaximumLength(1000);
        }
    }
}
