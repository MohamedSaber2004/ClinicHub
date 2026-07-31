using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Common.Services
{
    public sealed record PlanLimitResult(bool Allowed, int? Limit);

    public static class PlanLimitService
    {
        public static async Task<PlanLimitResult> CanAddDoctorAsync(IUnitOfWork unitOfWork, Guid clinicId, CancellationToken cancellationToken)
        {
            var plan = await GetActivePlanAsync(unitOfWork, clinicId, cancellationToken);
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
            Guid clinicId,
            CancellationToken cancellationToken)
        {
            var plan = await GetActivePlanAsync(unitOfWork, clinicId, cancellationToken);
            if (plan?.MaxStaff is not int max)
                return new PlanLimitResult(true, null);

            var staffUsers = await userManager.GetUsersInRoleAsync(nameof(UserType.Staff));
            var count = staffUsers.Count(u => u.ClinicId == clinicId && !u.IsDeleted);

            return new PlanLimitResult(count < max, max);
        }

        private static async Task<Plan?> GetActivePlanAsync(IUnitOfWork unitOfWork, Guid clinicId, CancellationToken cancellationToken)
        {
            var subscription = await unitOfWork.GetRepository<Subscription, Guid>()
                .GetAllWithIncluding(
                    s => s.ClinicId == clinicId && s.Status == SubscriptionStatus.Active,
                    s => s.Plan)
                .FirstOrDefaultAsync(cancellationToken);

            return subscription?.Plan;
        }
    }
}
