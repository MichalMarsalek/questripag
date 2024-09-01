using FluentAssertions;
using Questripag.Generator.Js;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Questripag.Tests;

public class GeneratorTests
{
    private string RemoveWhitespace(string text) => Regex.Replace(text, @"\s", "");

    [Theory]
    [MemberData(nameof(GetGetFilterPredicateTestCases))]
    public void GeneratorGenerate_GeneratesConfig(IEnumerable<MethodInfo> inputMethods, string expectedOutputCode)
    {
        var generator = new Generator.Js.Generator(false);
        var result = generator.Generate(inputMethods);
        RemoveWhitespace(result.ToString()).Should().EndWithEquivalentOf(RemoveWhitespace(expectedOutputCode) + ";");
    }

    public static IEnumerable<object[]> GetGetFilterPredicateTestCases()
    {
        return [
            [typeof(TestController1).GetMethods(), """
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
            """],
            [typeof(TestController2).GetMethods(), """
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
            """],
        ];
    }

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
