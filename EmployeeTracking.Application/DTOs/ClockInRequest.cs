using EmployeeTracking.Domain.Enums;

namespace EmployeeTracking.Application.DTOs
{
    /// <summary>Request body for clock-in and clock-out operations.</summary>
    public record ClockInRequest(
        /// <summary>
        /// The method used to record this punch.
        /// <br/>- <b>0</b> = WebApp
        /// <br/>- <b>1</b> = MobileApp
        /// <br/>- <b>2</b> = Kiosk
        /// <br/>- <b>3</b> = ManagerEntry
        /// <br/>- <b>4</b> = SystemCorrection
        /// </summary>
        TimeEntrySource Source,

        /// <summary>GPS latitude. Optional — include for mobile/kiosk punches.</summary>
        double? Latitude,

        /// <summary>GPS longitude. Optional — include for mobile/kiosk punches.</summary>
        double? Longitude,

        /// <summary>Device or kiosk identifier. Optional.</summary>
        string? DeviceId
    );
}
