using EmployeeTracking.Application.Commands.Auth;
using EmployeeTracking.Application.Interfaces;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class RevokeAllTokensCommandHandler
        : IRequestHandler<RevokeAllTokensCommand, Unit>
    {
        private readonly IUnitOfWork _uow;

        public RevokeAllTokensCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Unit> Handle(
            RevokeAllTokensCommand request, CancellationToken ct)
        {
            await _uow.RefreshTokens.RevokeAllForUserAsync(
                request.UserId,
                "Admin revoked all sessions",
                ct);

            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
