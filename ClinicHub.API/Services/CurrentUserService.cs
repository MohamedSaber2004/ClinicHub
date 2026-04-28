using System.Security.Claims;
using ClinicHub.Application.Common.Interfaces;

namespace ClinicHub.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public Guid UserId { get; }

        public bool IsAuthenticated { get; }

        public string? IpAddress { get; }

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
        }
    }
}
