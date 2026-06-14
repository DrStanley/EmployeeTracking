using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EmployeeTracking.Tests.Unit.Domain
{

    public class TimesheetTests
    {
        private static Timesheet CreateDraftTimesheet() =>
            Timesheet.CreateForPeriod(Guid.NewGuid(), Guid.NewGuid());

        [Fact]
        public void CreateForPeriod_ShouldSetStatusToDraft()
        {
            var timesheet = CreateDraftTimesheet();
            timesheet.Status.Should().Be(TimesheetStatus.Draft);
        }

        [Fact]
        public void Submit_ShouldChangeStatusToSubmitted()
        {
            var timesheet = CreateDraftTimesheet();
            timesheet.Submit();
            timesheet.Status.Should().Be(TimesheetStatus.Submitted);
        }

        [Fact]
        public void Submit_ShouldSetSubmittedAt()
        {
            var timesheet = CreateDraftTimesheet();
            timesheet.Submit();
            timesheet.SubmittedAt.Should().NotBeNull();
            timesheet.SubmittedAt.Should().BeCloseTo(
                DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Submit_ShouldRaiseDomainEvent()
        {
            var timesheet = CreateDraftTimesheet();
            timesheet.Submit();
            timesheet.DomainEvents.Should().ContainSingle(
                e => e is EmployeeTracking.Domain.DomainEvents.TimesheetSubmittedEvent);
        }

        [Fact]
        public void Submit_WhenAlreadySubmitted_ShouldThrowDomainException()
        {
            var timesheet = CreateDraftTimesheet();
            timesheet.Submit();

            var act = () => timesheet.Submit();
            act.Should().Throw<DomainException>()
                .WithMessage("*Cannot submit*");
        }

        [Fact]
        public void Approve_ShouldChangeStatusToApproved()
        {
            var timesheet = CreateDraftTimesheet();
            var reviewerId = Guid.NewGuid();
            timesheet.Submit();
            timesheet.Approve(reviewerId);
            timesheet.Status.Should().Be(TimesheetStatus.Approved);
        }

        [Fact]
        public void Approve_ShouldSetApprovedAt()
        {
            var timesheet = CreateDraftTimesheet();
            timesheet.Submit();
            timesheet.Approve(Guid.NewGuid());
            timesheet.ApprovedAt.Should().NotBeNull();
        }

        [Fact]
        public void Approve_ShouldAddApprovalAction()
        {
            var timesheet = CreateDraftTimesheet();
            var reviewerId = Guid.NewGuid();
            timesheet.Submit();
            timesheet.Approve(reviewerId, "Looks good");
            timesheet.ApprovalActions.Should().ContainSingle(
                a => a.ReviewerId == reviewerId
                  && a.Decision == ApprovalDecision.Approved);
        }

        [Fact]
        public void Approve_WhenNotSubmitted_ShouldThrowDomainException()
        {
            var timesheet = CreateDraftTimesheet();
            var act = () => timesheet.Approve(Guid.NewGuid());
            act.Should().Throw<DomainException>()
                .WithMessage("*Only submitted*");
        }

        [Fact]
        public void Reject_ShouldChangeStatusToRejected()
        {
            var timesheet = CreateDraftTimesheet();
            timesheet.Submit();
            timesheet.Reject(Guid.NewGuid(), "Missing entries.");
            timesheet.Status.Should().Be(TimesheetStatus.Rejected);
        }

        [Fact]
        public void Reject_ShouldSetRejectionReason()
        {
            var timesheet = CreateDraftTimesheet();
            timesheet.Submit();
            timesheet.Reject(Guid.NewGuid(), "Missing entries.");
            timesheet.RejectionReason.Should().Be("Missing entries.");
        }

        [Fact]
        public void Reject_WithEmptyReason_ShouldThrowDomainException()
        {
            var timesheet = CreateDraftTimesheet();
            timesheet.Submit();
            var act = () => timesheet.Reject(Guid.NewGuid(), "");
            act.Should().Throw<DomainException>()
                .WithMessage("*reason is required*");
        }

        [Fact]
        public void Lock_WhenApproved_ShouldChangeStatusToLocked()
        {
            var timesheet = CreateDraftTimesheet();
            timesheet.Submit();
            timesheet.Approve(Guid.NewGuid());
            timesheet.Lock();
            timesheet.Status.Should().Be(TimesheetStatus.Locked);
        }

        [Fact]
        public void Lock_WhenNotApproved_ShouldThrowDomainException()
        {
            var timesheet = CreateDraftTimesheet();
            var act = () => timesheet.Lock();
            act.Should().Throw<DomainException>()
                .WithMessage("*Only approved*");
        }

        [Fact]
        public void CalculateTotals_ShouldSetAllHourFields()
        {
            var timesheet = CreateDraftTimesheet();
            timesheet.CalculateTotals(40m, 5m, 8m, 0m);
            timesheet.TotalRegularHours.Should().Be(40m);
            timesheet.TotalOvertimeHours.Should().Be(5m);
            timesheet.TotalPTOHours.Should().Be(8m);
            timesheet.TotalUnpaidHours.Should().Be(0m);
        }

        [Fact]
        public void RejectedTimesheet_CanBeResubmitted()
        {
            var timesheet = CreateDraftTimesheet();
            timesheet.Submit();
            timesheet.Reject(Guid.NewGuid(), "Fix entries.");
            timesheet.Submit();
            timesheet.Status.Should().Be(TimesheetStatus.Submitted);
        }
    }
}
