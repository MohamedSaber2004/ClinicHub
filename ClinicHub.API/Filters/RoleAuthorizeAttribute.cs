using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace ClinicHub.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles = Array.Empty<string>();

        // Accepts role names as strings (e.g. nameof(UserType.Admin))
        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var currentUserService = context.HttpContext.RequestServices.GetService(typeof(ICurrentUserService)) as ICurrentUserService;

            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous) return;

            var user = context.HttpContext.User;
            var localizer = context.HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Messages>>();

            if (user.Identity == null || !user.Identity.IsAuthenticated || currentUserService == null || !currentUserService.IsAuthenticated)
            {
                var message = localizer[LocalizationKeys.ExceptionMessages.Unauthorized.Value];
                var response = ApiResponse<object>.Error(new Dictionary<string, string[]>(), message, StatusCodes.Status401Unauthorized);
                context.Result = new UnauthorizedObjectResult(response);
                return;
            }

            if (_roles.Any())
            {
                // The JWT stores user roles as a bitmask integer in the "UserTypes" claim
                // (e.g. Admin=1, SubAdmin=2, Doctor=16 — see UserType enum [Flags])
                var userTypesClaim = user.FindFirst("UserTypes")?.Value;

                bool hasRequiredRole = false;

                if (int.TryParse(userTypesClaim, out var userTypesInt))
                {
                    foreach (var roleName in _roles)
                    {
                        if (Enum.TryParse<UserType>(roleName, ignoreCase: true, out var requiredType)   
                            && requiredType != UserType.None
                            && (userTypesInt & (int)requiredType) != 0)
                        {
                            hasRequiredRole = true;
                            break;
                        }
                    }
                }

                if (!hasRequiredRole)
                {
                    var message = localizer[LocalizationKeys.ExceptionMessages.Unauthorized.Value];
                    var response = ApiResponse<object>.Error(new Dictionary<string, string[]>(), message, StatusCodes.Status403Forbidden);
                    context.Result = new ObjectResult(response)
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }
        }
    }
}
