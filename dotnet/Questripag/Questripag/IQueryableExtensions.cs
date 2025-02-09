using System.Linq.Expressions;

namespace Questripag;

public static class IQueryableExtensions
{
    private static IQueryable<TSource> WhereScalar<TSource, TProperty>(this IQueryable<TSource> source, Expression<Func<TSource, TProperty>> selector, Filter<TProperty> value)
        => source.Where(GetPredicate(selector, value));
    private static IQueryable<TSource> WhereVector<TSource, TProperty>(this IQueryable<TSource> source, Expression<Func<TSource, IEnumerable<TProperty>>> selector, Filter<TProperty> value)
        => source.Where(GetPredicate(selector, value));

    public static IQueryable<TSource> Where<TSource, TProperty>(this IQueryable<TSource> source, Expression<Func<TSource, TProperty>> selector, Filter<TProperty> value)
        => source.Where(GetPredicate(selector, value));
    public static IQueryable<TSource> Where<TSource, TProperty>(this IQueryable<TSource> source, Expression<Func<TSource, IEnumerable<TProperty>>> selector, Filter<TProperty> value)
        => source.Where(GetPredicate(selector, value));

    public static IQueryable<TSource> Where<TSource, TFilter, TOrder>(this IQueryable<TSource> source, Query<TFilter, TOrder> query)
    {
        var x = Expression.Parameter(typeof(TSource), "x");
        void ApplyWheres(Expression path, object? filter)
        {
            if (filter is null) return;
            var type = filter.GetType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Filter<>))
            {
                var propType = type.GetGenericArguments()[0];
                var whereMethod = typeof(IQueryableExtensions).GetMethod(nameof(WhereScalar))!;
                source = source.Where(GetPredicate<TSource>(propType, Expression.Lambda(path, x), (filter as Filter)!));
            }
            else
            {
                foreach (var prop in type.GetProperties())
                {
                    ApplyWheres(Expression.Property(path, prop), prop.GetValue(filter));
                }
            }
        }
        ApplyWheres(x, query.Filter);
        return source;
    }

    public static IQueryable<TSource> OrderBy<TSource, TFilter, TOrder>(this IQueryable<TSource> source, Query<TFilter, TOrder> query)
        => throw new NotImplementedException();

    public static DeferredOrderedQueryable<TSource> OrderBy<TSource, TProperty>(IQueryable<TSource> source, Expression<Func<TSource, TProperty>> selector, Order order)
        => DeferredOrderedQueryable<TSource>.FromIQueryable(source).OrderBy(selector, order);

    public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, IPaging paging)
        => source.Skip(paging.Skip).Take(paging.PageSize);

    private static Expression<Func<TSource, bool>> GetPredicate<TSource, TProperty>(Expression<Func<TSource, TProperty>> selector, Filter<TProperty> filter)
        => GetPredicate<TSource>(typeof(TProperty), selector, filter);

    private static Expression<Func<TSource, bool>> GetPredicate<TSource>(Type propType, LambdaExpression selector, Filter filter)
    {
        var y = Expression.Parameter(propType, "y");
        Expression<Func<TSource, bool>> result = _ => true;
        foreach (var value in filter.UntypedValues)
        { 
            if (propType == typeof(string))
            {
                var stringValue = value as string;
                Expression<Func<string, bool>> valuePredicate;
                if (filter.StringOperation is StringFilterOperation.Equals) valuePredicate = x => x.Equals(stringValue);
                if (filter.StringOperation is StringFilterOperation.StartsWith) valuePredicate = x => x.StartsWith(stringValue);
                if (filter.StringOperation is StringFilterOperation.Contains) valuePredicate = x => x.Contains(stringValue);
                else throw new Exception("Unsupported StringOperation");
                result = result.Or(valuePredicate.ComposeByInlining<TSource, bool>(selector));
            } 
            else
            {
                result = result.Or(Expression.Lambda(Expression.Equal(y, Expression.Constant(value)), y).ComposeByInlining<TSource, bool>(selector));
            }
        }
        foreach (var range in filter.UntypedRanges)
        {
            var lowerCheck = range.Item1 is null ? null : Expression.Lambda(Expression.LessThanOrEqual(Expression.Constant(range.Item1), y));
            var upperCheck = range.Item2 is null ? null : Expression.Lambda(Expression.LessThanOrEqual(y, Expression.Constant(range.Item2)));
            if (lowerCheck is null && upperCheck is null) continue;
            var check = (lowerCheck is null ? upperCheck : upperCheck is null ? lowerCheck : lowerCheck.And(upperCheck))!;
            result = result.Or(check.ComposeByInlining<TSource, bool>(selector));
        }

        return result;
    }

    private static Expression<Func<TSource, bool>> GetPredicate<TSource, TProperty>(Expression<Func<TSource, IEnumerable<TProperty>>> selector, Filter<TProperty> value)
    {
        throw new NotImplementedException();
    }
}
