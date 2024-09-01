using FluentAssertions;
using System.Linq.Expressions;

namespace Questripag.Tests;

public class QueryerTests
{
    [Theory(DisplayName = "")]
    [MemberData(nameof(GetGetFilterPredicateKeys))]
    public void QueryerGetFilterPredicate_ReturnsPredicate(string key)
    {
        var testCase = GetGetFilterPredicateTestCases()[key];
        var queryer = new Queryer();
        var result = queryer.GetFilterPredicate<TestSource, ITestQueryModel>(testCase.Input);
        result.ToString().Should().BeEquivalentTo(testCase.Output.ToString());
    }

    public static Dictionary<string, TestCase<FilterCoordinate<dynamic>, LambdaExpression>> GetGetFilterPredicateTestCases()
    {
        FilterCoordinate<dynamic> Filter(string key, params FilterValue<dynamic>[] values) => new(key, values);
        FilterValue<dynamic> Scalar(dynamic value) => new ScalarFilterValue<dynamic>(value);
        FilterValue<dynamic> Range(dynamic lower, dynamic upper) => new RangeFilterValue<dynamic>(lower, upper);
        var cases = new List<TestCase<FilterCoordinate<dynamic>, LambdaExpression>>()
        {
            new(
                Filter("Age", Scalar(20)),
                (TestSource x) => 20.Equals((object)x.Age)
            ),new(
                Filter("Nested.Property", Scalar("x")),
                (TestSource x) => "x".Equals((object)x.Nested.Property)
            ),
        };
        return cases.ToDictionary(x => x.Output.ToString(), x=> x);
    }
    public static IEnumerable<object[]> GetGetFilterPredicateKeys => GetGetFilterPredicateTestCases().Keys.Select(x => new object[] { x });

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