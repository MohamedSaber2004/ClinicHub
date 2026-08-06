using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequirePlanPermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly SubscriptionPermission _permission;

        public RequirePlanPermissionAttribute(SubscriptionPermission permission)
        {
            _permission = permission;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

            if (!currentUserService.CurrentClinicId.HasValue)
            {
                context.Result = new ObjectResult(ApiResponse<object>.Error("Clinic not found.", 403))
                {
                    StatusCode = 403
                };
                return;
            }

            var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();

            var clinicId = currentUserService.CurrentClinicId.Value;

            var subscription = await unitOfWork.GetRepository<Subscription, Guid>()
                .GetAllAsync(s => s.ClinicId == clinicId
                    && s.Status == SubscriptionStatus.Active
                    && s.EndDate > DateTime.Now)
                .Include(s => s.Plan)
                    .ThenInclude(p => p!.Permissions)
                .FirstOrDefaultAsync();

            if (subscription?.Plan == null)
            {
                context.Result = new ObjectResult(ApiResponse<object>.Error("Active subscription required to access this feature.", 403))
                {
                    StatusCode = 403
                };
                return;
            }

            var hasPermission = subscription.Plan.Permissions
                .Any(p => p.Permission == _permission);

            if (!hasPermission)
            {
                context.Result = new ObjectResult(ApiResponse<object>.Error("Your current plan does not include this feature. Please upgrade to access it.", 403))
                {
                    StatusCode = 403
                };
            }
        }
    }
}
