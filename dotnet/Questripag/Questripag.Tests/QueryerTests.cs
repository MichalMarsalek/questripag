using FluentAssertions;
using System.Linq.Expressions;

namespace Questripag.Tests;

public class QueryerTests
{
    [Theory]
    [MemberData(nameof(GetGetFilterPredicateTestCases))]
    public void QueryerGetFilterPredicate_ReturnsPredicate(FilterCoordinate<dynamic> inputFilter, LambdaExpression expectedPredicate)
    {
        var queryer = new Queryer();
        var result = queryer.GetFilterPredicate<TestSource, ITestQueryModel>(inputFilter);
        result.ToString().Should().BeEquivalentTo(expectedPredicate.ToString());
    }

    public static IEnumerable<object[]> GetGetFilterPredicateTestCases()
    {
        FilterCoordinate<dynamic> Filter(string key, params FilterValue<dynamic>[] values) => new(key, values);
        FilterValue<dynamic> Scalar(dynamic value) => new ScalarFilterValue<dynamic>(value);
        FilterValue<dynamic> Range(dynamic lower, dynamic upper) => new RangeFilterValue<dynamic>(lower, upper);
        var cases = new List<(FilterCoordinate<dynamic>, LambdaExpression)>()
        {
            (
                Filter("Age", Scalar(20)),
                (TestSource x) => 20.Equals((object)x.Age)
            ),(
                Filter("Nested.Property", Scalar("x")),
                (TestSource x) => "x".Equals((object)x.Nested.Property)
            ),
        };
        return cases.Select(x => new object[] { x.Item1, x.Item2 });
    }

    public class TestSource
    {
        public int Age { get; set; }
        public TestNested Nested { get; set; }
    }

    public class TestNested
    {
        public string Property { get; set; }
    }
}