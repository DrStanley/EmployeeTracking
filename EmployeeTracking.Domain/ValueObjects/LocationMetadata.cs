namespace EmployeeTracking.Domain.ValueObjects
{
    public sealed record LocationMetadata(
      double? Latitude,
      double? Longitude,
      string? DeviceId,
      string? IpAddress
  );
}
