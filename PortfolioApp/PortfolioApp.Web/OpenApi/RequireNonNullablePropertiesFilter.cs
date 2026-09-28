using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PortfolioApp.Web.OpenApi;

/// <summary>
/// Marks every non-nullable property as required, so the generated TypeScript types
/// do not turn each field into an optional one.
/// </summary>
internal sealed class RequireNonNullablePropertiesFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        foreach (var (name, property) in schema.Properties)
        {
            if (!property.Nullable)
                schema.Required.Add(name);
        }
    }
}
