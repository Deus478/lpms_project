using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Filters;

namespace DocumentManagement.Filters
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the action has file upload parameters
            var fileParams = context.ApiDescription.ParameterDescriptions
                .Where(p => p.ModelMetadata?.ContainerType == typeof(IFormFile) ||
                           p.Type == typeof(IFormFile))
                .ToList();

            if (!fileParams.Any())
                return;

            // Clear existing parameters
            operation.Parameters?.Clear();

            // Set up multipart/form-data request body
            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["file"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                },
                                ["title"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Nullable = true
                                },
                                ["description"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Nullable = true
                                },
                                ["category"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Nullable = true
                                }
                            },
                            Required = new HashSet<string> { "file" }
                        }
                    }
                }
            };
        }
    }
}