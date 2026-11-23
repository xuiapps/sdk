using System;

public static class Program
{
    private static int Add(int left, int right) => left + right;
    private static int Choose(int value) => Add(5, value > 0 ? 10 : 20);
    private static double Number(int value) => 0.5 + (value > 0 ? 1.25 : 2.5);
    private static string Message(int value) => value < 0 ? "negative" : value == 0 ? "zero" : "positive";

    private static void PrintMessage(int value) => Console.WriteLine(value > 0 ? "yes" : "no");

    public static int Main()
    {
        Console.WriteLine(Choose(1));
        Console.WriteLine(Choose(-1));
        Console.WriteLine(Number(1));
        Console.WriteLine(Number(-1));
        Console.WriteLine(Message(-1));
        Console.WriteLine(Message(0));
        Console.WriteLine(Message(1));
        PrintMessage(1);
        PrintMessage(-1);
        return Choose(1);
    }
}
