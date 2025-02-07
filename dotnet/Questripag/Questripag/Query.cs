namespace Questripag;

public interface IPaging
{
    public int Page { get; }
    public int PageSize { get; }
    public int Skip { get; }
}

public class Query<TFilter, TOrder> : IPaging
    where TFilter : notnull
    where TOrder : notnull
{
    public int Page { get; private set; }
    public int PageSize { get; private set; }
    public int Skip => (Page - 1) * PageSize;
    public TFilter Filter { get; private set; }
    public TOrder Order { get; private set; }

    public Query(int page, int pageSize, TFilter filter, TOrder order)
    {
        Page = page;
        PageSize = pageSize;
        Filter = filter;
        Order = order;
    }
}

public abstract class FilterValue<TValue>
{
    public static implicit operator FilterValue<TValue>(TValue value) => new ScalarFilterValue<TValue>(value);
}

public class ScalarFilterValue<TValue> : FilterValue<TValue>
{
    public TValue Value { get; private set; }
    public ScalarFilterValue(TValue value)
    {
        Value = value;
    }
}

public class RangeFilterValue<TValue> : FilterValue<TValue>
{
    public TValue LowerBound { get; private set; }
    public TValue UpperBound { get; private set; }
    public RangeFilterValue(TValue lowerBound, TValue upperBound)
    {
        LowerBound = lowerBound;
        UpperBound = upperBound;
    }
}

public class OrderCoordinate
{
    public int Precedence { get; private set; }
    public OrderDirection Direction { get; private set; }

    public OrderCoordinate(int precedence, OrderDirection direction)
    {
        Precedence = precedence;
        Direction = direction;
    }
}

public enum OrderDirection
{
    Descending = -1,
    None = 0,
    Ascending = 1,
}
