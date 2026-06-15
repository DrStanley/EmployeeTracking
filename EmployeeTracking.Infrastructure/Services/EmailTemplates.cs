using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeTracking.Infrastructure.Services
{
    public static class EmailTemplates
    {
        private static string Wrap(string title, string body) => $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8""/>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1""/>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
            background: #f4f4f5;
            margin: 0;
            padding: 0;
        }}

        .container {{
            max-width: 600px;
            margin: 32px auto;
            background: #fff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 1px 3px rgba(0,0,0,.1);
        }}

        .header {{
            background: #1e40af;
            padding: 24px 32px;
        }}

        .header h1 {{
            color: #fff;
            margin: 0;
            font-size: 20px;
        }}

        .body {{
            padding: 32px;
            color: #374151;
            line-height: 1.6;
        }}

        .body h2 {{
            color: #1e40af;
            margin-top: 0;
        }}

        .badge {{
            display: inline-block;
            padding: 4px 12px;
            border-radius: 9999px;
            font-size: 13px;
            font-weight: 600;
        }}

        .badge-blue {{
            background: #dbeafe;
            color: #1e40af;
        }}

        .badge-green {{
            background: #dcfce7;
            color: #15803d;
        }}

        .badge-red {{
            background: #fee2e2;
            color: #dc2626;
        }}

        .badge-yellow {{
            background: #fef9c3;
            color: #ca8a04;
        }}

        .info-box {{
            background: #f8fafc;
            border-left: 4px solid #1e40af;
            padding: 16px;
            margin: 20px 0;
            border-radius: 0 6px 6px 0;
        }}

        .footer {{
            background: #f8fafc;
            padding: 20px 32px;
            text-align: center;
            font-size: 13px;
            color: #6b7280;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Employee Tracking System</h1>
        </div>

        <div class=""body"">
            <h2>{title}</h2>
            {body}
        </div>

        <div class=""footer"">
            This is an automated message. Please do not reply to this email.<br/>
            © {DateTime.UtcNow.Year} Employee Tracking System
        </div>
    </div>
</body>
</html>";

        public static string ClockInConfirmation(
            string employeeName, DateTimeOffset timestamp) =>
            Wrap("Clock-In Confirmed", $"""
            <p>Hi <strong>{employeeName}</strong>,</p>
            <p>Your clock-in has been recorded successfully.</p>
            <div class="info-box">
              <strong>Time:</strong> {timestamp:dddd, MMMM dd yyyy 'at' HH:mm} UTC
            </div>
            <p>Remember to clock out at the end of your shift.</p>
            """);

        public static string MissedPunchAlert(
            string employeeName, DateTimeOffset openSince) =>
            Wrap("Missed Punch Detected", $"""
            <p>Hi <strong>{employeeName}</strong>,</p>
            <p>We detected an open clock-in with no matching clock-out.</p>
            <div class="info-box">
              <strong>Clocked in:</strong> {openSince:dddd, MMMM dd yyyy 'at' HH:mm} UTC
            </div>
            <p>Please contact your manager to correct this entry as soon as possible.</p>
            """);

        public static string TimesheetSubmitted(
            string managerName, string employeeName,
            string payPeriodName, Guid timesheetId) =>
            Wrap("Timesheet Pending Your Approval", $"""
            <p>Hi <strong>{managerName}</strong>,</p>
            <p><strong>{employeeName}</strong> has submitted a timesheet for
               <strong>{payPeriodName}</strong> and it is awaiting your review.</p>
            <div class="info-box">
              <strong>Employee:</strong> {employeeName}<br/>
              <strong>Pay Period:</strong> {payPeriodName}<br/>
              <strong>Timesheet ID:</strong> {timesheetId}
            </div>
            <p>Please log in to review and approve or reject this timesheet.</p>
            """);

        public static string TimesheetApproved(
            string employeeName, string payPeriodName,
            decimal regularHours, decimal overtimeHours) =>
            Wrap("Timesheet Approved", $"""
            <p>Hi <strong>{employeeName}</strong>,</p>
            <p>Your timesheet for <strong>{payPeriodName}</strong>
               has been <span class="badge badge-green">Approved</span>.</p>
            <div class="info-box">
              <strong>Regular Hours:</strong> {regularHours:F2}h<br/>
              <strong>Overtime Hours:</strong> {overtimeHours:F2}h<br/>
              <strong>Total:</strong> {(regularHours + overtimeHours):F2}h
            </div>
            """);

        public static string TimesheetRejected(
            string employeeName, string payPeriodName, string reason) =>
            Wrap("Timesheet Returned for Correction", $"""
            <p>Hi <strong>{employeeName}</strong>,</p>
            <p>Your timesheet for <strong>{payPeriodName}</strong>
               has been <span class="badge badge-red">Rejected</span>.</p>
            <div class="info-box">
              <strong>Reason:</strong> {reason}
            </div>
            <p>Please review and resubmit your timesheet.</p>
            """);

        public static string PTORequestSubmitted(
            string managerName, string employeeName,
            DateOnly start, DateOnly end, decimal hours) =>
            Wrap("PTO Request Pending Approval", $"""
            <p>Hi <strong>{managerName}</strong>,</p>
            <p><strong>{employeeName}</strong> has submitted a PTO request
               requiring your approval.</p>
            <div class="info-box">
              <strong>From:</strong> {start:MMMM dd, yyyy}<br/>
              <strong>To:</strong>   {end:MMMM dd, yyyy}<br/>
              <strong>Hours:</strong> {hours:F1}h
            </div>
            """);

        public static string PTOApproved(
            string employeeName, DateOnly start,
            DateOnly end, decimal hours) =>
            Wrap("PTO Request Approved", $"""
            <p>Hi <strong>{employeeName}</strong>,</p>
            <p>Your PTO request has been
               <span class="badge badge-green">Approved</span>.</p>
            <div class="info-box">
              <strong>From:</strong> {start:MMMM dd, yyyy}<br/>
              <strong>To:</strong>   {end:MMMM dd, yyyy}<br/>
              <strong>Hours:</strong> {hours:F1}h
            </div>
            <p>Enjoy your time off!</p>
            """);

        public static string PTORejected(
            string employeeName, DateOnly start,
            DateOnly end, string reason) =>
            Wrap("PTO Request Declined", $"""
            <p>Hi <strong>{employeeName}</strong>,</p>
            <p>Your PTO request has been
               <span class="badge badge-red">Declined</span>.</p>
            <div class="info-box">
              <strong>Dates:</strong> {start:MMM dd} – {end:MMM dd, yyyy}<br/>
              <strong>Reason:</strong> {reason}
            </div>
            <p>Please speak with your manager if you have any questions.</p>
            """);

        public static string OvertimeAlert(
            string employeeName, decimal hoursWorked,
            decimal threshold) =>
            Wrap("Overtime Alert", $"""
            <p>Hi <strong>{employeeName}</strong>,</p>
            <p>You have worked <strong>{hoursWorked:F1} hours</strong> this week,
               which exceeds the <strong>{threshold:F0}-hour</strong> overtime
               threshold.</p>
            <div class="info-box">
              <span class="badge badge-yellow">⚠ Overtime</span>
              &nbsp;{hoursWorked:F1}h worked this week
            </div>
            <p>Your manager has been notified. Please contact them if needed.</p>
            """);

        public static string LowPTOBalance(
            string employeeName, decimal remainingHours) =>
            Wrap("Low PTO Balance", $"""
            <p>Hi <strong>{employeeName}</strong>,</p>
            <p>Your PTO balance is running low.</p>
            <div class="info-box">
              <strong>Remaining Balance:</strong>
              <span class="badge badge-yellow">{remainingHours:F1}h</span>
            </div>
            <p>Please plan your time off accordingly.</p>
            """);

        public static string WelcomeEmail(
            string employeeName, string email, string temporaryPassword) =>
            Wrap("Welcome to Employee Tracking System", $"""
            <p>Hi <strong>{employeeName}</strong>,</p>
            <p>Your employee account has been created. Here are your details:</p>
            <div class="info-box">
              <strong>Email:</strong>    {email}<br/>
            </div>
            <p><strong>Please change your password immediately after your first login.</strong></p>
            """);
    }
}
