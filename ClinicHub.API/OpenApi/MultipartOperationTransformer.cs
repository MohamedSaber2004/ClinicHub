using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ClinicHub.API.Transformers
{
    public class MultipartOperationTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            if (operation.RequestBody?.Content == null) return Task.CompletedTask;

            foreach (var content in operation.RequestBody.Content)
            {
                if (content.Key.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase))
                {
                    var schema = content.Value.Schema;
                    if (schema != null && schema.Properties != null)
                    {
                        content.Value.Encoding ??= new Dictionary<string, OpenApiEncoding>();

                        var propertyKeys = schema.Properties.Keys.ToList();

                        foreach (var key in propertyKeys)
                        {
                            var originalProperty = schema.Properties[key];

                            // Handle any property that contains "Images", "Videos", "Audios", "Documents", or "Files"
                            if (key.Contains("Images", StringComparison.OrdinalIgnoreCase) || 
                                key.Contains("Videos", StringComparison.OrdinalIgnoreCase) || 
                                key.Contains("Audios", StringComparison.OrdinalIgnoreCase) || 
                                key.Contains("Documents", StringComparison.OrdinalIgnoreCase) || 
                                key.Contains("Files", StringComparison.OrdinalIgnoreCase) || 
                                key.Equals("File", StringComparison.OrdinalIgnoreCase))
                            {
                                // If it's not a Place property, treat it as a file/array of files
                                if (!key.Contains("Place", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (originalProperty.Type == JsonSchemaType.Array)
                                    {
                                        schema.Properties[key] = new OpenApiSchema
                                        {
                                            Type = JsonSchemaType.Array,
                                            Items = new OpenApiSchema
                                            {
                                                Type = JsonSchemaType.String,
                                                Format = "binary"
                                            },
                                            Description = $"List of {key} to upload"
                                        };
                                    }
                                    else
                                    {
                                        schema.Properties[key] = new OpenApiSchema
                                        {
                                            Type = JsonSchemaType.String,
                                            Format = "binary",
                                            Description = $"The {key} file to upload"
                                        };
                                    }

                                    content.Value.Encoding[key] = new OpenApiEncoding
                                    {
                                        Style = ParameterStyle.Form,
                                        Explode = true,
                                        ContentType = "application/octet-stream"
                                    };
                                }
                            }
                            
                            // Handle any property that contains "Place"
                            if (key.Contains("Place", StringComparison.OrdinalIgnoreCase))
                            {
                                schema.Properties[key] = new OpenApiSchema
                                {
                                    Type = JsonSchemaType.String,
                                    Description = $"The location ID for {key.Replace("Place", "")} (0-12)"
                                };

                                content.Value.Encoding[key] = new OpenApiEncoding
                                {
                                    ContentType = "text/plain",
                                    Explode = false
                                };
                            }
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
