using System.Reflection;

namespace Questripag;

public static class PropertyInfoExtensions
{
    public static bool IsFilterProp(this PropertyInfo prop)
        => !prop.GetCustomAttributes<NoFilterAttribute>().Any();
    public static bool IsOrderProp(this PropertyInfo prop)
        => !prop.GetCustomAttributes<NoOrderAttribute>().Any();
}
