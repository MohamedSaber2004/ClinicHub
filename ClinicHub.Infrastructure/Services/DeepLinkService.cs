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

        public DeepLinkService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public string GenerateClinicApprovalLink(Guid clinicId, Guid userId)
        {
            var token = GenerateToken($"clinic-approval:{clinicId}:{userId}");
            return $"{_settings.FrontendUrl.TrimEnd('/')}/clinic/setup?clinicId={clinicId}&userId={userId}&token={token}";
        }

        public string GeneratePostLink(Guid postId)
        {
            return $"{_settings.FrontendUrl.TrimEnd('/')}/post/{postId}";
        }

        private string GenerateToken(string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_settings.DeepLinkSecret);
            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexStringLower(hash);
        }
    }
}
