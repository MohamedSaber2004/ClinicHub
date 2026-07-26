namespace ClinicHub.API.Routes
{
    public static class ApiRoutes
    {
        public const string Base = "api/v{version:apiVersion}";

        public static class Auth
        {
            public const string Signup = Base + "/auth/signup";
            public const string Login = Base + "/auth/login";
            public const string LoginWeb = Base + "/auth/login-web";
            public const string LoginWithFacebook = Base + "/auth/login-facebook";
            public const string LoginWithGoogle = Base + "/auth/login-google";
            public const string ValidateGoogleToken = Base + "/auth/google/validate-token";
            public const string CompleteFacebookRegistration = Base + "/auth/complete-facebook-registration";
            public const string RefreshToken = Base + "/auth/refresh-token";
            public const string ForgetPassword = Base + "/auth/forget-password";
            public const string VerifyResetToken = Base + "/auth/verify-reset-token";
            public const string ResetPassword = Base + "/auth/reset-password";
            public const string Profile = Base + "/auth/profile";
            public const string UpdateProfile = Base + "/auth/profile/update";
            public const string UpdateLanguage = Base + "/auth/language/update";
            public const string Logout = Base + "/auth/logout";
            public const string SearchUsers = Base + "/auth/users/search";
        }

        public static class Attachments
        {
            public const string UploadFile = Base + "/attachments/upload";
            public const string UploadMultipleAttachments = Base + "/attachments/upload-multiple-attachments";
            public const string UpdateFile = Base + "/attachments/update/{name}";
            public const string DownloadFile = Base + "/attachments/download";
        }

        public static class Posts
        {
            public const string GetAllPagginated = Base + "/posts/pagginated";
            public const string GetById = Base + "/posts/{id:guid}";
            public const string GetPostReactions = Base + "/posts/{id:guid}/reactions";
            public const string Create = Base + "/posts/create";
            public const string Update = Base + "/posts/update";
            public const string Delete = Base + "/posts/delete";
            public const string ToggleReaction = Base + "/posts/{id:guid}/reactions";
        }

        public static class Comments
        {
            public const string GetAllPagginated = Base + "/comments/pagginated";
            public const string GetCommentsByPost = Base + "/comments/post/{postId:guid}";
            public const string GetReplies = Base + "/comments/{id:guid}/replies";
            public const string GetCommentReactions = Base + "/comments/{id:guid}/reactions";
            public const string Create = Base + "/comments/create";
            public const string Update = Base + "/comments/update";
            public const string Delete = Base + "/comments/delete";
            public const string ToggleReaction = Base + "/comments/{id:guid}/reactions";
        }

        public static class Notifications
        {
            public const string GetAllPagginated = Base + "/notifications/pagginated";
            public const string GetCount = Base + "/notifications/count";
        }

        public static class Clinics
        {
            public const string Search = Base + "/clinics/search";
            public const string GetRoute = Base + "/clinics/route";
            public const string GetByIdForUser = Base + "/clinics/{id:guid}";
            public const string GetAll = Base + "/clinics";
        }

        public static class ClinicManagement
        {
            public const string BaseRoute = Base + "/admin/clinics";
            public const string Create = BaseRoute;
            public const string Setup = BaseRoute + "/setup";
            public const string GetById = BaseRoute + "/{id:guid}";
            public const string GetPaginated = BaseRoute + "/paginated";
            public const string Dashboard = BaseRoute + "/dashboard/stats";
            public const string Update = BaseRoute + "/{id:guid}";
            public const string Activate = BaseRoute + "/{id:guid}/activate";
            public const string Deactivate = BaseRoute + "/{id:guid}/deactivate";
            public const string GetBookings = BaseRoute + "/bookings";
            public const string AcceptBooking = BaseRoute + "/bookings/accept";
            public const string RejectBooking = BaseRoute + "/bookings/reject";
        }

        public static class Specializations
        {
            public const string GetAll = Base + "/specializations";
            public const string GetById = Base + "/specializations/{id:guid}";
            public const string Create = Base + "/specializations/create";
            public const string Update = Base + "/specializations/update";
            public const string Delete = Base + "/specializations/delete";
            public const string GetActive = Base + "/specializations/active";
        }

        public static class Conversations
        {
            public const string GetAll = Base + "/conversations";
            public const string GetById = Base + "/conversations/{id:guid}";
            public const string Create = Base + "/conversations/create";
            public const string Update = Base + "/conversations/{id:guid}/update";
            public const string UpdateSettings = Base + "/conversations/{id:guid}/settings";
            public const string SendMessage = Base + "/conversations/{conversationId:guid}/messages";
            public const string GetMessages = Base + "/conversations/{conversationId:guid}/messages";
            public const string DeleteMessage = Base + "/conversations/messages/{messageId:guid}";
            public const string Delete = Base + "/conversations/{id:guid}";
        }

        public static class RealTime
        {
            public const string Auth = Base + "/realtime/auth";
            public const string Webhook = Base + "/realtime/webhook";
            public const string Typing = Base + "/realtime/typing";
            public const string GetTypingUsers = Base + "/realtime/typing/{conversationId:guid}";
            public const string SetActiveConversation = Base + "/realtime/active-conversation";
            public const string OnlineUsers = Base + "/realtime/online-users";
            public const string Connect = Base + "/realtime/connect";
            public const string Disconnect = Base + "/realtime/disconnect";
        }

        public static class ChatActions
        {
            public const string Typing = Base + "/chat/typing";
        }

        public static class Appointments
        {
            public const string Create = Base + "/appointments";
            public const string GetAll = Base + "/appointments";
            public const string GetById = Base + "/appointments/{id:guid}";
            public const string Update = Base + "/appointments/{id:guid}";
            public const string Delete = Base + "/appointments/{id:guid}";
            public const string Cancel = Base + "/appointments/{id:guid}/cancel";
            public const string Accept = Base + "/appointments/{id:guid}/accept";
            public const string Reject = Base + "/appointments/{id:guid}/reject";
        }

        public static class Availability
        {
            public const string GetAllAvailability = Base + "/availability";
            public const string Create = Base + "/availability";
            public const string Update = Base + "/availability/{id:guid}";
            public const string Delete = Base + "/availability/{id:guid}";
        }

        public static class Slots
        {
            public const string GetByDoctor = Base + "/clinics/{clinicId:guid}/doctors/{doctorId:guid}/slots";
        }

        public static class Reservations
        {
            public const string Create = Base + "/reservations";
        }

        public static class BookingConfig
        {
            public const string BaseRoute = Base + "/clinics/{clinicId:guid}/booking-config";
            public const string GetByClinic = BaseRoute;
            public const string Create = BaseRoute;
            public const string Update = BaseRoute;
            public const string Delete = BaseRoute;
        }

        public static class Payments
        {
            public const string Initiate = Base + "/payments/initiate";
            public const string CreateBooking = Base + "/payments";
            public const string VerifyBooking = Base + "/payments/verify";
            public const string Webhook = Base + "/payments/webhook";
            public const string GetStatus = Base + "/payments/status/{appointmentId:guid}";
            public const string Result = Base + "/payments/result";
        }

        public static class Ratings
        {
            public const string Create = Base + "/ratings";
            public const string GetDoctorRatings = Base + "/doctors/{doctorId:guid}/ratings";
            public const string GetClinicRatings = Base + "/clinics/{clinicId:guid}/ratings";
        }

        public static class Doctors
        {
            public const string GetAllByClinic = Base + "/admin/clinics/{clinicId:guid}/doctors";
            public const string GetById = Base + "/doctors/{id:guid}";
            public const string Create = Base + "/admin/clinics/{clinicId:guid}/doctors";
            public const string Update = Base + "/doctors/{id:guid}";
            public const string Delete = Base + "/doctors/{id:guid}";
        }

        public static class Users
        {
            public const string GetAll = Base + "/users";
            public const string GetById = Base + "/users/{id:guid}";
            public const string Add = Base + "/users";
            public const string Edit = Base + "/users/{id:guid}";
            public const string Delete = Base + "/users/{id:guid}";
            public const string AssignRole = Base + "/users/{id:guid}/roles";
            public const string EditRole = Base + "/users/{id:guid}/roles";
            public const string ChangePassword = Base + "/users/change-password";
        }

        public static class AdminDashboard
        {
            public const string BaseRoute = Base + "/admin/dashboard";
            public const string Stats = BaseRoute + "/stats";
            public const string UrgentTickets = BaseRoute + "/urgent-tickets";
            public const string Users = BaseRoute + "/users";
            public const string ClinicLogs = BaseRoute + "/clinics/{clinicId:guid}/logs";
            public const string ClinicsLookup = BaseRoute + "/clinics";
        }

        public static class UserVerifications
        {
            public const string BaseRoute = Base + "/admin/users";
            public const string GetPending = BaseRoute + "/pending";
            public const string Approve = BaseRoute + "/{id:guid}/approve";
            public const string Reject = BaseRoute + "/{id:guid}/reject";
        }

        public static class DoctorDashboard
        {
            public const string BaseRoute = Base + "/doctors";
            public const string Stats = BaseRoute + "/dashboard/stats";
            public const string Appointments = BaseRoute + "/appointments";
            public const string AcceptAppointment = BaseRoute + "/appointments/{id:guid}/accept";
            public const string RejectAppointment = BaseRoute + "/appointments/{id:guid}/reject";
            public const string CompleteAppointment = BaseRoute + "/appointments/{id:guid}/complete";
            public const string Patients = BaseRoute + "/patients";
            public const string PatientHistory = BaseRoute + "/patients/{patientId:guid}/history";
        }

        public static class StaffDashboard
        {
            public const string BaseRoute = Base + "/staff";
            public const string Stats = BaseRoute + "/dashboard/stats";
            public const string Appointments = BaseRoute + "/appointments";
            public const string ApproveAppointment = BaseRoute + "/appointments/{id:guid}/approve";
            public const string RejectAppointment = BaseRoute + "/appointments/{id:guid}/reject";
            public const string CheckIn = BaseRoute + "/appointments/{id:guid}/check-in";
            public const string RegisterPatient = BaseRoute + "/patients/register";
            public const string DoctorSchedule = BaseRoute + "/doctors/{doctorId:guid}/schedule";
            public const string Queue = BaseRoute + "/queue";
        }

        public static class Subscriptions
        {
            public const string BaseRoute = Base + "/subscriptions";
            public const string Create = BaseRoute;
            public const string InitiatePayment = BaseRoute + "/initiate-payment";
            public const string MySubscription = BaseRoute + "/my";
            public const string CancelMySubscription = BaseRoute + "/my/cancel";
        }

        public static class Plans
        {
            public const string BaseRoute = Base + "/plans";
            public const string GetAllActive = BaseRoute;
            public const string AdminBaseRoute = Base + "/admin/plans";
            public const string GetAll = AdminBaseRoute;
            public const string Create = AdminBaseRoute;
            public const string Update = AdminBaseRoute + "/{id:guid}";
            public const string Delete = AdminBaseRoute + "/{id:guid}";
        }

        public static class ClinicRegister
        {
            public const string Register = Base + "/clinics/register";
        }

        public static class AdminClinics
        {
            public const string PendingClinics = Base + "/admin/dashboard/clinics/pending";
            public const string ApproveClinic = Base + "/admin/dashboard/clinics/{id:guid}/approve";
            public const string RejectClinic = Base + "/admin/dashboard/clinics/{id:guid}/reject";
        }

        public static class AdminDashboardExt
        {
            public const string Tickets = Base + "/admin/dashboard/tickets";
            public const string UpdateTicketStatus = Base + "/admin/dashboard/tickets/{id:guid}/status";
            public const string AllSubscriptions = Base + "/admin/dashboard/subscriptions";
            public const string RevokeSubscription = Base + "/admin/dashboard/subscriptions/{id:guid}/revoke";
            public const string Advertisements = Base + "/admin/dashboard/advertisements";
            public const string ApproveAdvertisement = Base + "/admin/dashboard/advertisements/{id:guid}/approve";
            public const string RejectAdvertisement = Base + "/admin/dashboard/advertisements/{id:guid}/reject";
            public const string DeleteAdvertisement = Base + "/admin/dashboard/advertisements/{id:guid}";
        }

        public static class Advertisements
        {
            public const string BaseRoute = Base + "/advertisements";
            public const string MyAdvertisements = BaseRoute + "/my";
            public const string Create = BaseRoute;
            public const string Update = BaseRoute + "/{id:guid}";
            public const string Delete = BaseRoute + "/{id:guid}";
        }

        public static class ClinicStaff
        {
            public const string BaseRoute = Base + "/admin/clinics/staff";
            public const string GetAll = BaseRoute;
            public const string GetById = BaseRoute + "/{id:guid}";
            public const string Create = BaseRoute;
            public const string Update = BaseRoute + "/{id:guid}";
            public const string Delete = BaseRoute + "/{id:guid}";
        }
    }
}
