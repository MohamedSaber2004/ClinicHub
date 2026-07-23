using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ClinicHub.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequireSubscriptionAttribute : Attribute, IAsyncAuthorizationFilter
    {
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

            var hasActive = await unitOfWork.GetRepository<Subscription, Guid>()
                .ExistsAsync(s => s.ClinicId == currentUserService.CurrentClinicId.Value
                    && s.Status == SubscriptionStatus.Active
                    && s.EndDate > DateTime.UtcNow);

            if (!hasActive)
            {
                context.Result = new ObjectResult(ApiResponse<object>.Error("Active subscription required to access this feature.", 403))
                {
                    StatusCode = 403
                };
            }
        }
    }
}
