namespace Samples.CodeFlow;

public static class StackPrefix
{
    private static int Add(int left, int right) => left + right;

    public static int Run(int value) => Add(5, value > 0 ? 10 : 20);
}
