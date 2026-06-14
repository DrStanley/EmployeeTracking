using EmployeeTracking.Application.DTOs;
using EmployeeTracking.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace EmployeeTracking.Infrastructure.Services
{
    public class ResendEmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _config;
        private readonly ILogger<ResendEmailService> _logger;

        public ResendEmailService(
            IResend resend,
            IConfiguration config,
            ILogger<ResendEmailService> logger)
        {
            _resend = resend;
            _config = config;
            _logger = logger;
        }

        private string From =>
            $"{_config["Resend:FromName"]} <{_config["Resend:FromEmail"]}>";

        public async Task<EmailResult> SendAsync(
           EmailMessages message, CancellationToken ct = default)
        {
            try
            {
                var email = new Resend.EmailMessage
                {
                    From = From,
                    Subject = message.Subject,
                    HtmlBody = message.HtmlBody,
                };

                email.To.Add(message.To);

                if (!string.IsNullOrWhiteSpace(message.PlainTextBody))
                    email.TextBody = message.PlainTextBody;

                var response = await _resend.EmailSendAsync(email, ct);

                _logger.LogInformation(
                    "Email sent to {To} — Subject: {Subject} — Id: {Id}",
                    message.To, message.Subject, response.Content.ToString());

                return new EmailResult(
                    Success: true,
                    MessageId: response.Content.ToString(),
                    Error: null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send email to {To} — Subject: {Subject}",
                    message.To, message.Subject);

                return new EmailResult(
                    Success: false,
                    MessageId: null,
                    Error: ex.Message);
            }
        }


        public async Task<EmailResult> SendBulkAsync(
            IEnumerable<EmailMessages> messages, CancellationToken ct = default)
        {
            var results = new List<EmailResult>();

            // Resend doesn't have a bulk endpoint in the .NET SDK yet
            // so we send sequentially with a small delay because of rate limits
            foreach (var message in messages)
            {
                var result = await SendAsync(message, ct);
                results.Add(result);

                if (!result.Success)
                    _logger.LogWarning(
                        "Bulk send — failed for {To}: {Error}",
                        message.To, result.Error);

                await Task.Delay(50, ct); // 50ms between sends
            }

            var failed = results.Count(r => !r.Success);
            if (failed > 0)
                _logger.LogWarning("Bulk send completed — {Failed} failed out of {Total}",
                    failed, results.Count);

            // Return aggregate result — success if at least one sent
            return results.Any(r => r.Success)
                ? new EmailResult(true, null, failed > 0 ? $"{failed} emails failed" : null)
                : new EmailResult(false, null, "All emails failed");
        }

    }
}
