namespace ClinicHub.Application.Common.Models
{
    public class EmailSettings
    {
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int ForgetPasswordExpiryMinutes { get; set; }
        public int VerificationCodeExpiryMinutes { get; set; }
    }
}
