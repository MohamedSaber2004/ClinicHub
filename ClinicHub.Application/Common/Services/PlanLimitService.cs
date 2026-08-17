using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Common.Services
{
    public sealed record PlanLimitResult(bool Allowed, int? Limit, bool HasActiveSubscription = true);

    public static class PlanLimitService
    {
        public static async Task<PlanLimitResult> CanAddDoctorAsync(IUnitOfWork unitOfWork, Guid clinicId, CancellationToken cancellationToken)
        {
            var (plan, hasActiveSubscription) = await GetActivePlanAsync(unitOfWork, clinicId, cancellationToken);

            if (!hasActiveSubscription)
                return new PlanLimitResult(false, 0, false);

            if (plan?.MaxDoctors is not int max)
                return new PlanLimitResult(true, null);

            var count = await unitOfWork.DoctorRepository
                .GetAllAsync(d => d.ClinicId == clinicId && !d.IsDeleted)
                .CountAsync(cancellationToken);

            return new PlanLimitResult(count < max, max);
        }

        public static async Task<PlanLimitResult> CanAddStaffAsync(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IClinicHubContext context,
            Guid clinicId,
            CancellationToken cancellationToken)
        {
            var (plan, hasActiveSubscription) = await GetActivePlanAsync(unitOfWork, clinicId, cancellationToken);

            if (!hasActiveSubscription)
                return new PlanLimitResult(false, 0, false);

            if (plan?.MaxStaff is not int max)
                return new PlanLimitResult(true, null);

            var staffRoleId = await context.Roles
                .Where(r => r.Name == nameof(UserType.Staff))
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (staffRoleId == Guid.Empty)
                return new PlanLimitResult(true, max);

            var count = await (
                from u in userManager.Users
                join ur in context.UserRoles on u.Id equals ur.UserId
                where u.ClinicId == clinicId && !u.IsDeleted && ur.RoleId == staffRoleId
                select u
            ).CountAsync(cancellationToken);

            return new PlanLimitResult(count < max, max);
        }

        private static async Task<(Plan? Plan, bool HasActiveSubscription)> GetActivePlanAsync(
            IUnitOfWork unitOfWork,
            Guid clinicId,
            CancellationToken cancellationToken)
        {
            var subscription = await unitOfWork.GetRepository<Subscription, Guid>()
                .GetAllWithIncluding(
                    s => s.ClinicId == clinicId
                        && s.Status == SubscriptionStatus.Active
                        && s.EndDate > DateTime.Now,
                    s => s.Plan)
                .FirstOrDefaultAsync(cancellationToken);

            return subscription == null
                ? (null, false)
                : (subscription.Plan, true);
        }
    }
}
