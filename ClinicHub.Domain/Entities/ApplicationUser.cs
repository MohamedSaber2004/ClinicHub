using ClinicHub.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; private set; } = null!;
        public DateTime BirthDate { get; private set; }
        public Gender Gender { get; private set; }
        public string? ProfilePictureUrl { get; private set; }
        public string? PasswordResetToken { get; private set; }
        public DateTime? PasswordResetTokenExpiry { get; private set; }
        public LanguageCode Language { get; private set; } = LanguageCode.en;

        public string? VerificationCode { get; private set; }
        public DateTime? VerificationCodeExpiry { get; private set; }
        public string? FacebookUserId { get; private set; }
        public string? GoogleUserId { get; private set; }

        public static ApplicationUser Create(string fullName, string email, string phoneNumber, DateTime birthDate, Gender gender) => new()
        {
            FullName = fullName,
            Email = email,
            UserName = email,
            PhoneNumber = phoneNumber,
            BirthDate = birthDate,
            Gender = gender
        };

        public void UpdateLanguage(LanguageCode language) => Language = language;

        public void UpdateFullName(string fullName) => FullName = fullName;

        public void UpdateProfile(string fullName, string phoneNumber, DateTime birthDate, Gender gender)
        {
            FullName = fullName;
            PhoneNumber = phoneNumber;
            BirthDate = birthDate;
            Gender = gender;
        }

        public void SetVerificationCode(string code, DateTime expiry)
        {
            VerificationCode = code;
            VerificationCodeExpiry = expiry;
        }

        public void ClearVerificationCode()
        {
            VerificationCode = null;
            VerificationCodeExpiry = null;
        }

        public void SetPasswordResetToken(string token, DateTime expiry)
        {
            PasswordResetToken = token;
            PasswordResetTokenExpiry = expiry;
        }

        public void ClearPasswordResetToken()
        {
            PasswordResetToken = null;
            PasswordResetTokenExpiry = null;
        }

        public void SetFacebookUserId(string facebookUserId)
        {
            FacebookUserId = facebookUserId;
        }

        public void SetGoogleUserId(string googleUserId)
        {
            GoogleUserId = googleUserId;
        }

        public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; private set; } = new List<UserRefreshToken>();

        public virtual ICollection<UserFbToken> UserFbTokens { get; private set; } = new List<UserFbToken>();
        public virtual ICollection<Notification> Notifications { get; private set; } = new List<Notification>();
    }
}
