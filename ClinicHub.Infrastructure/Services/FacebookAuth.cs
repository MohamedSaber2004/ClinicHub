using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ClinicHub.Infrastructure.Services
{
    public class FacebookAuth : IFacebookAuth
    {
        private readonly HttpClient _httpClient;
        private readonly FacebookAuthSettings _options;
        private readonly string _appAccessToken;

        public FacebookAuth(HttpClient httpClient, IOptions<FacebookAuthSettings> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _appAccessToken = $"{_options.AppId}|{_options.AppSecret}";
        }

        public async Task<bool> IsValidFacebookTokenAsync(string accessToken, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(accessToken))
                return false;

            var validationUrl =
                $"{_options.GraphApiBaseUrl}/debug_token" +
                $"?input_token={Uri.EscapeDataString(accessToken)}" +
                $"&access_token={Uri.EscapeDataString(_appAccessToken)}";

            try
            {
                var response = await _httpClient.GetAsync(validationUrl, ct);
                if (!response.IsSuccessStatusCode)
                    return false;

                await using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

                if (doc.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("is_valid", out var isValid))
                {
                    return isValid.GetBoolean();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public async Task<FacebookAuthInfo?> GetFacebookUserInfoAsync(string accessToken, CancellationToken cancellationToken)
        {
            var userInfoUrl = $"{_options.GraphApiBaseUrl}/me?fields=id,email,first_name,last_name,picture&access_token={Uri.EscapeDataString(accessToken)}";

            try
            {
                var response = await _httpClient.GetAsync(userInfoUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return null;

                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
                var root = doc.RootElement;

                return new FacebookAuthInfo
                {
                    Id = root.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
                    Email = root.TryGetProperty("email", out var email) ? email.GetString() ?? string.Empty : string.Empty,
                    FirstName = root.TryGetProperty("first_name", out var firstName) ? firstName.GetString() ?? string.Empty : string.Empty,
                    LastName = root.TryGetProperty("last_name", out var lastName) ? lastName.GetString() ?? string.Empty : string.Empty,
                    PictureUrl = root.TryGetProperty("picture", out var picture) &&
                                 picture.TryGetProperty("data", out var data) &&
                                 data.TryGetProperty("url", out var url) ? url.GetString() ?? string.Empty : string.Empty
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
