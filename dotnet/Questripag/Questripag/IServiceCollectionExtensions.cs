using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Questripag;

public static class IServiceCollectionExtensions
{
    public static void AddQueryBinder(this MvcOptions options, QueryBinderProvider? binderProvider = null)
        => options.ModelBinderProviders.Insert(0, binderProvider ?? new QueryBinderProvider());
}
