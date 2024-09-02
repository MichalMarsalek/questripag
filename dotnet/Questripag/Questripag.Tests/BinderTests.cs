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
        var binder = new QueryBinder<ITestQueryModel>(new QueryBinderProvider());
        var input = ParseQueryCollection(testCase.Input);
        binder.QueryCollectionToQuery(input).Should().BeEquivalentTo(testCase.Output);
    }
    public static FilterCoordinate<object> Filter(string key, params FilterValue<object>[] values) => new(key, values);
    public static FilterValue<object> Scalar(object value) => new ScalarFilterValue<object>(value);
    public static FilterValue<object> Range(object lower, object upper) => new RangeFilterValue<object>(lower, upper);
    public static OrderCoordinate Order(string key, bool isDescending) => new(key, isDescending);

    public static Dictionary<string, TestCase<string, Query<ITestQueryModel>>> QueryCollectionToQueryTestCases =
        new List<TestCase<string, Query<ITestQueryModel>>>()
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
        }.ToDictionary(x => x.Input, x => x);

    private IQueryCollection ParseQueryCollection(string queryString)
        => new QueryCollection(queryString.Split("&").Select(x => x.Split("=")).GroupBy(x => x[0]).ToDictionary(x => x.Key, x => new StringValues(x.Select(x => x[1]).ToArray())));

}