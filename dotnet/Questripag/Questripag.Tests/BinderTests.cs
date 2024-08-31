using FluentAssertions;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Reflection;

namespace Questripag.Tests
{
    public class BinderTests
    {
        [Theory]
        [MemberData(nameof(GetQueryCollectionToQueryData))]
        public void QueryCollectionToQuery_ReturnsRawQuery(string inputQueryString, Query<ITestQueryModel> expectedRawQuery)
        {
            var binder = new Binder<ITestQueryModel>();
            var input = ParseQueryCollection(inputQueryString);
            binder.QueryCollectionToQuery(input).Should().BeEquivalentTo(expectedRawQuery);
        }

        public static IEnumerable<object[]> GetQueryCollectionToQueryData()
        {
            FilterCoordinate<object> Filter(string key, params FilterValue<object>[] values) => new(key, values);
            FilterValue<object> Scalar(object value) => new ScalarFilterValue<object>(value);
            FilterValue<object> Range(object lower, object upper) => new RangeFilterValue<object>(lower, upper);
            OrderCoordinate Order(string key, bool isDescending) => new(key, isDescending);
            var cases = new List<(string, Query<ITestQueryModel>)>()
            {
                (
                    "page=2@50&order=+name&age=18..65&isActive=true&role=Maintainer",
                    new(2, 50, [Filter("Age", Range(18, 65)), Filter("IsActive", Scalar(true)), Filter("Role", Scalar(TestRole.Maintainer))], [Order("Name", false)])
                ),
            };
            return cases.Select(x => new object[] { x.Item1, x.Item2 });
        }

        private IQueryCollection ParseQueryCollection(string queryString)
            => new QueryCollection(queryString.Split("&").ToDictionary(x => x.Split("=")[0], x => new StringValues(x.Split("=")[1])));

    }
}