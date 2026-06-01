using EmployeeTracking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracking.Application.Interfaces
{
    public interface IOvertimeStrategy
    {
        (decimal regular, decimal overtime) Calculate(
            IReadOnlyList<DailyHoursDto> dailyHours,
            decimal dailyThreshold,
            decimal weeklyThreshold);
    }
}
