using System.Collections;
using System.Linq.Expressions;

namespace Questripag;

public class DeferredOrderedQueryable<TSource> : IQueryable<TSource>
{
    private IQueryable<TSource> source = null!;
    private List<(Expression<Func<TSource, object>>, Order)> deferredOrders = new();

    public static DeferredOrderedQueryable<TSource> FromIQueryable(IQueryable<TSource> source)
    {
        return new DeferredOrderedQueryable<TSource> { source = source };
    }

    public DeferredOrderedQueryable<TSource> OrderBy<TProperty>(Expression<Func<TSource, TProperty>> selector, Order? order)
    {
        return order is null ? this : new DeferredOrderedQueryable<TSource> { source = source, deferredOrders = deferredOrders.Append((Expression.Lambda<Func<TSource, object>>(selector.Body, selector.Parameters), order)).ToList() };
    }

    public IQueryable<TSource> CommitOrder()
    {
        var actualOrders = deferredOrders.OrderBy(x => x.Item2.Precedence).ToList();
        if (!actualOrders.Any()) return source;
        var result = actualOrders.First().Item2.IsDescending ? source.OrderByDescending(actualOrders.First().Item1) : source.OrderBy(actualOrders.First().Item1);

        foreach (var actualOrder in actualOrders.Skip(1))
        {
            result = actualOrder.Item2.IsDescending ? result.ThenByDescending(actualOrder.Item1) : result.ThenBy(actualOrder.Item1);
        }
        return result;
    }

    public IEnumerator<TSource> GetEnumerator() => CommitOrder().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => CommitOrder().GetEnumerator();

    public Type ElementType => source.ElementType;

    public Expression Expression => CommitOrder().Expression;

    public IQueryProvider Provider => CommitOrder().Provider;
}
