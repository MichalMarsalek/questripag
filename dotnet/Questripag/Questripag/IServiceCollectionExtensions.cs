using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Questripag;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddQueryer<TSource, TQuery>(this IServiceCollection services, QueryerOptions? options = null)
        => services.AddTransient<IQueryer>((_) => new Queryer(options));

    public static void AddQueryBinder(this MvcOptions options, QueryBinderProvider? binderProvider = null)
        => options.ModelBinderProviders.Insert(0, binderProvider ?? new QueryBinderProvider());
}
