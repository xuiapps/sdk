[assembly: System.Reflection.AssemblyVersion("1.0.0.0")]

namespace Numbers;

public static class Arithmetic
{
    public static int Add(int left, int right) => left + right;

    // This overload and the unused class must not enter the WASM module.
    public static long Add(long left, long right) => left + right;
}

public static class Unused
{
    public static int Fail() => throw new System.NotSupportedException();
}
