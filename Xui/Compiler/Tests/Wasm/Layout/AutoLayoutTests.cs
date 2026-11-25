using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Layout;

[Collection(nameof(ManagedConsoleCollection))]
public class AutoLayoutTests
{
    [Fact]
    public Task AutoLayoutValuesKeepCopiesAndReferencesIndependent() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Runtime.InteropServices;
        [StructLayout(LayoutKind.Auto)] public struct Numbers { public byte Tag; public long Value; public short Tail; }
        [StructLayout(LayoutKind.Auto)] public struct Pair<T> { public T Value; public string Name; }
        public class Box { public Pair<Numbers> Value; }
        public static class Program {
            static Pair<Numbers> saved;
            static void Change(ref Pair<Numbers> value) { value.Value.Value=99; value.Name="changed"; }
            public static int Main() {
                var a=new Pair<Numbers> { Value=new Numbers { Tag=1, Value=40, Tail=2 }, Name="original" };
                saved=a;
                var box=new Box { Value=a };
                Change(ref a); Change(ref box.Value);
                Console.WriteLine(a.Name); Console.WriteLine(box.Value.Value.Value);
                Console.WriteLine(saved.Name);
                return (int)saved.Value.Value + saved.Value.Tail;
            }
        }
        """, "changed\n99\noriginal\n", 42);

    [Fact]
    public Task FrameworkDateTimeValuesExecuteFromTheirRealBodies() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static long Read(in DateTime value) => value.Ticks;
            public static int Main() {
                var value=new DateTime(42, DateTimeKind.Utc);
                var copy=value;
                Console.WriteLine(DateTime.MinValue.Ticks);
                Console.WriteLine(DateTime.MaxValue.Ticks);
                Console.WriteLine((int)copy.Kind);
                return (int)Read(in copy);
            }
        }
        """, "0\n3155378975999999999\n1\n", 42);
}
