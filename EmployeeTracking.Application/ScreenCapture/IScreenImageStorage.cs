namespace EmployeeTracking.Application.ScreenCapture
{
    public interface IScreenImageStorage
    {
        Task<string> SaveAsync(Guid employeeId, Stream content, string contentType, CancellationToken ct = default);
        Task<byte[]?> ReadAsync(string storageKey, CancellationToken ct = default);
    }
}
