using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Employee>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default);
        Task<IReadOnlyList<Employee>> GetByManagerAsync(Guid managerId, CancellationToken ct = default);
        Task AddAsync(Employee employee, CancellationToken ct = default);
        void Update(Employee employee);
        void Delete(Employee employee);
    }
}
