using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;
using ClinicHub.Domain.Enums;

namespace ClinicHub.API.Transformers
{
    public class LanguageHeaderOperationTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            operation.Parameters ??= new List<IOpenApiParameter>();

            var supportedLanguages = Enum.GetNames<LanguageCode>()
                .Select(name => (JsonNode?)JsonValue.Create(name))
                .ToList();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Required = false,
                Description = "The language for the response (ar, en)",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Default = JsonValue.Create("ar"),
                    Enum = supportedLanguages
                }
            });

            return Task.CompletedTask;
        }
    }
}
