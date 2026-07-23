using System.Security.Claims;
using ClinicHub.Application.Common.Interfaces;

namespace ClinicHub.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public Guid UserId { get; }

        public bool IsAuthenticated { get; }

        public string? IpAddress { get; }

        public int? UserTypes { get; }

        public Guid? CurrentClinicId { get; }

        public bool HasActiveSubscription { get; }

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value is { } userIdString &&
                Guid.TryParse(userIdString, out var userId))
            {
                UserId = userId;
            }
            else
            {
                UserId = Guid.Empty;
            }

            IsAuthenticated = httpContext?.User?.Identity?.IsAuthenticated ?? false;
            IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();

            var userTypesClaim = httpContext?.User?.FindFirst("UserTypes")?.Value;
            if (int.TryParse(userTypesClaim, out var userTypes))
            {
                UserTypes = userTypes;
            }

            var clinicIdClaim = httpContext?.User?.FindFirst("ClinicId")?.Value;
            if (Guid.TryParse(clinicIdClaim, out var clinicId))
            {
                CurrentClinicId = clinicId;
            }

            var subClaim = httpContext?.User?.FindFirst("HasActiveSubscription")?.Value;
            HasActiveSubscription = bool.TryParse(subClaim, out var hasSub) && hasSub;
        }
    }
}
