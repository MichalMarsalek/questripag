namespace Questripag.Tests;

public interface ITestQueryModel
{
    [NoFilter]
    public string Name { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    [NoOrder]
    public TestRole Role { get; set; }
    public string PropertyThatIsntInTheResponse { get; set; }
}
public class TestResponse
{
    public string Name { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public TestRole Role { get; set; }
    public string PropertyThatDoesntSupportAnyOperations { get; set; }
}

public enum TestRole { User, Contributor, Maintainer, Owner }

public class TestResponseWrapper<TResponse, TError>
{
    public TResponse? Response { get; set; }
    public TError? Error { get; set; }
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
    public int TestEndpoint1(Query<ITestQueryModel> query)
    {
        throw new NotSupportedException();
    }
    public Page<TestResponse> TestEndpoint2()
    {
        throw new NotSupportedException();
    }
    public Page<TestResponse> TestEndpoint3(Query<IUnit> query)
    {
        throw new NotSupportedException();
    }
}

public interface IUnit { }