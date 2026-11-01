using EmployeeTracking.Application.Commands.People;
using EmployeeTracking.Application.Interfaces;
using EmployeeTracking.Domain.Common;
using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Enums;
using EmployeeTracking.Infrastructure.Identity;
using EmployeeTracking.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Handlers
{
    public class ListPeopleHandler : IRequestHandler<ListPeopleQuery, IReadOnlyList<PersonAccountDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _db;

        public ListPeopleHandler(UserManager<ApplicationUser> userManager, AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IReadOnlyList<PersonAccountDto>> Handle(ListPeopleQuery request, CancellationToken ct)
        {
            var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync(ct);
            var employees = await _db.Employees.AsNoTracking().ToListAsync(ct);
            var people = new List<PersonAccountDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                people.Add(PersonMapper.From(user, roles, employees));
            }
            return people;
        }
    }

    public class SetPersonRoleHandler : IRequestHandler<SetPersonRoleCommand, PersonAccountDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _db;

        public SetPersonRoleHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public async Task<PersonAccountDto> Handle(SetPersonRoleCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(request.UserId)
                ?? throw new DomainException("That account was not found.");
            await PersonRoles.Assign(_userManager, _roleManager, user, request.Role);
            var roles = await _userManager.GetRolesAsync(user);
            var employees = await _db.Employees.AsNoTracking().ToListAsync(ct);
            return PersonMapper.From(user, roles, employees);
        }
    }

    public class AddPersonHandler : IRequestHandler<AddPersonCommand, PersonAccountDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _uow;
        private readonly AppDbContext _db;

        public AddPersonHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork uow,
            AppDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _uow = uow;
            _db = db;
        }

        public async Task<PersonAccountDto> Handle(AddPersonCommand request, CancellationToken ct)
        {
            var role = PersonRoles.RequireAssignable(request.Role);
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName)
                || string.IsNullOrWhiteSpace(request.JobTitle) || string.IsNullOrWhiteSpace(request.EmployeeNumber))
                throw new DomainException("Name, job title, and employee number are required.");
            var email = request.Email.Trim();
            if (await _userManager.FindByEmailAsync(email) is not null)
                throw new DomainException("An account with this email already exists. Change their role instead.");
            if (await _uow.Employees.GetByEmailAsync(email, ct) is not null)
                throw new DomainException("An employee with this email already exists.");

            var number = request.EmployeeNumber.Trim();
            var numberTaken = await _db.Employees.AnyAsync(
                e => e.EmployeeNumber == number, ct);
            if (numberTaken)
                throw new DomainException($"Employee number '{number}' is already in use.");

            var department = await _db.Departments.OrderBy(d => d.Name).FirstOrDefaultAsync(ct)
                ?? throw new DomainException("Add a department before creating an employee.");
            var policy = await _db.AttendancePolicies.OrderBy(p => p.Name).FirstOrDefaultAsync(ct)
                ?? throw new DomainException("Add an attendance policy before creating an employee.");

            var employee = Employee.Create(
                number,
                request.FirstName.Trim(),
                request.LastName.Trim(),
                email,
                request.JobTitle.Trim(),
                department.Id,
                Guid.Empty,
                policy.Id,
                EmploymentType.FullTime);

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmployeeId = employee.Id
            };
            var created = await _userManager.CreateAsync(user, request.Password);
            if (!created.Succeeded)
            {
                var errors = string.Join(", ", created.Errors.Select(e => e.Description));
                throw new DomainException(errors);
            }

            await PersonRoles.Assign(_userManager, _roleManager, user, role);
            await _uow.Employees.AddAsync(employee, ct);
            await _uow.SaveChangesAsync(ct);

            var roles = await _userManager.GetRolesAsync(user);
            return PersonMapper.From(user, roles, new[] { employee });
        }
    }

    internal static class PersonRoles
    {
        public static string RequireAssignable(string? role)
        {
            var value = role?.Trim() ?? "";
            if (value is not ("Employee" or "Manager"))
                throw new DomainException("Role must be Employee or Manager.");
            return value;
        }

        public static async Task Assign(
            UserManager<ApplicationUser> users,
            RoleManager<IdentityRole> roles,
            ApplicationUser user,
            string role)
        {
            var value = RequireAssignable(role);
            if (await users.IsInRoleAsync(user, "Admin"))
                throw new DomainException("An admin account cannot be changed here.");

            foreach (var existing in new[] { "Employee", "Manager" })
            {
                if (await users.IsInRoleAsync(user, existing))
                    await users.RemoveFromRoleAsync(user, existing);
            }

            if (!await roles.RoleExistsAsync(value))
                await roles.CreateAsync(new IdentityRole(value));
            await users.AddToRoleAsync(user, value);
        }
    }

    internal static class PersonMapper
    {
        public static PersonAccountDto From(
            ApplicationUser user,
            IList<string> roles,
            IEnumerable<Employee> employees)
        {
            var employee = employees.FirstOrDefault(e => e.Id == user.EmployeeId)
                ?? employees.FirstOrDefault(e => string.Equals(e.Email, user.Email, StringComparison.OrdinalIgnoreCase));
            var isAdmin = roles.Contains("Admin");
            var role = isAdmin ? "Admin" : roles.Contains("Manager") ? "Manager" : "Employee";
            var status = AccountAccess.Resolve(user.AccountStatus, employee?.AccountStatus);
            return new PersonAccountDto(
                user.Id,
                employee?.Id ?? Guid.Empty,
                user.Email ?? "",
                employee?.FullName ?? user.Email ?? "",
                employee?.JobTitle ?? "",
                role,
                isAdmin,
                status.ToString());
        }
    }

    public class SetPersonStatusHandler : IRequestHandler<SetPersonStatusCommand, PersonAccountDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _uow;
        private readonly AppDbContext _db;

        public SetPersonStatusHandler(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork uow,
            AppDbContext db)
        {
            _userManager = userManager;
            _uow = uow;
            _db = db;
        }

        public async Task<PersonAccountDto> Handle(SetPersonStatusCommand request, CancellationToken ct)
        {
            if (!Enum.TryParse<AccountStatus>(request.AccountStatus, true, out var status)
                || !Enum.IsDefined(status))
                throw new DomainException("Status must be Active, Suspended, or Deleted.");

            var user = await _userManager.FindByIdAsync(request.UserId)
                ?? throw new DomainException("That account was not found.");
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                throw new DomainException("An admin account cannot be changed here.");

            user.AccountStatus = status;
            var updated = await _userManager.UpdateAsync(user);
            if (!updated.Succeeded)
                throw new DomainException(string.Join(", ", updated.Errors.Select(e => e.Description)));

            var employee = user.EmployeeId != Guid.Empty
                ? await _uow.Employees.GetByIdAsync(user.EmployeeId, ct)
                : await _uow.Employees.GetByEmailAsync(user.Email ?? "", ct);
            if (employee is not null)
            {
                employee.SetAccountStatus(status);
                _uow.Employees.Update(employee);
            }

            if (status != AccountStatus.Active)
                await _uow.RefreshTokens.RevokeAllForUserAsync(user.Id, status.ToString(), ct);

            await _uow.SaveChangesAsync(ct);

            var roles = await _userManager.GetRolesAsync(user);
            var employees = await _db.Employees.AsNoTracking().ToListAsync(ct);
            return PersonMapper.From(user, roles, employees);
        }
    }
}
