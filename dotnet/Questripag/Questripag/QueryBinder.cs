using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Questripag;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Questripag
{
    public class QueryBinderProvider : IModelBinderProvider
    {
        public Func<int> DefaultPage { get; set; } = () => 1;
        public Func<int> DefaultPageSize { get; set; } = () => 10;
        public JsonSerializerOptions JsonSerializerOptions { get; set; } = new();

        private IModelBinder? GetBinder(Type modelType)
        {
            if (!modelType.IsGenericType || modelType.GetGenericTypeDefinition() != typeof(Query<,>))
                return null;

            Type[] types = modelType.GetGenericArguments();
            Type o = typeof(Query<,>).MakeGenericType(types);
            return (IModelBinder)Activator.CreateInstance(o, this)!;
        }

        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            return context?.Metadata?.ModelType == null ? null : GetBinder(context.Metadata.ModelType);
        }
    }

    public class QueryBinder<TQueryFilter, TQueryOrder>(QueryBinderProvider binderProvider) : IModelBinder
    {
        private readonly QueryBinderProvider _binderProvider = binderProvider;
        internal Query<TQueryFilter, TQueryOrder> QueryCollectionToQuery(IQueryCollection queryString)
        {
            var rawPage = queryString["page"].FirstOrDefault("")!;
            int page;
            int pageSize;
            var pageMatch = new Regex(@"^(\d+)$").Match(rawPage);
            if (pageMatch.Success)
            {
                page = int.Parse(pageMatch.Groups[1].Value);
                pageSize = _binderProvider.DefaultPageSize();
            }
            else
            {
                var pageOptionsMatch = new Regex(@"^(\d+)@(\d+)$").Match(rawPage);
                page = pageOptionsMatch.Success ? int.Parse(pageOptionsMatch.Groups[1].Value) : _binderProvider.DefaultPage();
                pageSize = pageOptionsMatch.Success ? int.Parse(pageOptionsMatch.Groups[2].Value) : _binderProvider.DefaultPageSize();
            }

            var rawOrder = string.Join("", queryString["order"].Where(x => x != "").Select(x => x.StartsWith("+") || x.StartsWith("-") ? x : "+" + x));
            var orderMatch = new Regex(@"^([\-\+\s][a-zA-Z]+(?:\.[a-zA-Z]+)*)*$").Match(rawOrder);
            var orderDict = orderMatch.Success
                ? orderMatch.Groups[1].Captures.Select((x, i) => new { x.Value, Precedence = i })
                    .ToDictionary(o => o.Value.Substring(1), o => new Order(o.Precedence, o.Value.StartsWith('-')), StringComparer.InvariantCultureIgnoreCase)
                : new Dictionary<string, Order>();

            var order = JsonSerializer.Deserialize<TQueryOrder>(JsonSerializer.Serialize());

            var filterDict = queryString.Where(x => x.Key != "page" && x.Key != "order" && x.Value.FirstOrDefault() != null)
                .SelectMany(x => x.Value.Select(v => new KeyValuePair<string, string>(x.Key, v)))
                .ToDictionary(x => x.Key, x => x.Value);


            return new(page, pageSize, filter, order);
        }

        private RawFilterCoordinate ParseRawFilterCoordinate(string key, string rawValue)
        {
            // TODO handle escapes of "|" & ".."
            return new(key, rawValue.Split("|").Select<string, Filter<string>>(x =>
            {
                var parts = x.Split("..");
                return parts.Length < 2 ? new ScalarFilterValue<string>(x) : new RangeFilterValue<string>(parts[0], parts[1]);
            }));
        }

        private dynamic ParseRawValue(string rawValue, Type type)
        {
            try
            {
                if (type == typeof(string))
                {
                    return rawValue;
                }
                else if (type == typeof(int))
                {
                    return int.Parse(rawValue);
                }
                else if (type == typeof(double))
                {
                    return double.Parse(rawValue);
                }
                else if (type == typeof(bool))
                {
                    if (rawValue == "0" || rawValue.Equals("false", StringComparison.InvariantCultureIgnoreCase)) return false;
                    if (rawValue == "1" || rawValue.Equals("true", StringComparison.InvariantCultureIgnoreCase)) return true;
                }
                else if (type.IsEnum)
                {
                    return int.TryParse(rawValue, out int result) && type.IsEnumDefined(result) ? Enum.ToObject(type, result)
                        : type.IsEnumDefined(rawValue) ? Enum.Parse(type, rawValue) : throw new SerializationException();
                }
                return JsonSerializer.Deserialize(JsonSerializer.Serialize(rawValue), type, _binderProvider.JsonSerializerOptions)!;
            }
            catch(Exception ex)
            {
                throw new SerializationException($"Unable to parse {rawValue} as {type}.", ex);
            }
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            var queryString = bindingContext.HttpContext.Request.Query;
            try
            {
                var result = QueryCollectionToQuery(queryString);
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }
            catch (SerializationException ex)
            {
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, ex.Message);
                return Task.CompletedTask;
            }
        }
    }
}


public class BinderOptions
{
    public StringFilterOperation StringOperation { get; set; } = StringFilterOperation.StartsWith;
    public DateTimeFilterPrecision DateTimePrecision { get; set; } = DateTimeFilterPrecision.Days;
}