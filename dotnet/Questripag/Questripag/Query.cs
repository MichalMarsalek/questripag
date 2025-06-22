namespace Questripag;

public interface IPaging
{
    public int Page { get; }
    public int PageSize { get; }
    public int Skip { get; }
}

public class Query<TFilter, TOrder> : IPaging
    where TFilter : class, new()
    where TOrder : class, new()
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

public abstract class Filter
{
    public StringFilterOperation StringOperation { get; set; }
    public DateTimeFilterPrecision DateTimePrecision { get; set; }
    internal List<object> UntypedValues { get; set; } = [];
    internal List<(object?, object?)> UntypedRanges { get; set; } = [];
}

public class Filter<TValue> : Filter
{
    public IEnumerable<TValue> TypedValues => UntypedValues.Cast<TValue>();

    public IEnumerable<(TValue?, TValue?)> TypedRanges => UntypedRanges.Select(x => ((TValue?)x.Item1, (TValue?)x.Item2));

    public static implicit operator Filter<TValue>(TValue value) => new Filter<TValue> { UntypedValues = [value] };

    public static implicit operator Filter<TValue>(List<TValue> values) => new Filter<TValue> { UntypedValues = [..values] };

    public static implicit operator Filter<TValue>((TValue?, TValue?) range) => new Filter<TValue> { UntypedRanges = [range] };

    public Filter<TValue> WithStringOperation(StringFilterOperation operation)
    {
        StringOperation = operation;
        return this;
    }
    public Filter<TValue> WithDateTimePrecision(DateTimeFilterPrecision precision)
    {
        DateTimePrecision = precision;
        return this;
    }
}

public class Order
{
    public int Precedence { get; private set; }
    public bool IsDescending { get; private set; }

    public Order(int precedence, bool isDescending)
    {
        Precedence = precedence;
        IsDescending = isDescending;
    }
}

public enum StringFilterOperation { Equals, StartsWith, Contains }
public enum DateTimeFilterPrecision { Exact, Seconds, Minutes, Days }