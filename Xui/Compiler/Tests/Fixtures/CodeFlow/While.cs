namespace Samples.CodeFlow;

public static class While
{
    public static int Run(int value)
    {
        int total = 0;
        while (value > 0)
        {
            total += value;
            value--;
        }
        return total;
    }
}
