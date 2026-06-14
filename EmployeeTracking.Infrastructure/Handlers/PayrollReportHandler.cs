using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.GetPayrollReport;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracking.Infrastructure.Handlers
{
    // Fetch existing report
    public class GetPayrollReportQueryHandler
        : IRequestHandler<GetPayrollReportQuery, PayrollReportDto>
    {
        private readonly IUnitOfWork _uow;

        public GetPayrollReportQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<PayrollReportDto> Handle(
            GetPayrollReportQuery request, CancellationToken ct)
        {
            var period = await _uow.PayPeriods.GetByIdAsync(request.PayPeriodId, ct)
                ?? throw new NotFoundException(nameof(PayPeriod), request.PayPeriodId);

            var reports = await _uow.PayrollReports
                .GetByPayPeriodAsync(request.PayPeriodId, ct);

            if (!reports.Any())
                throw new NotFoundException(
                    "PayrollReport",
                    $"No report generated yet for period '{period.Name}'. " +
                    "Call POST /api/reports/payroll/generate first.");

            return PayrollDtoHelper.BuildDto(period, reports);
        }
    }

    // Generate (or regenerate) a report for a pay period
    public class GeneratePayrollReportCommandHandler
        : IRequestHandler<GeneratePayrollReportCommand, PayrollReportDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly ITimesheetCalculationService _calculator;

        public GeneratePayrollReportCommandHandler(
            IUnitOfWork uow,
            ITimesheetCalculationService calculator)
        {
            _uow = uow;
            _calculator = calculator;
        }

        public async Task<PayrollReportDto> Handle(
            GeneratePayrollReportCommand request, CancellationToken ct)
        {
            var period = await _uow.PayPeriods.GetByIdAsync(request.PayPeriodId, ct)
                ?? throw new NotFoundException(nameof(PayPeriod), request.PayPeriodId);

            if (period.IsLocked)
                throw new DomainException(
                    $"Pay period '{period.Name}' is locked and cannot be regenerated.");

            // Get all active employees
            var employees = await _uow.Employees.GetAllAsync(ct);
            var reports = new List<PayrollReport>();

            foreach (var employee in employees)
            {
                // Calculate hours from time entries
                var (regular, overtime, pto, unpaid) = await _calculator
                    .CalculateTotalsAsync(employee.Id, period, ct);

                // Check timesheet status for exceptions
                var timesheet = await _uow.Timesheets
                    .GetByEmployeeAndPeriodAsync(employee.Id, request.PayPeriodId, ct);

                string? exceptionNotes = null;

                if (timesheet is null)
                    exceptionNotes = "No timesheet found for this period.";
                else if (timesheet.Status != TimesheetStatus.Approved)
                    exceptionNotes = $"Timesheet not approved — status: {timesheet.Status}.";

                // Remove existing report if regenerating
                var existing = await _uow.PayrollReports
                    .GetByEmployeeAndPeriodAsync(employee.Id, request.PayPeriodId, ct);

                if (existing is not null)
                {
                    var report = PayrollReport.Generate(
                        request.PayPeriodId,
                        employee.Id,
                        regular,
                        overtime,
                        pto,
                        unpaid,
                        exceptionNotes);

                    _uow.PayrollReports.Update(report);
                    reports.Add(report);
                }
                else
                {
                    var report = PayrollReport.Generate(
                        request.PayPeriodId,
                        employee.Id,
                        regular,
                        overtime,
                        pto,
                        unpaid,
                        exceptionNotes);

                    await _uow.PayrollReports.AddAsync(report, ct);
                    reports.Add(report);
                }
            }

            await _uow.SaveChangesAsync(ct);

            return BuildReportDto(period, reports, employees.ToList());
        }

        private static PayrollReportDto BuildReportDto(
            PayPeriod period,
            List<PayrollReport> reports,
            List<Employee> employees)
        {
            var lines = reports.Select(r =>
            {
                var emp = employees.FirstOrDefault(e => e.Id == r.EmployeeId);
                return new PayrollLineDto(
                    EmployeeId: r.EmployeeId,
                    EmployeeFullName: emp?.FullName ?? "Unknown",
                    EmployeeNumber: emp?.EmployeeNumber ?? "-",
                    Department: emp?.Department?.Name ?? "-",
                    RegularHours: r.RegularHours,
                    OvertimeHours: r.OvertimeHours,
                    PTOHours: r.PTOHours,
                    UnpaidHours: r.UnpaidHours,
                    TotalPayableHours: r.TotalPayableHours,
                    HasExceptions: r.HasExceptions,
                    ExceptionNotes: r.ExceptionNotes,
                    TimesheetStatus: r.HasExceptions ? "Pending" : "Approved");
            }).ToList();

            return new PayrollReportDto(
                PayPeriodId: period.Id,
                PayPeriodName: period.Name,
                StartDate: period.StartDate,
                EndDate: period.EndDate,
                Lines: lines,
                TotalRegularHours: lines.Sum(l => l.RegularHours),
                TotalOvertimeHours: lines.Sum(l => l.OvertimeHours),
                TotalPTOHours: lines.Sum(l => l.PTOHours),
                TotalUnpaidHours: lines.Sum(l => l.UnpaidHours),
                TotalEmployees: lines.Count,
                TotalExceptions: lines.Count(l => l.HasExceptions),
                GeneratedAt: DateTimeOffset.Now);
        }
    }

    // Shared helper used by GetPayrollReportQueryHandler
    file static class PayrollDtoHelper
    {
        internal static PayrollReportDto BuildDto(
            PayPeriod period,
            IReadOnlyList<PayrollReport> reports)
        {
            var lines = reports.Select(r => new PayrollLineDto(
                EmployeeId: r.EmployeeId,
                EmployeeFullName: r.Employee?.FullName ?? "Unknown",
                EmployeeNumber: r.Employee?.EmployeeNumber ?? "-",
                Department: r.Employee?.Department?.Name ?? "-",
                RegularHours: r.RegularHours,
                OvertimeHours: r.OvertimeHours,
                PTOHours: r.PTOHours,
                UnpaidHours: r.UnpaidHours,
                TotalPayableHours: r.TotalPayableHours,
                HasExceptions: r.HasExceptions,
                ExceptionNotes: r.ExceptionNotes,
                TimesheetStatus: r.HasExceptions ? "Pending" : "Approved"
            )).ToList();

            return new PayrollReportDto(
                PayPeriodId: period.Id,
                PayPeriodName: period.Name,
                StartDate: period.StartDate,
                EndDate: period.EndDate,
                Lines: lines,
                TotalRegularHours: lines.Sum(l => l.RegularHours),
                TotalOvertimeHours: lines.Sum(l => l.OvertimeHours),
                TotalPTOHours: lines.Sum(l => l.PTOHours),
                TotalUnpaidHours: lines.Sum(l => l.UnpaidHours),
                TotalEmployees: lines.Count,
                TotalExceptions: lines.Count(l => l.HasExceptions),
                GeneratedAt: DateTimeOffset.Now);
        }
    }
}
