namespace EmployeeTracking.Domain.Enums
{
    public enum UserRole { Employee, Manager, Admin, System }

    public enum TimeEntryType
    {
        ClockIn, ClockOut, BreakStart, BreakEnd, ManagerCorrection
    }

    public enum TimeEntrySource
    {
        WebApp, MobileApp, Kiosk, ManagerEntry, SystemCorrection
    }

    public enum TimesheetStatus
    {
        Draft, Submitted, Approved, Rejected, Locked
    }

    public enum PTORequestStatus
    {
        Pending, Approved, Rejected, Cancelled
    }

    public enum ApprovalDecision
    {
        Approved, Rejected, ReturnedForCorrection
    }

    public enum NotificationType
    {
        MissedPunch, PendingApproval, TimesheetApproved,
        TimesheetRejected, LowPTOBalance, OvertimeAlert
    }

    public enum EmploymentType
    {
        FullTime, PartTime, Contract, Casual
    }

}
