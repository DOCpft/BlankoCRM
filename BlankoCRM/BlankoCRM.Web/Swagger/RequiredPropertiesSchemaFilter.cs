using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BlankoCRM.Web.Swagger;

public sealed class RequiredPropertiesSchemaFilter : ISchemaFilter
{
    private static readonly NullabilityInfoContext NullabilityContext = new();

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema openApiSchema || context.Type is null)
        {
            return;
        }

        if (openApiSchema.Properties is null)
        {
            return;
        }

        var properties = context.Type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetMethod is not null);

        foreach (var property in properties)
        {
            var declaredJsonName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
            var jsonName = openApiSchema.Properties.Keys.FirstOrDefault(key =>
                string.Equals(key, declaredJsonName, StringComparison.Ordinal) ||
                string.Equals(key, property.Name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, JsonNamingPolicy.CamelCase.ConvertName(property.Name), StringComparison.Ordinal));

            if (jsonName is null)
            {
                continue;
            }

            var isRequired = property.IsDefined(typeof(RequiredAttribute), inherit: true)
                || IsNonNullableReferenceType(property);

            if (isRequired)
            {
                openApiSchema.Required ??= new HashSet<string>();

                if (openApiSchema.Required.Contains(jsonName))
                {
                    continue;
                }

                openApiSchema.Required.Add(jsonName);
            }
        }
    }

    private static bool IsNonNullableReferenceType(PropertyInfo property)
    {
        if (property.PropertyType.IsValueType)
        {
            return Nullable.GetUnderlyingType(property.PropertyType) is null;
        }

        var nullability = NullabilityContext.Create(property);
        return nullability.ReadState == NullabilityState.NotNull;
    }
}
