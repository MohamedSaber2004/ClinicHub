using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class NotificationRepository : GenericRepository<Notification, Guid>, INotificationRepository
    {
        private readonly ClinicHubContext _context;

        public NotificationRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public Task<int> DeleteByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
            => _context.Set<Notification>()
                .IgnoreQueryFilters()
                .Where(n => ids.Contains(n.Id))
                .ExecuteDeleteAsync(cancellationToken);
    }
}
