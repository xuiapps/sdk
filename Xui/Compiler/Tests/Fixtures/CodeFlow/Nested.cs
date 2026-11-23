namespace Samples.CodeFlow;

public static class Nested
{
    public static int Run(int value)
    {
        int total = 0;
        while (value > 0)
        {
            if (value > 2)
                total += value;
            else if (value == 2)
                total += 20;
            else
                total += 10;
            value--;
        }
        return total;
    }
}
