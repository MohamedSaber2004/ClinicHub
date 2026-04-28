using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace ClinicHub.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetToken, CancellationToken ct = default)
        {
            var subject = "ClinicHub - Password Reset Request";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Hello {fullName},</h2>
                    <p>We received a request to reset your password for your ClinicHub account.</p>
                    <p>Please use the following token to reset your password. This token will expire in {_settings.ForgetPasswordExpiryMinutes} minutes.</p>
                    <div style='background-color: #f4f4f4; padding: 10px; border-radius: 5px; font-weight: bold; font-size: 1.2em; text-align: center;'>
                        {resetToken}
                    </div>
                    <p>If you did not request a password reset, please ignore this email.</p>
                    <p>Regards,<br />ClinicHub Team</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, fullName, subject, body, ct);
        }

        public async Task SendVerificationEmailAsync(string toEmail, string fullName, string verificationCode, CancellationToken ct = default)
        {
            var subject = "ClinicHub - Account Verification";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Hello {fullName},</h2>
                    <p>Welcome to ClinicHub! Please use the code below to verify your account:</p>
                    <div style='background-color: #f4f4f4; padding: 10px; border-radius: 5px; font-weight: bold; font-size: 1.5em; text-align: center; letter-spacing: 5px;'>
                        {verificationCode}
                    </div>
                    <p>This code will expire shortly.</p>
                    <p>Regards,<br />ClinicHub Team</p>
                </body>
                </html>";

            await SendEmailAsync(toEmail, fullName, subject, body, ct);
        }

        private async Task SendEmailAsync(string toEmail, string fullName, string subject, string body, CancellationToken ct)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.Name, _settings.Email));
            email.To.Add(new MailboxAddress(fullName, toEmail));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password, ct);
            await smtp.SendAsync(email, ct);
            await smtp.DisconnectAsync(true, ct);
        }
    }
}
