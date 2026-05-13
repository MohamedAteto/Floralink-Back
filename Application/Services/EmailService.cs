using System.Net;
using System.Net.Mail;
using FloraLink.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FloraLink.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    private readonly Dictionary<(int plantId, string severity), DateTime> _lastSent = new();
    private readonly TimeSpan _cooldown = TimeSpan.FromHours(1);

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendPlantAlertAsync(string toEmail, string ownerName, string plantName, string message, string severity)
    {
        var smtpHost = _config["Email:SmtpHost"];
        var senderEmail = _config["Email:SenderEmail"];
        var senderPassword = _config["Email:SenderPassword"];

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword))
        {
            _logger.LogWarning("Email not configured — skipping alert email to {Email}", toEmail);
            return;
        }

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("No recipient email address — skipping alert email");
            return;
        }

        var smtpPort = int.TryParse(_config["Email:SmtpPort"], out var port) ? port : 587;
        var senderName = _config["Email:SenderName"] ?? "FloraLink Alerts";

        var severityColor = severity == "Critical" ? "#dc2626" : "#d97706";
        var severityIcon = severity == "Critical" ? "🚨" : "⚠️";
        var subject = $"{severityIcon} FloraLink Alert: {plantName} needs attention";

        var body = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;background:#f0fdf4;font-family:Arial,sans-serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background:#f0fdf4;padding:32px 0;'>
    <tr><td align='center'>
      <table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);'>
        <tr>
          <td style='background:linear-gradient(135deg,#16a34a,#15803d);padding:32px;text-align:center;'>
            <div style='font-size:48px;margin-bottom:8px;'>🌿</div>
            <h1 style='color:#ffffff;margin:0;font-size:24px;font-weight:700;letter-spacing:-0.5px;'>FloraLink</h1>
            <p style='color:#bbf7d0;margin:4px 0 0;font-size:14px;'>Plant Health Monitoring</p>
          </td>
        </tr>
        <tr>
          <td style='padding:32px;'>
            <div style='background:{severityColor}15;border:2px solid {severityColor};border-radius:12px;padding:20px;margin-bottom:24px;text-align:center;'>
              <span style='font-size:32px;'>{severityIcon}</span>
              <h2 style='color:{severityColor};margin:8px 0 4px;font-size:20px;font-weight:700;'>{severity} Alert</h2>
              <p style='color:#374151;margin:0;font-size:16px;font-weight:500;'>{plantName}</p>
            </div>
            <p style='color:#6b7280;font-size:15px;margin:0 0 16px;'>Hi {ownerName},</p>
            <p style='color:#374151;font-size:15px;line-height:1.6;margin:0 0 24px;background:#f9fafb;border-radius:8px;padding:16px;border-left:4px solid {severityColor};'>{message}</p>
            <p style='color:#6b7280;font-size:14px;margin:0 0 24px;'>Please check on your plant as soon as possible and take the necessary action to restore it to optimal health.</p>
            <div style='text-align:center;margin:24px 0;'>
              <a href='https://floralinkproject.netlify.app' style='display:inline-block;background:linear-gradient(135deg,#16a34a,#15803d);color:#ffffff;text-decoration:none;padding:14px 32px;border-radius:8px;font-size:15px;font-weight:600;'>Open FloraLink Dashboard</a>
            </div>
          </td>
        </tr>
        <tr>
          <td style='background:#f9fafb;padding:16px 32px;border-top:1px solid #e5e7eb;text-align:center;'>
            <p style='color:#9ca3af;font-size:12px;margin:0;'>You're receiving this because you have plant alerts enabled on FloraLink.<br/>© 2025 FloraLink — Plant Monitoring Made Simple</p>
          </td>
        </tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(senderEmail, senderPassword)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
            _logger.LogInformation("Alert email sent to {Email} for plant {Plant} ({Severity})", toEmail, plantName, severity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send alert email to {Email}", toEmail);
        }
    }

    public bool ShouldSendEmail(int plantId, string severity)
    {
        var key = (plantId, severity);
        if (_lastSent.TryGetValue(key, out var last) && DateTime.UtcNow - last < _cooldown)
            return false;
        _lastSent[key] = DateTime.UtcNow;
        return true;
    }
}
