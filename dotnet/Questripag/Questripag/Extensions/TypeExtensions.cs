namespace Questripag;

public static class TypeExtensions
{
    private static Type? GetGenericDefinitionOrNull(Type type)
    {
        return type.IsGenericType ? type.GetGenericTypeDefinition() : null;
    }

    private static T? SingleNotNullOrNull<T>(IEnumerable<T?> enumerable)
        where T : class
    {
        T? first = null;
        foreach (var item in enumerable)
        {
            if (item is null) continue;
            if (first is not null) return null;
            first = item;
        }
        return first;
    }

    /// <summary>
    /// Recursively unwraps a nested Page argument.
    /// </summary>
    /// <param name="responseType">A type to unwrap.</param>
    /// <returns>The single page argument or null if there isn't exactly one.</returns>
    public static Type? UnwrapPageArgument(this Type responseType)
    {
        Type? UnwrapPage(Type type)
        {
            var genericDefinition = GetGenericDefinitionOrNull(type);
            if (genericDefinition == typeof(Page<>)) return type;
            if (genericDefinition != null)
            {
                return SingleNotNullOrNull(type.GenericTypeArguments.Select(UnwrapPage));
            }
            return null;
        }
        var pageType = UnwrapPage(responseType);
        return pageType?.GenericTypeArguments?[0];
    }

    public static Type? UnwrapQueryArgument(this Type responseType)
    {
        var genericDef = GetGenericDefinitionOrNull(responseType);
        if (genericDef == typeof(Query<>))
        {
            return responseType.GenericTypeArguments[0];
        }
        return null;
    }
}
