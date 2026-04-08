using System.Net;
using System.Net.Mail;
using BankApp.Server.Services.Notifications;

namespace BankApp.Server.Services.Notifications;

/// <summary>
/// Sends transactional emails using SMTP configuration from application settings.
/// </summary>
public class EmailService : IEmailService
{
    private const int DefaultSmtpPort = 587;
    private readonly IConfiguration configuration;
    private readonly ILogger<EmailService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailService"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration containing SMTP settings.</param>
    /// <param name="logger">Logger for email send failures.</param>
    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    /// <inheritdoc />
    public void SendLockNotification(string email)
    {
        SendEmail(email, EmailTemplates.AccountLockedSubject, EmailTemplates.AccountLockedBody);
    }

    /// <inheritdoc />
    public void SendLoginAlert(string email)
    {
        SendEmail(email, EmailTemplates.LoginAlertSubject, EmailTemplates.LoginAlertBody);
    }

    /// <inheritdoc />
    public void SendOTPCode(string email, string code)
    {
        SendEmail(email, EmailTemplates.OtpSubject, EmailTemplates.GetOtpBody(code));
    }

    /// <inheritdoc />
    public void SendPasswordResetLink(string email, string token)
    {
        SendEmail(email, EmailTemplates.PasswordResetSubject, EmailTemplates.GetPasswordResetBody(token));
    }

    private void SendEmail(string toEmail, string subject, string body)
    {
        try
        {
            string host = configuration["Email:SmtpHost"] ?? throw new InvalidOperationException("Email:SmtpHost is missing from configuration.");
            int port = int.Parse(configuration["Email:SmtpPort"] ?? throw new InvalidOperationException("Email:SmtpPort is missing from configuration."));
            string smtpUsername = configuration["Email:SmtpUser"] ?? throw new InvalidOperationException("Email:SmtpUser is missing from configuration.");
            string smtpPassword = configuration["Email:SmtpPass"] ?? throw new InvalidOperationException("Email:SmtpPass is missing from configuration.");
            string fromAddress = configuration["Email:FromAddress"] ?? smtpUsername;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true,
                UseDefaultCredentials = false,
            };
            using var mailMessage = new MailMessage(fromAddress, toEmail, subject, body);
            client.Send(mailMessage);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to send email to {ToEmail} with subject '{Subject}'.", toEmail, subject);
        }
    }
}