using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Questripag;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

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
        where TQueryFilter : class, new()
        where TQueryOrder : class, new()
    {
        private static readonly List<Type> _ordinalTypes = [typeof(int), typeof(long), typeof(BigInteger), typeof(float), typeof(double), typeof(DateTime), typeof(DateTimeOffset), typeof(DateOnly)];

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
                    .ToDictionary(o => o.Value.Substring(1), o => new Order(o.Precedence, o.Value.StartsWith('-')), StringComparer.OrdinalIgnoreCase)
                : [];

            TQueryOrder order = new();
            TQueryFilter filter = new TQueryFilter();

            void DeserializeFilter(string? parentPath, object obj, Type type)
            {
                foreach (var prop in type.GetProperties().Where(x => x.SetMethod != null))
                {
                    var propName = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name;
                    var path = parentPath == null ? propName : $"{parentPath}.{propName}";
                    var propType = prop.PropertyType;
                    if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Filter<>))
                    {
                        var valueType = propType.GetGenericArguments()[0];
                        var rawValues = queryString.Where(x => x.Key.Equals(path, StringComparison.OrdinalIgnoreCase)).SelectMany(x => x.Value);
                        if (!rawValues.Any()) continue;
                        Filter filter;
                        if (valueType == typeof(string))
                        {
                            filter = new Filter<string>
                            {
                                UntypedValues = [..rawValues]
                            };
                        }
                        else
                        {
                            rawValues = rawValues.SelectMany(x => x.Split('|')).ToList();
                            if (!_ordinalTypes.Contains(valueType) && rawValues.Any(x => x.Contains("..")))
                            {
                                throw new SerializationException($"Only ordinal types may form ranges. Encountered range of type {valueType}.");
                            }
                            var filterType = typeof(Filter<>).MakeGenericType(valueType);
                            filter = (Filter)Activator.CreateInstance(filterType)!;
                            foreach (var rawValue in rawValues)
                            {
                                var rawParts = rawValue.Split("..");
                                if (rawParts.Length > 2)
                                {
                                    throw new SerializationException($"Expected at most one '..', got {rawParts.Length - 1}.");
                                }
                                var parts = rawParts.Select(x => ParseRawValue(x, valueType)).ToList();
                                if (rawParts.Length == 1)
                                {
                                   filter.UntypedValues.Add(parts[0]);
                                }
                                else if (rawParts.Length == 2)
                                {
                                    filter.UntypedRanges.Add((parts[0], parts[1]));
                                }
                            }
                        }
                        prop.SetValue(obj, filter);
                    }
                    else
                    {
                        var nestedObject = Activator.CreateInstance(propType)!;
                        prop.SetValue(obj, nestedObject);
                        DeserializeFilter(path, nestedObject, propType);
                    }
                }
            }

            void DeserializeOrder(string? parentPath, object obj, Type type)
            {
                foreach (var prop in type.GetProperties().Where(x => x.SetMethod != null))
                {
                    var propName = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name;
                    var path = parentPath == null ? propName : $"{parentPath}.{propName}";
                    var propType = prop.PropertyType;
                    if (propType == typeof(Order))
                    {
                        prop.SetValue(obj, orderDict.GetValueOrDefault(path));
                    }
                    else
                    {
                        var nestedObject = Activator.CreateInstance(propType)!;
                        prop.SetValue(obj, nestedObject);
                        DeserializeOrder(path, nestedObject, propType);
                    }
                }
            }

            DeserializeFilter(null, filter, typeof(TQueryFilter));
            DeserializeOrder(null, order, typeof(TQueryOrder));
            return new Query<TQueryFilter, TQueryOrder>(page, pageSize, filter, order);
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
                else if (type == typeof(long))
                {
                    return long.Parse(rawValue);
                }
                else if (type == typeof(BigInteger))
                {
                    return BigInteger.Parse(rawValue);
                }
                else if (type == typeof(double))
                {
                    return double.Parse(rawValue);
                }
                else if (type == typeof(float))
                {
                    return float.Parse(rawValue);
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
                else if (type == typeof(Color))
                {
                    return ColorTranslator.FromHtml(rawValue);
                }
                else if (type == typeof(DateOnly))
                {
                    return DateOnly.Parse(rawValue);
                }
                else if (type == typeof(TimeOnly))
                {
                    return TimeOnly.Parse(rawValue);
                }
                else if (type == typeof(DateTime))
                {
                    return DateTime.Parse(rawValue);
                }
                else if (type == typeof(DateTimeOffset))
                {
                    return DateTimeOffset.Parse(rawValue);
                }
                else if (type == typeof(Guid))
                {
                    return Guid.Parse(rawValue);
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