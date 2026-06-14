using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace EmployeeTracking.Tests.Unit.Domain
{
    public class PTORequestTests
    {
        private static PTORequest CreatePendingRequest(decimal hours = 40m) =>
            PTORequest.Create(
                Guid.NewGuid(),
                new DateRange(
                    DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                    DateOnly.FromDateTime(DateTime.Today.AddDays(5))),
                hours,
                "Vacation");

        [Fact]
        public void Create_ShouldSetStatusToPending()
        {
            var request = CreatePendingRequest();
            request.Status.Should().Be(PTORequestStatus.Pending);
        }

        [Fact]
        public void Approve_ShouldChangeStatusToApproved()
        {
            var request = CreatePendingRequest();
            var reviewerId = Guid.NewGuid();
            request.Approve(reviewerId, "Approved!");
            request.Status.Should().Be(PTORequestStatus.Approved);
        }

        [Fact]
        public void Approve_ShouldSetReviewerFields()
        {
            var request = CreatePendingRequest();
            var reviewerId = Guid.NewGuid();
            request.Approve(reviewerId, "OK");
            request.ReviewedBy.Should().Be(reviewerId);
            request.ReviewerNotes.Should().Be("OK");
            request.ReviewedAt.Should().NotBeNull();
        }

        [Fact]
        public void Approve_WhenAlreadyApproved_ShouldThrowDomainException()
        {
            var request = CreatePendingRequest();
            request.Approve(Guid.NewGuid());
            var act = () => request.Approve(Guid.NewGuid());
            act.Should().Throw<DomainException>()
                .WithMessage("*Only pending*");
        }

        [Fact]
        public void Reject_ShouldChangeStatusToRejected()
        {
            var request = CreatePendingRequest();
            request.Reject(Guid.NewGuid(), "Team at capacity.");
            request.Status.Should().Be(PTORequestStatus.Rejected);
        }

        [Fact]
        public void Cancel_WhenPending_ShouldChangeStatusToCancelled()
        {
            var request = CreatePendingRequest();
            request.Cancel();
            request.Status.Should().Be(PTORequestStatus.Cancelled);
        }

        [Fact]
        public void Cancel_WhenApproved_ShouldThrowDomainException()
        {
            var request = CreatePendingRequest();
            request.Approve(Guid.NewGuid());
            var act = () => request.Cancel();
            act.Should().Throw<DomainException>()
                .WithMessage("*admin*");
        }
    }
}
