using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NSwag.Generation.AspNetCore;

namespace Questripag;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddQueryer<TSource, TQuery>(this IServiceCollection services, QueryerOptions? options = null)
        => services.AddTransient<IQueryer>((_) => new Queryer(options));

    public static MvcOptions AddQueryBinder(this MvcOptions options, QueryBinderProvider? binderProvider = null)
    {
        options.ModelBinderProviders.Insert(0, binderProvider ?? new QueryBinderProvider());
        return options;
    }
    public static AspNetCoreOpenApiDocumentGeneratorSettings AddQueryMapping(this AspNetCoreOpenApiDocumentGeneratorSettings x, QueryOpenApiOptions? options = null)
    {        
        x.OperationProcessors.Add(new QueryOperationProcessor(options));
        return x;
    }
}