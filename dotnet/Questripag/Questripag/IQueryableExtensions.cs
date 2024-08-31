namespace Questripag;

public static class IQueryableExtensions
{
    public static IQueryable<TSource> Filter<TSource, TQuery>(this IQueryable<TSource> source, Queryer queryer, IFiltering<TQuery> filtering)
        => queryer.Filter(source, filtering);

    public static IQueryable<TSource> Order<TSource, TQuery>(this IQueryable<TSource> source, Queryer queryer, IOrdering<TQuery> ordering)
        => queryer.Order(source, ordering);
    public static IQueryable<TSource> Page<TSource>(this IQueryable<TSource> source, IPaging paging)
        => source.Skip(paging.Skip).Take(paging.PageSize);
}
