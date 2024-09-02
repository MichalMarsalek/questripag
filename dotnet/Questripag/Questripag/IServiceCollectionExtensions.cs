using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema.Generation.TypeMappers;
using NJsonSchema;
using NSwag.Generation.AspNetCore;

namespace Questripag;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddQueryer<TSource, TQuery>(this IServiceCollection services, QueryerOptions? options = null)
        => services.AddTransient<IQueryer>((_) => new Queryer(options));

    public static MvcOptions AddQueryBinder(this MvcOptions options, QueryBinderProvider? binderProvider = null)
    {
        options.ModelBinderProviders.Insert(0, binderProvider ?? new QueryBinderProvider());
        return options;
    }
    public static AspNetCoreOpenApiDocumentGeneratorSettings AddQueryMapping(this AspNetCoreOpenApiDocumentGeneratorSettings options)
    {
        //options.OperationProcessors.Add();

        options.SchemaSettings.TypeMappers = [
            new ObjectTypeMapper(typeof(Query<>), new JsonSchema
            {
                Type = JsonObjectType.Object,
                Properties =
                {
                {
                    "page", new JsonSchemaProperty
                    {
                        IsRequired = true,
                        Type = JsonObjectType.String
                    }
                },
                {
                    "order", new JsonSchemaProperty
                    {
                        IsRequired = true,
                        Type = JsonObjectType.String
                    }
                },
                {
                    "filter", new JsonSchemaProperty
                    {
                        IsRequired = true,
                        Type = JsonObjectType.String
                    }
                }
                }
            }),
        ];

        //options.SchemaSettings.SchemaProcessors.Add();

        return options;
    }
}
