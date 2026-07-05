using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class UserFbTokenRepository : GenericRepository<UserFbToken, Guid>, IUserFbTokenRepository
    {
        private readonly ClinicHubContext _context;

        public UserFbTokenRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<UserFbToken>> GetUserTokensAsync(Guid userId)
        {
            return await _context.UserFbTokens
                .Where(t => t.UserId == userId && t.IsActive && !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<UserFbToken>> GetUserTokensByPlatformAsync(Guid userId, DevicePlatform platform)
        {
            return await _context.UserFbTokens
                .Where(t => t.UserId == userId && t.DevicePlatform == platform && t.IsActive && !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<UserFbToken?> GetByTokenAsync(string token)
        {
            return await _context.UserFbTokens
                .FirstOrDefaultAsync(t => t.Token == token && !t.IsDeleted);
        }
    }
}
