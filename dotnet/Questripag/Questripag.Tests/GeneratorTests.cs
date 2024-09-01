using FluentAssertions;
using Questripag.Generator.Js;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Questripag.Tests
{
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
                            Name: {response: true, filter: false, order: true},
                            Age: {response: true, filter: true, order: true},
                            IsActive: {response: true, filter: true, order: true},
                            Role: {response: true, filter: true, order: false},
                            PropertyThatDoesntSupportAnyOperations: {response: true, filter: false, order: false},
                            PropertyThatIsntInTheResponse: {response: false, filter: true, order: true}
                        }
                    }
                """],
                [typeof(TestController2).GetMethods(), """
                    {
                        TestEndpoint1: {
                            Name: {response: false, filter: false, order: true},
                            Age: {response: false, filter: true, order: true},
                            IsActive: {response: false, filter: true, order: true},
                            Role: {response: false, filter: true, order: false},
                            PropertyThatIsntInTheResponse: {response: false, filter: true, order: true}
                        },
                        TestEndpoint2: {
                            Name: {response: true, filter: false, order: false},
                            Age: {response: true, filter: false, order: false},
                            IsActive: {response: true, filter: false, order: false},
                            Role: {response: true, filter: false, order: false},
                            PropertyThatDoesntSupportAnyOperations: {response: true, filter: false, order: false}
                        },
                        TestEndpoint3: {
                            Name: {response: true, filter: false, order: false},
                            Age: {response: true, filter: false, order: false},
                            IsActive: {response: true, filter: false, order: false},
                            Role: {response: true, filter: false, order: false},
                            PropertyThatDoesntSupportAnyOperations: {response: true, filter: false, order: false}
                        }
                    }
                """],
            ];
        }
    }
}