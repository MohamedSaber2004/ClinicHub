using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class NotificationRepository : GenericRepository<Notification, Guid>, INotificationRepository
    {
        public NotificationRepository(ClinicHubContext context) : base(context)
        {
        }
    }
}
