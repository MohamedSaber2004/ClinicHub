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
            public static readonly KeyString UserNotFound = new("Validation.UserNotFound");
            public static readonly KeyString ConversationNotFound = new("Validation.ConversationNotFound");
            public static readonly KeyString MessageNotFound = new("Validation.MessageNotFound");
            public static readonly KeyString CannotChatWithYourself = new("Validation.CannotChatWithYourself");
            public static readonly KeyString UnauthorizedAction = new("Validation.UnauthorizedAction");
            public static readonly KeyString DeletedSuccessfully = new("Validation.DeletedSuccessfully");
            public static readonly KeyString DeletedFailed = new("Validation.DeletedFailed");
            public static readonly KeyString AtLeastOneFieldRequired = new("Validation.AtLeastOneFieldRequired");
            public static readonly KeyString InvalidOperation = new("Validation.InvalidOperation");
            public static readonly KeyString InvalidDate = new("Validation.InvalidDate");
            public static readonly KeyString InvalidTimeRange = new("Validation.InvalidTimeRange");
            public static readonly KeyString InvalidAge = new("Validation.InvalidAge");
            public static readonly KeyString PageNumberMustBeGreaterThanOrEqualToOne = new("Validation.PageNumberMustBeGreaterThanOrEqualToOne");
            public static readonly KeyString PageSizeMustBeGreaterThanOrEqualToOne = new("Validation.PageSizeMustBeGreaterThanOrEqualToOne");
            public static readonly KeyString PageSizeMustBeLessThanOrEqualToHundred = new("Validation.PageSizeMustBeLessThanOrEqualToHundred");
            public static readonly KeyString InvalidDateRange = new("Validation.InvalidDateRange");
            public static readonly KeyString InvalidRole = new("Validation.InvalidRole");
            public static readonly KeyString InvalidWorkingDays = new("Validation.InvalidWorkingDays");
            public static readonly KeyString MustBeGreaterThanZero = new("Validation.MustBeGreaterThanZero");
        }

        public static class AppointmentMessages
        {
            public static readonly KeyString DoctorNotAvailableAtThisTime = new("Appointments.DoctorNotAvailableAtThisTime");
            public static readonly KeyString TimeSlotAlreadyBooked = new("Appointments.TimeSlotAlreadyBooked");
            public static readonly KeyString DoctorNotFound = new("Appointments.DoctorNotFound");
            public static readonly KeyString AppointmentNotFound = new("Appointments.AppointmentNotFound");
            public static readonly KeyString NotAuthorizedToCancel = new("Appointments.NotAuthorizedToCancel");
            public static readonly KeyString CannotCancelAppointment = new("Appointments.CannotCancelAppointment");
            public static readonly KeyString Cancelled = new("Appointments.Cancelled");
            public static readonly KeyString NotAuthorizedToRespond = new("Appointments.NotAuthorizedToRespond");
            public static readonly KeyString CannotRespondAppointment = new("Appointments.CannotRespondAppointment");
            public static readonly KeyString Accepted = new("Appointments.Accepted");
            public static readonly KeyString Rejected = new("Appointments.Rejected");
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
            public static readonly KeyString PhoneNumberExistsBefore = new("Auth.PhoneNumberExistsBefore");
            public static readonly KeyString RoleAssignmentFailed = new("Auth.RoleAssignmentFailed");
        }

        public static class ClinicMessages
        {
            public static readonly KeyString InvalidLatitude = new("Clinics.InvalidLatitude");
            public static readonly KeyString InvalidLongitude = new("Clinics.InvalidLongitude");
            public static readonly KeyString InvalidRadius = new("Clinics.InvalidRadius");
            public static readonly KeyString ClinicNotFound = new("Clinics.NotFound");
            public static readonly KeyString EmailAlreadyExists = new("Clinics.EmailAlreadyExists");
            public static readonly KeyString PhoneAlreadyExists = new("Clinics.PhoneAlreadyExists");
            public static readonly KeyString EmailNotFound = new("Clinics.EmailNotFound");
            public static readonly KeyString PhoneNotFound = new("Clinics.PhoneNotFound");
        }

        public static class SpecializationMessages
        {
            public static readonly KeyString NotFound = new("Specializations.NotFound");
            public static readonly KeyString Created = new("Specializations.Created");
            public static readonly KeyString Updated = new("Specializations.Updated");
            public static readonly KeyString Deleted = new("Specializations.Deleted");
        }

        public static class AvailabilityMessages
        {
            public static readonly KeyString NotFound = new("Availability.NotFound");
            public static readonly KeyString Created = new("Availability.Created");
            public static readonly KeyString Updated = new("Availability.Updated");
            public static readonly KeyString Deleted = new("Availability.Deleted");
            public static readonly KeyString Restored = new("Availability.Restored");
        }

        public static class RatingMessages
        {
            public static readonly KeyString TargetRequired = new("Ratings.TargetRequired");
            public static readonly KeyString SingleTargetRequired = new("Ratings.SingleTargetRequired");
            public static readonly KeyString InvalidValue = new("Ratings.InvalidValue");
            public static readonly KeyString AlreadyRated = new("Ratings.AlreadyRated");
            public static readonly KeyString Created = new("Ratings.Created");
        }

        public static class RealTimeMessages
        {
            public static readonly KeyString SocketIdRequired = new("RealTime.SocketIdRequired");
            public static readonly KeyString ChannelNameRequired = new("RealTime.ChannelNameRequired");
            public static readonly KeyString MissingSocketInfo = new("RealTime.MissingSocketInfo");
            public static readonly KeyString NotConversationParticipant = new("RealTime.NotConversationParticipant");
            public static readonly KeyString ConversationIdRequired = new("RealTime.ConversationIdRequired");
            public static readonly KeyString ConversationNotFound = new("RealTime.ConversationNotFound");
            public static readonly KeyString ConnectionIdRequired = new("RealTime.ConnectionIdRequired");
        }

        public static class AttachmentMessages
        {
            public static readonly KeyString InvalidFormat = new("Attachments.InvalidFormat");
            public static readonly KeyString NoMediaProvided = new("Attachments.NoMediaProvided");
            public static readonly KeyString FileEmpty = new("Attachments.FileEmpty");
            public static readonly KeyString UploadFailed = new("Attachments.UploadFailed");
            public static readonly KeyString FileNotFound = new("Attachments.FileNotFound");
            public static readonly KeyString InvalidPlace = new("Attachments.InvalidPlace");
            public static readonly KeyString InvalidFileType = new("Attachments.InvalidFileType");
        }

        public static class PaymentMessages
        {
            public static readonly KeyString NotFound = new("Payments.NotFound");
            public static readonly KeyString AlreadyPaid = new("Payments.AlreadyPaid");
            public static readonly KeyString AppointmentNotFound = new("Payments.AppointmentNotFound");
            public static readonly KeyString AppointmentNotPending = new("Payments.AppointmentNotPending");
            public static readonly KeyString Unauthorized = new("Payments.Unauthorized");
            public static readonly KeyString PaymobOrderFailed = new("Payments.PaymobOrderFailed");
            public static readonly KeyString PaymobKeyFailed = new("Payments.PaymobKeyFailed");
            public static readonly KeyString WebhookHmacInvalid = new("Payments.WebhookHmacInvalid");
            public static readonly KeyString HmacRequired = new("Payments.HmacRequired");
            public static readonly KeyString TransactionRequired = new("Payments.TransactionRequired");
            public static readonly KeyString InvalidOrderId = new("Payments.InvalidOrderId");
            public static readonly KeyString InitiateSuccess = new("Payments.InitiateSuccess");
            public static readonly KeyString StatusFetched = new("Payments.StatusFetched");
            public static readonly KeyString PhoneNumberRequired = new("Payments.PhoneNumberRequired");
            public static readonly KeyString InvalidPhoneNumber = new("Payments.InvalidPhoneNumber");
            public static readonly KeyString PaymentFailed = new("Payments.PaymentFailed");
            public static readonly KeyString VerificationFailed = new("Payments.VerificationFailed");
            public static readonly KeyString AlreadyVerified = new("Payments.AlreadyVerified");
            public static readonly KeyString RefundFailed = new("Payments.RefundFailed");
            public static readonly KeyString RefundSuccess = new("Payments.RefundSuccess");
            public static readonly KeyString InvalidTransactionId = new("Payments.InvalidTransactionId");
            public static readonly KeyString AlreadyRefunded = new("Payments.AlreadyRefunded");
        }

        public static class DoctorMessages
        {
            public static readonly KeyString NotFound = new("Doctors.NotFound");
            public static readonly KeyString Created = new("Doctors.Created");
            public static readonly KeyString Updated = new("Doctors.Updated");
            public static readonly KeyString Deleted = new("Doctors.Deleted");
            public static readonly KeyString AlreadyExistsInClinic = new("Doctors.AlreadyExistsInClinic");
        }

        public static class BookingMessages
        {
            public static readonly KeyString SlotNotFound = new("Booking.SlotNotFound");
            public static readonly KeyString SlotUnavailable = new("Booking.SlotUnavailable");
            public static readonly KeyString ReservationExpired = new("Booking.ReservationExpired");
            public static readonly KeyString ReservationNotFound = new("Booking.ReservationNotFound");
            public static readonly KeyString PastDate = new("Booking.PastDate");
            public static readonly KeyString InvalidDate = new("Booking.InvalidDate");
            public static readonly KeyString BookingConfigNotFound = new("Booking.ConfigNotFound");
            public static readonly KeyString BookingConfigCreated = new("Booking.ConfigCreated");
            public static readonly KeyString BookingConfigUpdated = new("Booking.ConfigUpdated");
            public static readonly KeyString BookingConfigDeleted = new("Booking.ConfigDeleted");
            public static readonly KeyString BookingCreated = new("Booking.Created");
            public static readonly KeyString FeeNotConfigured = new("Booking.FeeNotConfigured");
        }

    }
}
