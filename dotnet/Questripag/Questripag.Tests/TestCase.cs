namespace Questripag.Tests;

public class TestCase<TInput, TOutput>
{
    public TInput Input { get; private set; }
    public TOutput Output { get; private set; }

    public TestCase(TInput input, TOutput output)
    {
        Input = input;
        Output = output;
    }
}
