using EmployeeTracking.Application.Commands.Auth;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class RevokeTokenCommandHandler
        : IRequestHandler<RevokeTokenCommand, Unit>
    {
        private readonly IUnitOfWork _uow;

        public RevokeTokenCommandHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Unit> Handle(
            RevokeTokenCommand request, CancellationToken ct)
        {
            var token = await _uow.RefreshTokens
                .GetByTokenAsync(request.RefreshToken, ct)
                ?? throw new NotFoundException("RefreshToken", request.RefreshToken);

            if (!token.IsActive)
                throw new DomainException(
                    "Token is already revoked or expired.");

            token.Revoke("User logged out");
            _uow.RefreshTokens.Update(token);
            await _uow.SaveChangesAsync(ct);

            return Unit.Value;
        }
    }
}
