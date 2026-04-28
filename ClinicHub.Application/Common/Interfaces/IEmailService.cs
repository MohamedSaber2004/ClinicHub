namespace ClinicHub.Application.Common.Interfaces
{
    /// <summary>
    /// Service for sending transactional emails.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a password reset email to a user.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="fullName">The recipient's full name.</param>
        /// <param name="resetToken">The password reset token.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetToken, CancellationToken ct = default);

        /// <summary>
        /// Sends a verification email to a user.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="fullName">The recipient's full name.</param>
        /// <param name="verificationCode">The verification code.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendVerificationEmailAsync(string toEmail, string fullName, string verificationCode, CancellationToken ct = default);
    }
}
