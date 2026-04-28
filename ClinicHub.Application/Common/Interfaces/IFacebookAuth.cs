namespace ClinicHub.Application.Common.Interfaces
{
    public interface IFacebookAuth
    {
        Task<bool> IsValidFacebookTokenAsync(string accessToken, CancellationToken ct);
        Task<FacebookAuthInfo?> GetFacebookUserInfoAsync(string accessToken, CancellationToken cancellationToken);
    }

    public class FacebookAuthInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PictureUrl { get; set; } = string.Empty;
    }
}
