using FluentValidation;

namespace EmployeeTracking.Application.Commands.Activity
{
    public class RecordAppUsageCommandValidator : AbstractValidator<RecordAppUsageCommand>
    {
        public RecordAppUsageCommandValidator()
        {
            RuleFor(x => x.EmployeeId).NotEmpty();
            RuleFor(x => x.AppName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.EndedAt).GreaterThan(x => x.StartedAt)
                .WithMessage("End time must be after start time.");
            RuleFor(x => x)
                .Must(x => x.EndedAt - x.StartedAt <= TimeSpan.FromHours(24))
                .WithMessage("A single activity session cannot exceed 24 hours.");
            RuleFor(x => x.Source).IsInEnum();
        }
    }
}
