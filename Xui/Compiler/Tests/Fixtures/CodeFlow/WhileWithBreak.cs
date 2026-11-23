namespace Samples.CodeFlow;

public static class WhileWithBreak
{
    public static int Run(int value)
    {
        int total = 0;
        while (value > 0)
        {
            if (value == 3) break;
            total += value;
            value--;
        }
        return total;
    }
}
