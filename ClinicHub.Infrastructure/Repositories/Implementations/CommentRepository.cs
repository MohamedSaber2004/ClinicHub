using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class CommentRepository : GenericRepository<Comment, Guid>, ICommentRepository
    {
        private readonly ClinicHubContext _context;

        public CommentRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Comment?> GetByIdWithRepliesAsync(Guid id, CancellationToken ct = default)
        {
            return await GetFirstWithIncluding(c => c.Id == id,
                c => c.Replies,
                c => c.Reactions,
                c => c.Media)
                .FirstOrDefaultAsync(ct);
        }
    }
}
