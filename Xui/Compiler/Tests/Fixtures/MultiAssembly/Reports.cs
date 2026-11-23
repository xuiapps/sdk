namespace Reports;

public static class Report
{
    public static int Run(int value)
    {
        if (value < 0) return 0;
        System.Console.WriteLine("From Reports.dll");
        int result = Numbers.Arithmetic.Add(value, 2);
        System.Console.WriteLine(result);
        return Numbers.Arithmetic.Add(result, 1);
    }
}
