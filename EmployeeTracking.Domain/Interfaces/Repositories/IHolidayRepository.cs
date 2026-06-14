using EmployeeTracking.Domain.Entities;

namespace EmployeeTracking.Domain.Interfaces.Repositories
{
    public interface IHolidayRepository
    {
        Task<Holiday?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Holiday>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Holiday>> GetByYearAsync(int year, CancellationToken ct = default);
        Task AddAsync(Holiday holiday, CancellationToken ct = default);
        void Update(Holiday holiday);
        void Delete(Holiday holiday);
    }
}
