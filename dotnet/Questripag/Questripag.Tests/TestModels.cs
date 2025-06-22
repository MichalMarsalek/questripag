using System.Text.Json.Serialization;

namespace Questripag.Tests;

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

public class TestFilterModel
{
    public Filter<string>? Name { get; set; }
    public Filter<int>? Age { get; set; }
    [JsonPropertyName("isActive")]
    public Filter<bool>? AlsoRenamedProp { get; set; }
    public Filter<TestRole>? Role { get; set; }
    public NestedObjectFilterModel? Nested { get; set; }

    public class NestedObjectFilterModel
    {
        public Filter<string>? Property { get; set; }
    }
}

public class TestOrderModel
{
    public Order? Name { get; set; }
    public Order? Age { get; set; }
    [JsonPropertyName("isActive")]
    public Order? AlsoRenamedProp { get; set; }
    public TestRole? Role { get; set; }
    public NestedObjectFilterModel? Nested { get; set; }

    public class NestedObjectFilterModel
    {
        public Order? Property { get; set; }
    }
}

public enum TestRole { User, Contributor, Maintainer, Owner }