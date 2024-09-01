using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace Questripag.Tests;

public class BinderTests
{
    [Theory(DisplayName = "")]
    [MemberData(nameof(GetQueryCollectionToQueryKeys))]
    public void BinderQueryCollectionToQuery_ReturnsRawQuery(string key)
    {
        var testCase = GetQueryCollectionToQueryTestCases()[key];
        var binder = new QueryBinder<ITestQueryModel>(new QueryBinderProvider());
        var input = ParseQueryCollection(testCase.Input);
        binder.QueryCollectionToQuery(input).Should().BeEquivalentTo(testCase.Output);
    }

    public static Dictionary<string, TestCase<string, Query<ITestQueryModel>>> GetQueryCollectionToQueryTestCases()
    {
        FilterCoordinate<object> Filter(string key, params FilterValue<object>[] values) => new(key, values);
        FilterValue<object> Scalar(object value) => new ScalarFilterValue<object>(value);
        FilterValue<object> Range(object lower, object upper) => new RangeFilterValue<object>(lower, upper);
        OrderCoordinate Order(string key, bool isDescending) => new(key, isDescending);
        var cases = new List<TestCase<string, Query<ITestQueryModel>>>()
        {   new(
                "page=1@10",
                new(1, 10, [], [])
            ),
            new(
                "page=1@10&order=name&order=-age",
                new(1, 10, [], [Order("Name", false), Order("Age", true)])
            ),
            new(
                "page=2@50&order=+name&age=18..65&isActive=true&role=Maintainer",
                new(2, 50, [Filter("Age", Range(18, 65)), Filter("isActive", Scalar(true)), Filter("Role", Scalar(TestRole.Maintainer))], [Order("Name", false)])
            ),
        };
        return cases.ToDictionary(x => x.Input, x => x);
    }

    public static IEnumerable<object[]> GetQueryCollectionToQueryKeys => GetQueryCollectionToQueryTestCases().Keys.Select(x => new object[] { x });

    private IQueryCollection ParseQueryCollection(string queryString)
        => new QueryCollection(queryString.Split("&").Select(x => x.Split("=")).GroupBy(x => x[0]).ToDictionary(x => x.Key, x => new StringValues(x.Select(x => x[1]).ToArray())));

}