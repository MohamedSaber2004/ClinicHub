using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories
{
    public interface IMessageMediaRepository : IGenericRepository<MessageMedia, Guid>
    {
        Task<List<MessageMedia>> GetMessageMediaAsync(Guid messageId);
    }
}
