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
                                    ["post.postId"] = new OpenApiSchema { Type = "integer", Format = "int32" },
                                    ["post.userAuthorId"] = new OpenApiSchema { Type = "integer", Format = "int32" },
                                    ["post.postTitle"] = new OpenApiSchema { Type = "string" },
                                    ["post.postBody"] = new OpenApiSchema { Type = "string" },
                                    ["post.postSubtitle"] = new OpenApiSchema { Type = "string" },
                                    ["titleImageFile"] = new OpenApiSchema { Type = "string", Format = "binary" }
                                },
                                Required = new HashSet<string>
                            {
                                "post.postId",
                                "post.userAuthorId",
                                "post.postTitle",
                                "post.postBody",
                                "post.postSubtitle"
                            }
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

        /*
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var parameters = context.ApiDescription.ActionDescriptor.Parameters
                .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFile))
                .ToList();

            if (parameters.Any())
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
                                Properties = parameters.ToDictionary(
                                    p => p.Name,
                                    p => new OpenApiSchema { Type = "string", Format = "binary" }
                                )
                            }
                        }
                    }
                };
            }
        }
        */
    }