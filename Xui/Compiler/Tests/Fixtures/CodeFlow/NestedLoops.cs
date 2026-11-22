namespace Samples.CodeFlow;

public static class NestedLoops
{
    public static int Run(int value)
    {
        int total = 0;
        while (value > 0)
        {
            int inner = 2;
            do
            {
                total += value;
                inner--;
            }
            while (inner > 0);
            value--;
        }
        return total;
    }
}
