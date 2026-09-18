using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Common
{
    /// <summary>
    /// Resolves the clinic scope for clinic-owner endpoints.
    /// The JWT ClinicId claim is minted at login and can be missing or stale
    /// (e.g. token issued before the owner's clinic existed and never refreshed
    /// after Setup). Fall back to the database instead of failing with
    /// "clinic not found": the user's linked clinic first, then a clinic they
    /// administer. Callers must already be authenticated + role-authorized, and
    /// resolution is always scoped to the caller's own user id.
    /// </summary>
    public static class ClinicScopeResolver
    {
        public static async Task<Guid?> ResolveClinicIdAsync(
            IUnitOfWork unitOfWork,
            Guid userId,
            Guid? currentClinicId,
            CancellationToken cancellationToken = default)
        {
            if (currentClinicId.HasValue)
                return currentClinicId.Value;

            if (userId == Guid.Empty)
                return null;

            var linkedClinicId = await unitOfWork.GetRepository<ApplicationUser, Guid>()
                .GetAllAsync(u => u.Id == userId && !u.IsDeleted)
                .Select(u => (Guid?)u.ClinicId)
                .FirstOrDefaultAsync(cancellationToken);

            if (linkedClinicId.HasValue)
                return linkedClinicId.Value;

            return await unitOfWork.ClinicRepository
                .GetAllAsync(c => c.ClinicAdminId == userId && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
