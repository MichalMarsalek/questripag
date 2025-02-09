using Microsoft.EntityFrameworkCore;

namespace Questripag.EFCore;

public static class Extensions
{
    public async static Task<Page<TSource>> ToPageAsync<TSource>(this IQueryable<TSource> source, IPaging paging, CancellationToken cancellationToken)
    {
        var items = await source.Page(paging).ToListAsync(cancellationToken);
        var totalItemsCount = items.Count < paging.PageSize ? paging.Skip + items.Count : await source.CountAsync(cancellationToken);
        return new(items, totalItemsCount);
    }
}
