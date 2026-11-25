internal static class Program
{
    public static int Main()
    {
        System.Console.WriteLine("From Application.exe");
        int input = Numbers.Arithmetic.Add(38, 1);
        int result = Reports.Report.Run(input);
        System.Console.WriteLine(result);
        return result;
    }
}
