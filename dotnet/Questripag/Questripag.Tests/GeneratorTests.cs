using FluentAssertions;
using Questripag.Generator.Js;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Questripag.Tests;

public class GeneratorTests
{
    private string RemoveWhitespace(string text) => Regex.Replace(text, @"\s", "");

    [Theory(DisplayName = "")]
    [MemberData(nameof(GetGetFilterPredicateKeys))]
    public void GeneratorGenerate_GeneratesConfig(string key)
    {
        var testCase = GetGetFilterPredicateTestCases()[key];
        var generator = new Generator.Js.Generator(false);
        var result = generator.Generate(testCase.Input);
        RemoveWhitespace(result.ToString()).Should().EndWithEquivalentOf(RemoveWhitespace(testCase.Output) + ";");
    }

    public static Dictionary<string, TestCase<IEnumerable<MethodInfo>, string>> GetGetFilterPredicateTestCases()
    {
        var cases = new List<TestCase<IEnumerable<MethodInfo>, string>>() {
            new(typeof(TestController1).GetMethods(), """
                {
                    TestEndpoint: {
                        Name: {response: true, filter: false, order: true, type: "String"},
                        Age: {response: true, filter: true, order: true, type: "Integer"},
                        IsActive: {response: true, filter: true, order: true, type: "Boolean"},
                        Role: {response: true, filter: true, order: false, type: "Enumerable/TestRole"},
                        PropertyThatDoesntSupportAnyOperations: {response: true, filter: false, order: false, type: "String"},
                        Nested: {response: true, filter: false, order: false, type: "Other"},
                        PropertyThatIsntInTheResponse: {response: false, filter: true, order: true, type: "String"},
                        "Nested.Property": {response: false, filter: true, order: true, type: "String"}
                    }
                }
            """),
            new(typeof(TestController2).GetMethods(), """
                {
                    TestEndpoint1: {
                        Name: {response: false, filter: true, order: true, type: "String"}
                    },
                    TestEndpoint2: {
                        Name: {response: true, filter: false, order: false, type: "String"}
                    },
                    TestEndpoint3: {
                        Name: {response: true, filter: false, order: false, type: "String"}
                    }
                }
            """),
        };
        return cases.ToDictionary(x => x.Input.First().DeclaringType!.Name, x => x);
    }
    public static IEnumerable<object[]> GetGetFilterPredicateKeys => GetGetFilterPredicateTestCases().Keys.Select(x => new object[] { x });

    public class TestController1
    {
        public Task<TestResponseWrapper<Page<TestResponse>, string>> TestEndpoint(int someOtherParameter, Query<ITestQueryModel> query)
        {
            throw new NotSupportedException();
        }
    }

    public class TestController2
    {
        public int TestEndpoint1(Query<IQueryModelSmaller> query)
        {
            throw new NotSupportedException();
        }
        public Page<TestResponseSmaller> TestEndpoint2()
        {
            throw new NotSupportedException();
        }
        public Page<TestResponseSmaller> TestEndpoint3(Query<IUnit> query)
        {
            throw new NotSupportedException();
        }
    }

    public interface IUnit { }
}
