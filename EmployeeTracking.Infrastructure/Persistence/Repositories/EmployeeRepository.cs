using EmployeeTracking.Domain.Entities;
using EmployeeTracking.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTracking.Infrastructure.Persistence.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context) => _context = context;

        public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Shift)
                .Include(e => e.AttendancePolicy)
                .FirstOrDefaultAsync(e => e.Id == id, ct);

        public async Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default)
            => await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == email, ct);

        public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default)
            => await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.IsActive)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Employee>> GetByDepartmentAsync(
            Guid departmentId, CancellationToken ct = default)
            => await _context.Employees
                .Where(e => e.DepartmentId == departmentId && e.IsActive)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Employee>> GetByManagerAsync(
            Guid managerId, CancellationToken ct = default)
            => await _context.Employees
                .Where(e => e.ManagerId == managerId && e.IsActive)
                .ToListAsync(ct);

        public async Task AddAsync(Employee employee, CancellationToken ct = default)
            => await _context.Employees.AddAsync(employee, ct);

        public void Update(Employee employee)
            => _context.Employees.Update(employee);

        public void Delete(Employee employee)
            => _context.Employees.Remove(employee);
    }
}
