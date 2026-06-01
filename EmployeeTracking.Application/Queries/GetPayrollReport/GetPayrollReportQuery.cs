using EmployeeTracking.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracking.Application.Queries.GetPayrollReport
{
    public record GetPayrollReportQuery(
     Guid PayPeriodId
 ) : IRequest<PayrollReportDto>;
}
