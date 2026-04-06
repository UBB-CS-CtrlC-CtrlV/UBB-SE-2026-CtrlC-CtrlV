using System.Net;
using System.Net.Mail;
using BankApp.Infrastructure.Services.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BankApp.Infrastructure.Services.Infrastructure.Implementations
{
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
            string subject = "BankApp - Account Locked";
            string body = "Hello,\n\nYour account has been temporarily locked due to multiple failed login attempts. Please try again later or reset your password.";
            SendEmail(email, subject, body);
        }

        /// <inheritdoc />
        public void SendLoginAlert(string email)
        {
            string subject = "BankApp - New Login Detected";
            string body = "Hello,\n\nWe detected a new login to your BankApp account. If this was you, no action is needed. If this wasn't you, please change your password immediately.";
            SendEmail(email, subject, body);
        }

        /// <inheritdoc />
        public void SendOTPCode(string email, string code)
        {
            string subject = "Your BankApp Login Code";
            string body = $"Hello,\n\nYour One-Time Password (OTP) is: {code}\n\nThis code is valid for 5 minutes. Do not share it with anyone.";
            SendEmail(email, subject, body);
        }

        /// <inheritdoc />
        public void SendPasswordResetLink(string email, string token)
        {
            string subject = "BankApp - Password Reset Code";
            string body = $"Hello,\n\nYou requested a password reset. Please copy and paste the recovery code below into the app:\n\n{token}\n\nIf you did not request this, please ignore this email.";
            SendEmail(email, subject, body);
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
}
