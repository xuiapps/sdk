namespace Samples.CodeFlow;

public static class MixedStacks
{
    public static int Run(int value)
    {
        double number = value + 0.5;
        string message = value > 0 ? "positive" : "other";
        System.Console.WriteLine(message);
        return (int)number;
    }
}
