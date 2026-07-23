namespace ClinicHub.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid UserId { get; }
        bool IsAuthenticated { get; }
        string? IpAddress { get; }
        int? UserTypes { get; }
        Guid? CurrentClinicId { get; }
        bool HasActiveSubscription { get; }
    }
}
