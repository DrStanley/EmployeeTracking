using EmployeeTracking.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracking.Application.Queries.PTO
{

    public record GetMyPTORequestsQuery(
        Guid EmployeeId
    ) : IRequest<IReadOnlyList<PTORequestDto>>;

    public record GetPendingPTOForManagerQuery(
        Guid ManagerId
    ) : IRequest<IReadOnlyList<PTORequestDto>>;

    public record GetPTOBalanceQuery(
        Guid EmployeeId,
        int Year
    ) : IRequest<PTOBalanceDto>;
}
