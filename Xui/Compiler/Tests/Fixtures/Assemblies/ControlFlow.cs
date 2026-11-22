using System;
using System.Reflection;

[assembly: AssemblyVersion("2.0.0.0")]

namespace Samples.ControlFlow;

public interface IClassifier
{
    string Classify(int value);
}

public readonly struct Range
{
    public readonly int Min;
    public readonly int Max;

    public Range(int min, int max) { Min = min; Max = max; }
}

public sealed class Classifier : IClassifier
{
    public string Classify(int value)
    {
        try
        {
            switch (value)
            {
                case 0: return "zero";
                case 1: return "one";
                case 2: return "two";
                case 3: return "three";
                case 4: return "four";
                default: return "other";
            }
        }
        finally { Console.WriteLine("classified"); }
    }

    public int Sum(int count)
    {
        int total = 0;
        for (int i = 0; i < count; i++) total += i;
        return total;
    }

    public int Divide(int value)
    {
        try { return 100 / value; }
        catch (DivideByZeroException) { return -1; }
    }
}

public static class Numbers
{
    public static int Negative() => -123456;
    public static long Large() => 0x1122334455667788;
    public static float Single() => 1.5f;
    public static double Double() => -1.25;
}
