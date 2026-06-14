namespace EmployeeTracking.Application.DTOs
{
    public record EmailMessages(
     string To,
     string Subject,
     string HtmlBody,
     string? PlainTextBody = null
 );

    public record EmailResult(
        bool Success,
        string? MessageId,
        string? Error
    );
}
