using System.Linq.Expressions;

namespace Questripag;

public static class IQueryableExtensions
{
    public static IQueryable<TSource> Where<TSource, TProperty>(this IQueryable<TSource> source, Expression<Func<TSource, TProperty>> selector, FilterValue<TProperty> value)
        => source.Where(GetPredicate(selector, value));

    public static IQueryable<TSource> Filter<TSource>(this IQueryable<TSource> source, object filter)
        => throw new NotImplementedException();

    public static IQueryable<TSource> Order<TSource>(this IQueryable<TSource> source, object order)
        => throw new NotImplementedException();

    public static IQueryable<TSource> Filter<TSource, TFilter, TOrder>(this IQueryable<TSource> source, Query<TFilter, TOrder> query)
        => source.Filter(query.Filter);

    public static IQueryable<TSource> Order<TSource, TFilter, TOrder>(this IQueryable<TSource> source, Query<TFilter, TOrder> query)
        => source.Order(query.Order);

    public static DeferredOrderedQueryable<TSource> OrderBy<TSource, TProperty>(IQueryable<TSource> source, Expression<Func<TSource, TProperty>> selector, OrderCoordinate order)
        => DeferredOrderedQueryable<TSource>.FromIQueryable(source).OrderBy(selector, order);

    public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, IPaging paging)
        => source.Skip(paging.Skip).Take(paging.PageSize);

    private static Expression<Func<TSource, bool>> GetPredicate<TSource, TProperty>(Expression<Func<TSource, TProperty>> selector, FilterValue<TProperty> value)
    {
        throw new NotImplementedException();
    }
}
