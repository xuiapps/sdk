namespace Samples.CodeFlow;

public static class EarlyReturn
{
    public static int Run(int value)
    {
        if (value < 0)
            return -value;
        if (value == 0)
            return 10;
        return value + 1;
    }
}
