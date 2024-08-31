using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Questripag.EFCore
{
    public static class Extensions
    {
        public async static Task<Page<TSource>> ToPageAsync<TSource>(this IQueryable<TSource> source, IPaging paging, CancellationToken cancellationToken)
        {
            var items = await source.Page(paging).ToListAsync(cancellationToken);
            var totalItemsCount = items.Count < paging.PageSize ? paging.Skip + items.Count : await source.CountAsync(cancellationToken);
            return new(items, totalItemsCount);
        }

        public async static Task<Page<TTarget>> Query<TSource, TQuery, TTarget>(this Queryer queryer, IQueryable<TSource> source, Expression<Func<TSource, TTarget>> projection, Query<TQuery> query, CancellationToken cancellationToken)
            => await source.Filter(queryer, query).Order(queryer, query).Select(projection).ToPageAsync(query, cancellationToken);
    }
}
