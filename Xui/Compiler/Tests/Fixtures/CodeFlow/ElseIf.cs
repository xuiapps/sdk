namespace Samples.CodeFlow;

public static class ElseIf
{
    public static int Run(int value)
    {
        int result;
        if (value < 0)
            result = -1;
        else if (value == 0)
            result = 0;
        else
            result = 1;
        return result;
    }
}
