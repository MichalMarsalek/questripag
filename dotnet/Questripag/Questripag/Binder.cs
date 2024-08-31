using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Questripag
{
    public static class BinderConfiguration
    {
        public static Func<int> DefaultPage { get; set; } = () => 1;
        public static Func<int> DefaultPageSize { get; set; } = () => 10;
        public static Func<IEnumerable<OrderCoordinate>> DefaultOrder { get; set; } = () => [];
    }

    public class BinderProvider : IModelBinderProvider
    {
        private IModelBinder? GetBinder(Type modelType)
        {
            if (!modelType.IsGenericType || modelType.GetGenericTypeDefinition() != typeof(Query<>))
                return null;

            Type[] types = modelType.GetGenericArguments();
            Type o = typeof(Query<>).MakeGenericType(types);
            return (IModelBinder)Activator.CreateInstance(o)!;
        }

        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            return context?.Metadata?.ModelType == null ? null : GetBinder(context.Metadata.ModelType);
        }
    }

    public class Binder<TQueryModel> : IModelBinder
    {
        public Query<TQueryModel> QueryCollectionToQuery(IQueryCollection queryString)
            => RawQueryToQuery(QueryStringToRawQuery(queryString));

        private RawQuery QueryStringToRawQuery(IQueryCollection queryString)
        {
            var rawPage = queryString["page"].FirstOrDefault("")!;
            var pageMatch = new Regex(@"^(\d+)@(\d+)$").Match(rawPage);
            int page = pageMatch.Success ? int.Parse(pageMatch.Groups[1].Value) : BinderConfiguration.DefaultPage();
            int pageSize = pageMatch.Success ? int.Parse(pageMatch.Groups[2].Value) : BinderConfiguration.DefaultPageSize();
            var rawOrder = queryString["order"].FirstOrDefault("")!;
            var orderMatch = new Regex(@"^([\-\+\s][a-zA-Z]+(?:\.[a-zA-Z]+)*)*$").Match(rawOrder);
            var order = orderMatch.Success
                ? orderMatch.Groups[1].Captures.Select(x => x.Value).Select(o => new OrderCoordinate(o.Substring(1), o.StartsWith('-')))
                : BinderConfiguration.DefaultOrder();
            var filter = queryString.Where(x => x.Key != "page" && x.Key != "order" && x.Value.FirstOrDefault() != null)
                .Select(x => ParseRawFilterCoordinate(x.Key, x.Value.First()));
            return new(page, pageSize, filter, order);
        }

        private RawFilterCoordinate ParseRawFilterCoordinate(string key, string rawValue)
        {
            // TODO handle escapes of "|" & ".."
            return new(key, rawValue.Split("|").Select<string, FilterValue<string>>(x =>
            {
                var parts = x.Split("..");
                return parts.Length < 2 ? new ScalarFilterValue<string>(x) : new RangeFilterValue<string>(parts[0], parts[1]);
            }));
        }

        private Query<TQueryModel> RawQueryToQuery(RawQuery rawQuery)
        {   // TODO Respect attributes for ignoring filter/order on specific prop

            var type = typeof(TQueryModel);
            var props = type.GetProperties();
            return new Query<TQueryModel>(
                rawQuery.Page,
                rawQuery.PageSize,
                props.Select(prop =>
                {
                    var propType = prop.PropertyType;
                    var rawValues = rawQuery.Filters.FirstOrDefault(x => x.Key.Equals(prop.Name, StringComparison.InvariantCultureIgnoreCase))?.Value;
                    var values = new List<FilterValue<object>>();
                    if (rawValues != null) {
                        foreach (var rawValue in rawValues) {
                            try
                            {
                                if (rawValue is ScalarFilterValue<string> sfv)
                                {
                                    values.Add(new ScalarFilterValue<object>(ParseRawValue(sfv.Value, propType)));
                                }
                                else if (rawValue is RangeFilterValue<string> rfv)
                                {
                                    values.Add(new RangeFilterValue<object>(ParseRawValue(rfv.LowerBound, propType), ParseRawValue(rfv.LowerBound, propType)));
                                }
                            }
                            catch { }
                        }
                    }
                    return values.Count < 1 ? null : new FilterCoordinate<dynamic>(prop.Name, values);

                }).Where(x => x != null)!,
                rawQuery.Orders.Select(x => new OrderCoordinate(
                    props.FirstOrDefault(prop => x.Key.Equals(prop.Name, StringComparison.InvariantCultureIgnoreCase))?.Name,
                    x.IsDescending)
                ).Where(x => x.Key != null)
            );
        }

        private dynamic ParseRawValue(string rawValue, Type type)
        {
            // TODO Add more types
            // TODO Add support for custom types & custom parsing
            try
            {
                if (type == typeof(string))
                {
                    return rawValue;
                }
                if (type == typeof(Guid))
                {
                    return Guid.Parse(rawValue);
                }
                else if (type == typeof(bool))
                {
                    if (rawValue == "0" || rawValue == "false") return false;
                    if (rawValue == "1" || rawValue == "true") return true;
                    throw new SerializationException();
                }
                else if (type == typeof(int))
                {
                    return int.Parse(rawValue);
                }
                else if (type == typeof(double))
                {
                    return double.Parse(rawValue);
                }
                else if (type == typeof(DateTime))
                {
                    return DateTime.Parse(rawValue);
                }
                else if (type == typeof(DateTimeOffset))
                {
                    return DateTimeOffset.Parse(rawValue);
                }
                else if (type == typeof(DateOnly))
                {
                    return DateOnly.Parse(rawValue);
                }
                else if (type == typeof(TimeOnly))
                {
                    return TimeOnly.Parse(rawValue);
                }
                else if (type.IsEnum)
                {
                    return int.TryParse(rawValue, out int result) && type.IsEnumDefined(result) ? Enum.ToObject(type, result)
                        : type.IsEnumDefined(rawValue) ? Enum.Parse(type, rawValue) : throw new SerializationException();
                }
                throw new SerializationException($"Unable to parse {rawValue} as {type}.");
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
