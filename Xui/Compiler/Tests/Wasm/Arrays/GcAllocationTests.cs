using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Arrays;

[Collection(nameof(ManagedConsoleCollection))]
public class GcAllocationTests
{
    [Fact]
    public Task ExplicitOutOfMemoryGuardsThrowManagedExceptions() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program { public static int Main() {
            try { throw new OutOfMemoryException(); }
            catch(SystemException error) { Console.WriteLine(error is OutOfMemoryException); Console.WriteLine(error.Message); return 42; }
        } }
        """, "True\nInsufficient memory to continue the execution of the program.\n",42);

    [Fact]
    public Task GenericFactoriesCreateIndependentScalarReferenceAndStructElements() => InlineArrayTests.AssertLinked("""
        using System;
        public struct Entry { public string Text; public int Value; }
        public static class Program {
            static T[] Allocate<T>(int count)=>GC.AllocateUninitializedArray<T>(count);
            public static int Main() {
                var bytes=Allocate<byte>(4096); for(int i=0;i<bytes.Length;i++) bytes[i]=(byte)i;
                Console.WriteLine(bytes[4095]);
                var entries=Allocate<Entry>(2); entries[0].Text="first"; entries[0].Value=42;
                Console.WriteLine(entries[1].Text==null); Console.WriteLine(entries[1].Value);
                Entry copy=entries[0]; copy.Value=99; Console.WriteLine(entries[0].Value);
                var texts=GC.AllocateArray<string>(2); Console.WriteLine(texts[0]==null);
                var numbers=GC.AllocateArray<double>(2); Console.WriteLine(numbers[1]==0);
                Console.WriteLine(Allocate<int>(0).Length);
                try { Allocate<int>(-1); } catch(OverflowException) { Console.WriteLine("overflow"); }
                try { GC.AllocateArray<int>(-1); } catch(OverflowException) { Console.WriteLine("overflow"); }
                return entries[0].Value;
            }
        }
        """, "255\nTrue\n0\n42\nTrue\nTrue\n0\noverflow\noverflow\n",42);

    [Theory]
    [InlineData("AllocateArray")]
    [InlineData("AllocateUninitializedArray")]
    public async Task PinnedGcStorageIsExplicitlyUnsupported(string operation)
    {
        using var app=Canvas.CanvasTests.Read(Frontend.Roslyn.Compile($$"""
            using System;
            public static class Program { public static int Main() {
                try { GC.{{operation}}<byte>(4,pinned:true); return 1; }
                catch(NotSupportedException) { return 42; }
            } }
            """));
        using var framework=new Frontend.Framework();
        _=new Xui.Compiler.XuiNet.Compilation([app,..framework.Assemblies]);
        Assert.Equal(42,(await Repl.Node.ExecuteAsync(new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(app))).ExitCode);
    }
}
