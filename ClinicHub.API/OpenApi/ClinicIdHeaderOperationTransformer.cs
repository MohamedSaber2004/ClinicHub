using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace ClinicHub.API.Transformers
{
    public class ClinicIdHeaderOperationTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            operation.Parameters ??= new List<IOpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-ClinicId",
                In = ParameterLocation.Header,
                Required = false,
                Description = "The clinic ID to scope the request to a specific clinic (for multi-clinic owners)",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Format = "uuid",
                    Default = JsonValue.Create("00000000-0000-0000-0000-000000000000")
                }
            });

            return Task.CompletedTask;
        }
    }
}
