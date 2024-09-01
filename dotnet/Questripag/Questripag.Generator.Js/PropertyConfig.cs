using System.Text.Json.Serialization;

namespace Questripag.Generator.Js;

public class PropertyConfig
{
    public bool Response { get; set; }
    public bool Filter { get; set; }
    public bool Order { get; set; }
    [JsonIgnore]
    public Type ClrType { get; set; }
    public string Type
    {
        get
        {
            if (ClrType == typeof(bool)) return PropertyType.Boolean.ToString();
            if (ClrType == typeof(int)) return PropertyType.Integer.ToString();
            if (ClrType == typeof(double)) return PropertyType.Double.ToString();
            if (ClrType == typeof(decimal)) return PropertyType.Decimal.ToString();
            if (ClrType == typeof(string)) return PropertyType.String.ToString();
            if (ClrType.IsEnum) return $"{PropertyType.Enumerable}/{ClrType.Name}";
            if (ClrType == typeof(Guid)) return PropertyType.Guid.ToString();
            if (ClrType == typeof(DateTime)) return PropertyType.DateTime.ToString();
            if (ClrType == typeof(DateOnly)) return PropertyType.DateOnly.ToString();
            if (ClrType == typeof(TimeOnly)) return PropertyType.TimeOnly.ToString();
            if (ClrType == typeof(DateTimeOffset)) return PropertyType.DateTimeOffset.ToString();
            return PropertyType.Other.ToString();
        }
    }
}
