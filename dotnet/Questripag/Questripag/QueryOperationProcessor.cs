using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Text.Json;

namespace Questripag;

public class QueryOperationProcessor(QueryOpenApiOptions? options = null) : IOperationProcessor
{
    public QueryOpenApiOptions Options { get; set; } = options ?? new();
    public bool Process(OperationProcessorContext context)
    {
        JsonSchema GetOrGenerateSchema(Type type)
        {
            return context.SchemaResolver.HasSchema(type, false)
                    ? context.SchemaResolver.GetSchema(type, false)
                    : context.SchemaGenerator.Generate(type, context.SchemaResolver);
        }
        var queryOptions = context.MethodInfo.GetParameters().Select(x => x.ParameterType.UnwrapQueryArgument()).FirstOrDefault(x => x != null);
        if (queryOptions != null)
        {
            var bodyParamIndex = context.OperationDescription.Operation.Parameters.Select((x, i) => (x, i)).Where(x => x.x.Kind == OpenApiParameterKind.Body).Select(x => x.i).FirstOrDefault(-1);
            if (bodyParamIndex >= 0) context.OperationDescription.Operation.Parameters.RemoveAt(bodyParamIndex);
            var parametersToAdd = new List<OpenApiParameter>
            {
                new(){
                    Name = "page",
                    Kind = OpenApiParameterKind.Query,
                    Schema = GetOrGenerateSchema(typeof(string)),
                    Default = "1",
                    Pattern = @"[1-9]\d*(@[1-9]\d*)?"
                },
                new(){
                    Name = "order",
                    Kind = OpenApiParameterKind.Query,
                    Schema = new JsonSchema
                    {
                        Type = JsonObjectType.Array,
                        Item = new JsonSchema
                        {
                            Type = JsonObjectType.String,
                        }
                    },
                    UniqueItems = true,
                    CollectionFormat = OpenApiParameterCollectionFormat.Multi,
                },
            };
            foreach(var prop in queryOptions.GetProperties().Where(x => x.IsOrderProp()))
            {
                parametersToAdd[1].Schema.Item!.Enumeration.Add(prop.SerializationName());
                parametersToAdd[1].Schema.Item!.Enumeration.Add("-" + prop.SerializationName());
            }
            parametersToAdd.AddRange(queryOptions.GetProperties().Where(x => x.IsFilterProp()).Select(prop => new OpenApiParameter
            {
                Name = prop.SerializationName(),
                Kind = OpenApiParameterKind.Query,
                Schema = GetOrGenerateSchema(typeof(List<>).MakeGenericType((Options.FilterValuesAsStrings ? typeof(string) : prop.PropertyType))),
                Explode = false,
                CollectionFormat = OpenApiParameterCollectionFormat.Pipes,
                Style = OpenApiParameterStyle.PipeDelimited
            }));
            foreach (var param in parametersToAdd)
            {
                context.OperationDescription.Operation.Parameters.Add(param);
            }
        }
        return true;
    }
}

public class QueryOpenApiOptions
{
    public bool FilterValuesAsStrings { get; set; } = false;
}
