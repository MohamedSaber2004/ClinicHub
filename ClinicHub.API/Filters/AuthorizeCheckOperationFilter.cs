using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ClinicHub.API.Filters
{
    public class AuthorizeCheckOperationTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            if (context.Description.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
                return Task.CompletedTask;

            var actionAttrs = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true);
            var controllerAttrs = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(inherit: true);

            var hasAllowAnonymous = actionAttrs.OfType<AllowAnonymousAttribute>().Any()
                                    || controllerAttrs.OfType<AllowAnonymousAttribute>().Any();
            if (hasAllowAnonymous)
            {
                operation.Security = null;
                return Task.CompletedTask;
            }

            var hasAuthorize = actionAttrs.OfType<AuthorizeAttribute>().Any()
                              || controllerAttrs.OfType<AuthorizeAttribute>().Any()
                              || actionAttrs.OfType<RoleAuthorizeAttribute>().Any()
                              || controllerAttrs.OfType<RoleAuthorizeAttribute>().Any();

            if (!hasAuthorize)
            {
                operation.Security = null;
                return Task.CompletedTask;
            }

            operation.Security ??= [];

            var bearerRef = new OpenApiSecuritySchemeReference("Bearer");

            if (!operation.Security.Any(r => r.Keys.Any(k => k.Name == "Bearer")))
            {
                operation.Security.Add(new OpenApiSecurityRequirement { [bearerRef] = [] });
            }

            return Task.CompletedTask;
        }
    }
}
