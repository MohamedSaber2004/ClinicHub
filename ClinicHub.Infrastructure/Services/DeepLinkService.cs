using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace ClinicHub.Infrastructure.Services
{
    public class DeepLinkService : IDeepLinkService
    {
        private readonly EmailSettings _settings;
        private readonly DeepLinkSettings _deepLinkSettings;

        public DeepLinkService(IOptions<EmailSettings> settings, IOptions<DeepLinkSettings> deepLinkSettings)
        {
            _settings = settings.Value;
            _deepLinkSettings = deepLinkSettings.Value;
        }

        public string GenerateClinicApprovalLink(Guid clinicId, Guid userId)
        {
            var token = GenerateToken($"clinic-approval:{clinicId}:{userId}");
            return $"{_settings.FrontendUrl.TrimEnd('/')}{DeepLinkRoutes.ClinicSetup}?clinicId={clinicId}&userId={userId}&token={token}";
        }

        public string GeneratePostLink(Guid postId)
        {
            return GenerateGoLink($"post/{postId}");
        }

        public string GenerateVerificationApprovedLink(string userId, string role, string status)
        {
            var token = GenerateToken($"{userId}:{status}");
            return $"{_settings.FrontendUrl.TrimEnd('/')}{DeepLinkRoutes.VerificationApproved}?userId={userId}&role={role}&status={status}&token={token}";
        }

        public string GenerateLink(string path)
        {
            return $"{_settings.FrontendUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        }

        public string GenerateGoLink(string path)
        {
            return $"{_deepLinkSettings.BaseUrl.TrimEnd('/')}/go/{path.TrimStart('/')}";
        }

        public bool VerifyToken(string data, string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            var expected = GenerateToken(data);
            var expectedBytes = Encoding.UTF8.GetBytes(expected);
            var tokenBytes = Encoding.UTF8.GetBytes(token);

            return expectedBytes.Length == tokenBytes.Length
                && CryptographicOperations.FixedTimeEquals(expectedBytes, tokenBytes);
        }

        public string GenerateToken(string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_settings.DeepLinkSecret);
            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexStringLower(hash);
        }
    }
}
