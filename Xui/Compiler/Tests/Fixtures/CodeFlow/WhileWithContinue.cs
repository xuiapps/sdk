namespace Samples.CodeFlow;

public static class WhileWithContinue
{
    public static int Run(int value)
    {
        int total = 0;
        while (value > 0)
        {
            value--;
            if (value == 2) continue;
            total += value;
        }
        return total;
    }
}
