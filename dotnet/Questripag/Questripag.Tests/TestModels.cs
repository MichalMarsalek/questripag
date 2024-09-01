using System.Text.Json.Serialization;

namespace Questripag.Tests;

public interface ITestQueryModel
{
    [NoFilter]
    public string Name { get; set; }
    public int Age { get; set; }
    [JsonPropertyName("isActive")]
    public bool RenamedProp { get; set; }
    [NoOrder]
    public TestRole Role { get; set; }
    public string PropertyThatIsntInTheResponse { get; set; }
    public string Nested_Property { get; set; }
}
public interface IQueryModelSmaller
{
    public string Name { get; set; }
}
public class TestResponse
{
    public string Name { get; set; }
    public int Age { get; set; }
    [JsonPropertyName("isActive")]
    public bool AlsoRenamedProp { get; set; }
    public TestRole Role { get; set; }
    public string PropertyThatDoesntSupportAnyOperations { get; set; }
    public NestedObject Nested { get; set; }

    public class NestedObject
    {
        public string Property { get; set; }
    }
}
public class TestResponseSmaller
{
    public string Name { get; set; }
}

public enum TestRole { User, Contributor, Maintainer, Owner }

public class TestResponseWrapper<TResponse, TError>
{
    public TResponse? Response { get; set; }
    public TError? Error { get; set; }
}