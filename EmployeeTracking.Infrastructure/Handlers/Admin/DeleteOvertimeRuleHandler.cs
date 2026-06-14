using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers.Admin
{
    public class DeleteOvertimeRuleHandler
        : IRequestHandler<DeleteOvertimeRuleCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        public DeleteOvertimeRuleHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<Unit> Handle(
            DeleteOvertimeRuleCommand request, CancellationToken ct)
        {
            var rule = await _uow.OvertimeRules.GetByIdAsync(request.Id, ct)
                ?? throw new NotFoundException(nameof(OvertimeRule), request.Id);

            _uow.OvertimeRules.Delete(rule);
            await _uow.SaveChangesAsync(ct);
            return Unit.Value;
        }
    }
}
