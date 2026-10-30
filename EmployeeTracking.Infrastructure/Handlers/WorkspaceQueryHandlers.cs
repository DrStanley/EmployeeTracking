using EmployeeTracking.Application.Activity;
using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Application.Queries.Workspace;
using EmployeeTracking.Application.Time;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using MediatR;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class GetWeeklyHoursHandler : IRequestHandler<GetWeeklyHoursQuery, WeeklyHoursDto>
    {
        private readonly IUnitOfWork _uow;
        public GetWeeklyHoursHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<WeeklyHoursDto> Handle(GetWeeklyHoursQuery request, CancellationToken ct)
        {
            var today = DateTime.Today;
            var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
            var from = new DateTimeOffset(monday, TimeZoneInfo.Local.GetUtcOffset(monday));
            var entries = await _uow.TimeEntries.GetByEmployeeAndDateRangeAsync(
                request.EmployeeId, from, from.AddDays(7), ct);
            var byDay = WorkedHoursCalculator.ByLocalDay(entries);

            decimal Day(int offset) => byDay.GetValueOrDefault(DateOnly.FromDateTime(monday.AddDays(offset)));
            var days = Enumerable.Range(0, 7).Select(Day).ToArray();
            return new WeeklyHoursDto(days[0], days[1], days[2], days[3], days[4], days[5], days[6],
                Math.Round(days.Sum(), 2, MidpointRounding.AwayFromZero));
        }
    }

    public class GetWorkedHoursHandler : IRequestHandler<GetWorkedHoursQuery, WorkedHoursDto>
    {
        private readonly IUnitOfWork _uow;
        public GetWorkedHoursHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<WorkedHoursDto> Handle(GetWorkedHoursQuery request, CancellationToken ct)
        {
            AppActivityCalculator.EnsureRange(request.From, request.To);
            var targetId = request.EmployeeId ?? request.RequesterId;
            if (targetId == Guid.Empty)
                throw new DomainException("Select an employee to view hours.");
            await EnsureCanViewAsync(request, targetId, ct);

            var entries = await _uow.TimeEntries.GetByEmployeeAndDateRangeAsync(
                targetId, request.From.AddDays(-1), request.To, ct);
            var hours = WorkedHoursCalculator.Pairs(entries)
                .Where(pair => pair.Start >= request.From && pair.Start < request.To)
                .Sum(pair => pair.Hours);
            return new WorkedHoursDto(Math.Round(hours, 2, MidpointRounding.AwayFromZero));
        }

        private async Task EnsureCanViewAsync(GetWorkedHoursQuery request, Guid targetId, CancellationToken ct)
        {
            if (targetId == request.RequesterId || request.IsAdmin)
                return;
            if (request.IsManager)
            {
                var reports = await _uow.Employees.GetByManagerAsync(request.RequesterId, ct);
                if (reports.Any(employee => employee.Id == targetId))
                    return;
            }
            throw new DomainException("You can only view your own hours or your direct reports.");
        }
    }

    public class GetPresenceHandler : IRequestHandler<GetPresenceQuery, IReadOnlyList<PresenceDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetPresenceHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<PresenceDto>> Handle(GetPresenceQuery request, CancellationToken ct)
        {
            var employees = await _uow.Employees.GetAllAsync(ct);
            var entries = await _uow.TimeEntries.GetSinceAsync(DateTimeOffset.UtcNow.AddDays(-3), ct);
            var byEmployee = entries.GroupBy(entry => entry.EmployeeId)
                .ToDictionary(group => group.Key, group => group.ToList());

            return employees
                .Select(employee =>
                {
                    byEmployee.TryGetValue(employee.Id, out var mine);
                    var state = ClockStateReader.Read(mine ?? new List<TimeEntry>());
                    return new PresenceDto(
                        employee.Id,
                        employee.FullName,
                        employee.JobTitle,
                        state.Status,
                        state.Status == "on-break" ? state.BreakStartedAt : state.ClockedInAt);
                })
                .OrderBy(person => person.Status switch
                {
                    "clocked-in" => 0,
                    "on-break" => 1,
                    _ => 2
                })
                .ThenBy(person => person.FullName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }

    public class GetEmployeeProfileHandler : IRequestHandler<GetEmployeeProfileQuery, EmployeeProfileDto>
    {
        private readonly IUnitOfWork _uow;
        public GetEmployeeProfileHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<EmployeeProfileDto> Handle(GetEmployeeProfileQuery request, CancellationToken ct)
        {
            if (request.EmployeeId == Guid.Empty)
                throw new DomainException("Employee profile was not found.");

            var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);
            var balance = await _uow.PTOBalances.GetByEmployeeAndYearAsync(
                employee.Id, DateTime.UtcNow.Year, ct);

            return new EmployeeProfileDto(
                employee.Id,
                employee.FullName,
                employee.Email,
                employee.JobTitle,
                employee.Location,
                employee.Department?.Name ?? "Unassigned",
                employee.Manager?.FullName,
                employee.Shift?.Name,
                employee.Shift?.PlannedStart.ToString("HH:mm"),
                employee.Shift?.PlannedEnd.ToString("HH:mm"),
                employee.EmploymentType.ToString(),
                balance?.AvailableHours ?? 0m);
        }
    }

    public class GetScheduleHandler : IRequestHandler<GetScheduleQuery, ScheduleDto>
    {
        private readonly IUnitOfWork _uow;
        public GetScheduleHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<ScheduleDto> Handle(GetScheduleQuery request, CancellationToken ct)
        {
            var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new NotFoundException(nameof(Employee), request.EmployeeId);
            var shift = employee.Shift is { IsActive: true } ? employee.Shift : null;
            var today = DateOnly.FromDateTime(DateTime.Today);
            var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
            var days = Enumerable.Range(0, 7).Select(offset =>
            {
                var date = monday.AddDays(offset);
                var workday = date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday;
                return new ScheduleDayDto(
                    date,
                    date.DayOfWeek.ToString(),
                    date == today,
                    workday && shift is not null,
                    workday ? shift?.PlannedStart.ToString("HH:mm") : null,
                    workday ? shift?.PlannedEnd.ToString("HH:mm") : null);
            }).ToList();

            return new ScheduleDto(
                shift?.Name,
                shift?.PlannedStart.ToString("HH:mm"),
                shift?.PlannedEnd.ToString("HH:mm"),
                days);
        }
    }

    public class GetCalendarMonthHandler : IRequestHandler<GetCalendarMonthQuery, CalendarMonthDto>
    {
        private readonly IUnitOfWork _uow;
        public GetCalendarMonthHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<CalendarMonthDto> Handle(GetCalendarMonthQuery request, CancellationToken ct)
        {
            if (request.Month is < 1 or > 12)
                throw new DomainException("Month must be between 1 and 12.");

            var start = new DateOnly(request.Year, request.Month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            var holidays = (await _uow.Holidays.GetByYearAsync(request.Year, ct))
                .Select(holiday => (Holiday: holiday, Date: OccursOn(holiday, request.Year)))
                .Where(item => item.Date is not null && item.Date.Value >= start && item.Date.Value <= end)
                .Select(item => new CalendarHolidayDto(item.Date!.Value, item.Holiday.Name))
                .OrderBy(holiday => holiday.Date)
                .ToList();

            var visibleIds = await VisibleEmployeeIdsAsync(request, ct);
            var absences = (await _uow.PTORequests.GetApprovedOverlappingAsync(start, end, ct))
                .Where(requestItem => visibleIds.Contains(requestItem.EmployeeId))
                .Select(requestItem => new CalendarAbsenceDto(
                    requestItem.EmployeeId,
                    requestItem.Employee?.FullName ?? "Employee",
                    requestItem.StartDate,
                    requestItem.EndDate,
                    requestItem.HoursRequested))
                .ToList();

            return new CalendarMonthDto(request.Year, request.Month, holidays, absences);
        }

        private static DateOnly? OccursOn(Holiday holiday, int year)
        {
            if (!holiday.IsRecurringAnnually)
                return holiday.Date.Year == year ? holiday.Date : null;
            try
            {
                return new DateOnly(year, holiday.Date.Month, holiday.Date.Day);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        private async Task<HashSet<Guid>> VisibleEmployeeIdsAsync(
            GetCalendarMonthQuery request, CancellationToken ct)
        {
            if (request.IsAdmin)
                return (await _uow.Employees.GetAllAsync(ct)).Select(employee => employee.Id).ToHashSet();

            var ids = new HashSet<Guid>();
            if (request.RequesterId != Guid.Empty)
                ids.Add(request.RequesterId);
            if (request.IsManager)
            {
                foreach (var report in await _uow.Employees.GetByManagerAsync(request.RequesterId, ct))
                    ids.Add(report.Id);
            }
            return ids;
        }
    }

    public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetAuditLogsHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<IReadOnlyList<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken ct)
        {
            var logs = await _uow.AuditLogs.GetRecentAsync(100, ct);
            var names = new Dictionary<Guid, string>();
            var result = new List<AuditLogDto>();
            foreach (var log in logs)
            {
                string? name = null;
                if (log.PerformedBy is Guid performer && performer != Guid.Empty)
                {
                    if (!names.TryGetValue(performer, out name))
                    {
                        name = (await _uow.Employees.GetByIdAsync(performer, ct))?.FullName;
                        names[performer] = name ?? "Unknown";
                        name = names[performer];
                    }
                }
                result.Add(new AuditLogDto(
                    log.Id, log.EntityName, log.EntityId, log.Action, log.NewValues, name, log.Timestamp));
            }
            return result;
        }
    }
}
