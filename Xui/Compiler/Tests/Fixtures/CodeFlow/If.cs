namespace Samples.CodeFlow;

public static class If
{
    public static int Run(int value)
    {
        int result = value;
        if (value > 0)
            result = value + 10;
        return result;
    }
}
