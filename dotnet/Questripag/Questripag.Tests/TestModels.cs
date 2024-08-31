namespace Questripag.Tests;

public interface ITestQueryModel
{
    public string Name { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public TestRole Role { get; set; }
}

public enum TestRole { User, Contributor, Maintainer, Owner }
