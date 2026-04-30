namespace ClinicHub.Application.Localization
{
    public static class LocalizationKeys
    {
        public static class ActionResults
        {
            public static readonly KeyString Ok = new("ActionResults.Ok");
            public static readonly KeyString Created = new("ActionResults.Created");
            public static readonly KeyString Accepted = new("ActionResults.Accepted");
            public static readonly KeyString Deleted = new("ActionResults.Deleted");
        }

        public static class ExceptionMessages
        {
            public static readonly KeyString Validation = new("Exceptions.Validation");
            public static readonly KeyString InvalidModelState = new("Exceptions.InvalidModelState");
            public static readonly KeyString NotFound = new("Exceptions.NotFound");
            public static readonly KeyString BadRequest = new("Exceptions.BadRequest");
            public static readonly KeyString Unauthorized = new("Exceptions.Unauthorized");
            public static readonly KeyString UnknownException = new("Exceptions.UnknownError");
        }

        public static class ValidationMessages
        {
            public static readonly KeyString Required = new("Validation.Required");
            public static readonly KeyString MinLength = new("Validation.MinLength");
            public static readonly KeyString MaxLength = new("Validation.MaxLength");
            public static readonly KeyString InvalidFormat = new("Validation.InvalidFormat");
            public static readonly KeyString InvalidEmail = new("Validation.InvalidEmail");
            public static readonly KeyString InvalidEnumValue = new("Validation.InvalidEnumValue");
            public static readonly KeyString MediaUrlsAndTypesMismatch = new("Validation.MediaUrlsAndTypesMismatch");
            public static readonly KeyString MinAge = new("Validation.MinAge");
        }

        public static class GeneralMessages
        {
            public static readonly KeyString Success = new("Messages.Success");
            public static readonly KeyString Error = new("Messages.Error");
            public static readonly KeyString Warning = new("Messages.Warning");
            public static readonly KeyString Info = new("Messages.Info");
        }

        public static class PostMessages
        {
            public static readonly KeyString NotFound = new("Posts.NotFound");
            public static readonly KeyString Created = new("Posts.Created");
            public static readonly KeyString Updated = new("Posts.Updated");
            public static readonly KeyString Deleted = new("Posts.Deleted");
        }

        public static class CommentMessages
        {
            public static readonly KeyString NotFound = new("Comments.NotFound");
            public static readonly KeyString Created = new("Comments.Created");
            public static readonly KeyString Updated = new("Comments.Updated");
            public static readonly KeyString Deleted = new("Comments.Deleted");
        }

        public static class ReactionMessages
        {
            public static readonly KeyString Toggled = new("Reactions.Toggled");
        }

        public static class AuthMessages
        {
            public static readonly KeyString SignupSuccess = new("Auth.SignupSuccess");
            public static readonly KeyString LoginSuccess = new("Auth.LoginSuccess");
            public static readonly KeyString InvalidCredentials = new("Auth.InvalidCredentials");
            public static readonly KeyString EmailAlreadyExists = new("Auth.EmailAlreadyExists");
            public static readonly KeyString WeakPassword = new("Auth.WeakPassword");
            public static readonly KeyString UserNotFound = new("Auth.UserNotFound");
            public static readonly KeyString ResetTokenSent = new("Auth.ResetTokenSent");
            public static readonly KeyString ResetTokenInvalid = new("Auth.ResetTokenInvalid");
            public static readonly KeyString PasswordResetSuccess = new("Auth.PasswordResetSuccess");
            public static readonly KeyString TokenValid = new("Auth.TokenValid");
            public static readonly KeyString PasswordMismatch = new("Auth.PasswordMismatch");
            public static readonly KeyString RefreshTokenInvalid = new("Auth.RefreshTokenInvalid");
            public static readonly KeyString TokenRefreshed = new("Auth.TokenRefreshed");
            public static readonly KeyString AccountNotVerified = new("Auth.AccountNotVerified");
            public static readonly KeyString InvalidVerificationCode = new("Auth.InvalidVerificationCode");
            public static readonly KeyString VerificationSuccess = new("Auth.VerificationSuccess");
            public static readonly KeyString ProfileUpdated = new("Auth.ProfileUpdated");
            public static readonly KeyString LanguageUpdated = new("Auth.LanguageUpdated");
            public static readonly KeyString InvalidFacebookToken = new("Auth.InvalidFacebookToken");
            public static readonly KeyString FacebookUserInfoError = new("Auth.FacebookUserInfoError");
            public static readonly KeyString FacebookUserCreationFailed = new("Auth.FacebookUserCreationFailed");
            public static readonly KeyString FacebookTokenRequired = new("Auth.FacebookTokenRequired");
            public static readonly KeyString FacebookEmailRequired = new("Auth.FacebookEmailRequired");
            public static readonly KeyString InvalidGoogleToken = new("Auth.InvalidGoogleToken");
            public static readonly KeyString GoogleUserInfoError = new("Auth.GoogleUserInfoError");
            public static readonly KeyString GoogleUserCreationFailed = new("Auth.GoogleUserCreationFailed");
            public static readonly KeyString GoogleTokenRequired = new("Auth.GoogleTokenRequired");
            public static readonly KeyString GoogleEmailRequired = new("Auth.GoogleEmailRequired");
            public static readonly KeyString InvalidEmail = new("Auth.InvalidEmail");
            public static readonly KeyString MustBeGmail = new("Auth.MustBeGmail");
            public static readonly KeyString LogoutSuccess = new("Auth.LogoutSuccess");
            public static readonly KeyString RefreshTokenRequired = new("Auth.RefreshTokenRequired");
        }

        public static class ClinicMessages
        {
            public static readonly KeyString InvalidLatitude = new("Clinics.InvalidLatitude");
            public static readonly KeyString InvalidLongitude = new("Clinics.InvalidLongitude");
            public static readonly KeyString InvalidRadius = new("Clinics.InvalidRadius");
        }
    }
}
