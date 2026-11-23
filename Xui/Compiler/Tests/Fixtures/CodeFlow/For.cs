namespace Samples.CodeFlow;

public static class For
{
    public static int Run(int value)
    {
        int total = 0;
        for (int i = 1; i <= value; i++) total += i;
        return total;
    }
}
