using Forum.Model.DB;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var parameters = context.ApiDescription.ActionDescriptor.Parameters;
        if (parameters.Any(p => IsFormFile(p.ParameterType)))
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["post"] = new OpenApiSchema
                                {
                                    Type = "object",
                                    Properties = context.SchemaRepository.Schemas[nameof(Post)].Properties
                                },
                                ["titleImageFile"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary"
                                }
                            },
                            Required = new HashSet<string> { "post" }
                        }
                    }
                }
            };
        }
    }

    private bool IsFormFile(Type type)
    {
        if (type == typeof(IFormFile))
        {
            return true;
        }
        var underlyingType = Nullable.GetUnderlyingType(type);
        return underlyingType != null && underlyingType == typeof(IFormFile);
    }
}