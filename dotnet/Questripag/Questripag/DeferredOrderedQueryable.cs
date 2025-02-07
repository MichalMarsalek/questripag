using System.Collections;
using System.Linq.Expressions;

namespace Questripag;

public class DeferredOrderedQueryable<TSource> : IQueryable<TSource>
{
    private IQueryable<TSource> source = null!;
    private List<(Expression<Func<TSource, object>>, OrderCoordinate)> deferredOrders = new();

    public static DeferredOrderedQueryable<TSource> FromIQueryable(IQueryable<TSource> source)
    {
        return new DeferredOrderedQueryable<TSource> { source = source };
    }

    public DeferredOrderedQueryable<TSource> OrderBy<TProperty>(Expression<Func<TSource, TProperty>> selector, OrderCoordinate order)
    {
        return new DeferredOrderedQueryable<TSource> { source = source, deferredOrders = deferredOrders.Append((Expression.Lambda<Func<TSource, object>>(selector.Body, selector.Parameters), order)).ToList() };
    }

    public IQueryable<TSource> CommitOrder()
    {
        var actualOrders = deferredOrders.Where(x => x.Item2.Direction != OrderDirection.None).OrderBy(x => x.Item2.Precedence).ToList();
        if (!actualOrders.Any()) return source;
        var result = actualOrders.First().Item2.Direction == OrderDirection.Descending ? source.OrderByDescending(actualOrders.First().Item1) : source.OrderBy(actualOrders.First().Item1);

        foreach (var actualOrder in actualOrders.Skip(1))
        {
            result = actualOrder.Item2.Direction == OrderDirection.Descending ? result.ThenByDescending(actualOrder.Item1) : result.ThenByDescending(actualOrder.Item1);
        }
        return result;
    }

    public IEnumerator<TSource> GetEnumerator() => CommitOrder().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => CommitOrder().GetEnumerator();

    public Type ElementType => source.ElementType;

    public Expression Expression => CommitOrder().Expression;

    public IQueryProvider Provider => CommitOrder().Provider;
}
