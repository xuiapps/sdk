using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Generics;

[Collection(nameof(ManagedConsoleCollection))]
public class IntegerHashTests
{
    [Fact]
    public Task ConstrainedIntegerAndEnumHashingUsesTheirExactBits() => GcTests.AssertExecution("""
        using System;
        public enum Signed : int { Minimum = int.MinValue }
        public enum Unsigned : uint { Maximum = uint.MaxValue }
        public static class Program {
            static int Hash<T>(T key) where T : struct => key.GetHashCode();
            static int Borrowed<T>(ref T key) where T : struct => key.GetHashCode();
            public static int Main() {
                Console.WriteLine(Hash(0)); Console.WriteLine(Hash(-1)); Console.WriteLine(Hash(int.MinValue));
                Console.WriteLine(Hash(uint.MaxValue)); Console.WriteLine(Hash(0x80000000u));
                Console.WriteLine(Hash(Signed.Minimum)); Console.WriteLine(Hash(Unsigned.Maximum));
                int value = 42; return Borrowed(ref value);
            }
        }
        """, "0\n-1\n-2147483648\n-1\n-2147483648\n-2147483648\n-1\n", 42);
}
