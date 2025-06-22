using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace Questripag.Tests;

public class BinderTests
{
    [Theory(DisplayName = "")]
    [KeyedTestCases(nameof(QueryCollectionToQueryTestCases))]
    public void BinderQueryCollectionToQuery_ReturnsQuery(string key)
    {
        var testCase = QueryCollectionToQueryTestCases[key];
        var binder = new QueryBinder<TestFilterModel, TestOrderModel>(new QueryBinderProvider());
        var input = ParseQueryCollection(testCase.Input);
        var output = testCase.Output;
        output.Filter.Nested ??= new();
        output.Order.Nested ??= new();
        binder.QueryCollectionToQuery(input).Should().BeEquivalentTo(output);
    }

    public static Order Asc(int prec = 0) => new(prec, false);
    public static Order Desc(int prec = 0) => new(prec, true);

    public static Dictionary<string, TestCase<string, Query<TestFilterModel, TestOrderModel>>> QueryCollectionToQueryTestCases =
        new List<TestCase<string, Query<TestFilterModel, TestOrderModel>>>()
        {   new(
                "page=1@10",
                new(1, 10, new(), new())
            ),
            new(
                "page=1@10&order=name&order=-age",
                new(1, 10, new(), new(){Name = Asc(), Age = Desc(1)})
            ),
            new(
                "page=2@50&order=+name&age=18..65&isActive=true&role=Maintainer|Owner",
                new(2, 50, new(){Age = (18, 65), AlsoRenamedProp = true, Role = new List<TestRole> { TestRole.Maintainer, TestRole.Owner } }, new(){Name = Asc()})
            ),
            new(
                "page=1@50&nested.property=xx|yy",
                new(1, 50, new(){Nested = new() {Property = "xx|yy" } }, new())
            ),
        }.ToDictionary(x => x.Input, x => x);

    private IQueryCollection ParseQueryCollection(string queryString)
        => new QueryCollection(queryString.Split("&").Select(x => x.Split("=")).GroupBy(x => x[0]).ToDictionary(x => x.Key, x => new StringValues(x.Select(x => x[1]).ToArray())));

}