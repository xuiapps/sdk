namespace Samples.CodeFlow;

public static class IfElse
{
    public static int Run(int value)
    {
        int result;
        if (value > 0)
            result = value + 10;
        else
            result = value - 10;
        return result;
    }
}
