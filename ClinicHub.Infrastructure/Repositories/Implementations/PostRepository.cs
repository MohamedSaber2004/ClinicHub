using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class PostRepository : GenericRepository<Post, Guid>, IPostRepository
    {
        private readonly ClinicHubContext _context;

        public PostRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Post?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        {
            return await GetFirstWithIncluding(p => p.Id == id, 
                p => p.Comments, 
                p => p.Reactions, 
                p => p.Media)
                .FirstOrDefaultAsync(ct);
        }
    }
}
