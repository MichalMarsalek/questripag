namespace Questripag;

public interface IPaging
{
    public int Page { get; }
    public int PageSize { get; }
    public int Skip { get; }
}

public interface IFiltering<TQueryModel>
{
    public IEnumerable<FilterCoordinate<object>> Filters { get; }
}
public interface IOrdering<TQueryModel>
{
    public IEnumerable<OrderCoordinate> Orders { get; }
}

public class Query<TQueryModel> : IPaging, IFiltering<TQueryModel>, IOrdering<TQueryModel>
{
    public int Page { get; private set; }
    public int PageSize { get; private set; }
    public int Skip => (Page - 1) * PageSize;
    public IEnumerable<FilterCoordinate<object>> Filters { get; private set; }
    public IEnumerable<OrderCoordinate> Orders { get; private set; }

    public Query(int page, int pageSize, IEnumerable<FilterCoordinate<object>> filters, IEnumerable<OrderCoordinate> orders)
    {
        Page = page;
        PageSize = pageSize;
        Filters = filters;
        Orders = orders;
    }
}

public class FilterCoordinate<TValue>
{
    public string Key { get; private set; }
    public IEnumerable<FilterValue<TValue>> Value {get; private set; }
    public FilterCoordinate(string key, IEnumerable<FilterValue<TValue>> value)
    {
        Key = key;
        Value = value;
    }
}

public abstract class FilterValue<TValue> { }

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
    public string Key { get; private set; }
    public bool IsDescending { get; private set; }

    public OrderCoordinate(string key, bool isDescending)
    {
        Key = key;
        IsDescending = isDescending;
    }
}
