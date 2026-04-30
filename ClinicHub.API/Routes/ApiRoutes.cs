namespace ClinicHub.API.Routes
{
    public static class ApiRoutes
    {
        public const string Base = "api/v{version:apiVersion}";

        public static class Auth
        {
            public const string Signup = Base + "/auth/signup";
            public const string Verify = Base + "/auth/verify";
            public const string Login = Base + "/auth/login";
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

        public static class Clinics
        {
            public const string Search = Base + "/clinics/search";
        }

        public static class Maps
        {
            public const string Route = Base + "/maps/route";
        }
    }
}
