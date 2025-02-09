using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Questripag;

public class RawQuery
{
    public int Page { get; private set; }
    public int PageSize { get; private set; }
    public int Skip => (Page - 1) * PageSize;
    public IEnumerable<RawFilterCoordinate> Filters { get; private set; }
    public IEnumerable<Order> Orders { get; private set; }

    public RawQuery(int page, int pageSize, IEnumerable<RawFilterCoordinate> filters, IEnumerable<Order> orders)
    {
        Page = page;
        PageSize = pageSize;
        Filters = filters;
        Orders = orders;
    }
}

public class RawFilterCoordinate
{
    public string Key { get; private set; }
    public IEnumerable<Filter<string>> Value {get; private set; }
    public RawFilterCoordinate(string key, IEnumerable<Filter<string>> value)
    {
        Key = key;
        Value = value;
    }
}
