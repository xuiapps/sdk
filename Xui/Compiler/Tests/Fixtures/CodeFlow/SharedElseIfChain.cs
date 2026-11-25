namespace Samples.CodeFlow;

public static class SharedElseIfChain
{
    private static int checks;
    private static bool Check(int value) { checks++; return (value & 1) != 0; }

    public static int Run(int value)
    {
        checks = 0;
        int result;
        if (value > 10 && Check(value)) result = 1;
        else if (value > 8 && Check(value + 1)) result = 2;
        else if (value > 6 && Check(value)) result = 3;
        else if (value > 4 && Check(value + 1)) result = 4;
        else if (value > 2 && Check(value)) result = 5;
        else if (value > 0 && Check(value + 1)) result = 6;
        else result = value < 0 ? 7 : 8;
        int other = 12;
        ref int selected = ref (value < 0 ? ref result : ref other);
        selected += 1;
        int extra = value switch { 0 => 10, 1 => 20, 2 => 30, 3 => 40, 4 => 50, _ => 60 };
        return checks * 100 + (result > 4 ? result : result + 10) + other + extra;
    }
}
