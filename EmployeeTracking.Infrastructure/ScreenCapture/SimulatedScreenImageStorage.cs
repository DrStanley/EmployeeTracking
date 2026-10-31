using EmployeeTracking.Application.ScreenCapture;

namespace EmployeeTracking.Infrastructure.ScreenCapture
{
    public class SimulatedScreenImageStorage : IScreenImageStorage
    {
        public Task<string> SaveAsync(Guid employeeId, Stream content, string contentType, CancellationToken ct = default)
            => Task.FromResult($"simulated://{employeeId:N}/{Guid.NewGuid():N}");

        public Task<byte[]?> ReadAsync(string storageKey, CancellationToken ct = default)
            => Task.FromResult<byte[]?>(null);
    }
}
