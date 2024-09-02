using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Questripag;

public static class PropertyInfoExtensions
{
    public static bool IsFilterProp(this PropertyInfo prop)
        => !prop.GetCustomAttributes<NoFilterAttribute>().Any();
    public static bool IsOrderProp(this PropertyInfo prop)
        => !prop.GetCustomAttributes<NoOrderAttribute>().Any();
    public static string SerializationName(this PropertyInfo prop)
    {
        var nameAttribute = prop.GetCustomAttributes<JsonPropertyNameAttribute>().FirstOrDefault();
        return nameAttribute?.Name ?? Uncapitalize(Regex.Replace(prop.Name, @"(?=\w)_(?=\w)", "."));
    }

    private static string Uncapitalize(this string value)
    {
        return value.Length < 1 ? value : value[0].ToString().ToLowerInvariant() + value[1..];
    }
}
