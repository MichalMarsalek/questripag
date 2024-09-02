using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema.Generation.TypeMappers;
using NJsonSchema;
using NSwag.Generation.AspNetCore;
using NJsonSchema.Generation;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Reflection;
using NSwag;

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
        options.OperationProcessors.Add(new QueryOperationProcessor());
        return options;
    }
}

public class QueryOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var queryOptions = context.MethodInfo.GetParameters().Select(x => x.ParameterType.UnwrapQueryArgument()).FirstOrDefault(x => x != null);
        if (queryOptions != null)
        {
            var bodyParamIndex = context.OperationDescription.Operation.Parameters.Select((x, i) => (x, i)).Where(x => x.x.Kind == OpenApiParameterKind.Body).Select(x => x.i).FirstOrDefault(-1);
            if (bodyParamIndex >= 0) context.OperationDescription.Operation.Parameters.RemoveAt(bodyParamIndex);
            var stringSchema = context.SchemaResolver.HasSchema(typeof(string), false)
                ? context.SchemaResolver.GetSchema(typeof(string), false)
                : context.SchemaGenerator.Generate(typeof(string), context.SchemaResolver);
            var parametersToAdd = new List<OpenApiParameter>
            {
                new(){
                    Name = "page",
                    Kind = OpenApiParameterKind.Query,
                    Schema = stringSchema
                },
                new(){
                    Name = "order",
                    Kind = OpenApiParameterKind.Query,
                    Schema = stringSchema
                },
            };
            parametersToAdd.AddRange(queryOptions.GetProperties().Where(x => x.IsFilterProp()).Select(x => new OpenApiParameter
            {
                Name = x.SerializationName(),
                Kind = OpenApiParameterKind.Query,
                Schema = stringSchema
            }));
            foreach(var param in parametersToAdd )
            {
                context.OperationDescription.Operation.Parameters.Add(param);
            }
        }
        return true;
    }
}