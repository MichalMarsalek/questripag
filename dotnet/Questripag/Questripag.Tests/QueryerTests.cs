using FluentAssertions;
using System.Linq.Expressions;

namespace Questripag.Tests
{
    public class QueryerTests
    {
        [Theory]
        [MemberData(nameof(GetGetFilterPredicateTestCases))]
        public void QueryerGetFilterPredicate_ReturnsPredicate(FilterCoordinate<dynamic> inputFilter, LambdaExpression expectedPredicate)
        {
            var queryer = new Queryer();
            var result = queryer.GetFilterPredicate<ITestQueryModel, ITestQueryModel>(inputFilter);
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
                    (ITestQueryModel x) => 20.Equals((object)x.Age)
                ),
            };
            return cases.Select(x => new object[] { x.Item1, x.Item2 });
        }
    }
}