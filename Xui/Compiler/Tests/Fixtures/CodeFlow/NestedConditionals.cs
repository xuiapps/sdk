namespace Samples.CodeFlow;

public static class NestedConditionals
{
    public static int Run(int value)
    {
        if (value > 0)
        {
            if (value > 1) return 20;
            return 10;
        }
        else
        {
            if (value < 0) return -10;
            return 0;
        }
    }
}
