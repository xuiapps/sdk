namespace Samples.CodeFlow;

public static class DoWhile
{
    public static int Run(int value)
    {
        int total = 0;
        do
        {
            total += value;
            value--;
        }
        while (value > 0);
        return total;
    }
}
