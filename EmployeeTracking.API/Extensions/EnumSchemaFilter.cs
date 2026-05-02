using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EmployeeTracking.API.Extensions
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (!context.Type.IsEnum) return;

            schema.Enum.Clear();
            schema.Type = "integer";
            schema.Description = BuildEnumDescription(context.Type);
            schema.Extensions.Add(
                "x-enumNames",
                new OpenApiArray
                {
                new OpenApiArray()
                });

            foreach (var name in Enum.GetNames(context.Type))
            {
                var value = (int)Enum.Parse(context.Type, name);
                schema.Description += $"\n- `{value}` = {name}";
            }
        }

        private static string BuildEnumDescription(Type enumType)
            => $"**{enumType.Name}** possible values:";
    }
}
