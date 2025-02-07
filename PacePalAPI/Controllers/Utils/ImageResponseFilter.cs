using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

public class ImageResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var producesAttributes = context.MethodInfo.GetCustomAttributes<ProducesAttribute>();
        foreach (var attr in producesAttributes)
        {
            if (attr.ContentTypes.Contains("image/jpeg"))
            {
                foreach (var response in operation.Responses)
                {
                    response.Value.Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["image/jpeg"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            }
                        }
                    };
                }
            }
        }
    }
}