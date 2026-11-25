namespace Samples.CodeFlow;

public static class Switch
{
    public static int Run(int value)
    {
        int result;
        switch (value)
        {
            case 0: result = 10; break;
            case 1:
            case 2: result = 20; break;
            case 3: result = 30; break;
            case 4: result = 40; break;
            default: result = -1; break;
        }
        return result + 1;
    }
}
