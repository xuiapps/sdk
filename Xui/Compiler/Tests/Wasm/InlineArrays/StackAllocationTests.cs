using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.InlineArrays;

[Collection(nameof(ManagedConsoleCollection))]
public class StackAllocationTests
{
    [Theory]
    [InlineData(2_000_000)]
    [InlineData(int.MaxValue)]
    public async Task OversizedBlocksTrapBeforeWritingOutsideTheStack(int length)
    {
        using var app = Canvas.CanvasTests.Read(Frontend.Roslyn.Compile($$"""
            using System;
            public static class Program {
                public static int Main() {
                    Span<byte> buffer=stackalloc byte[{{length}}];
                    buffer[buffer.Length-1]=1; return buffer.Length;
                }
            }
            """));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var wasm = new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(app);
        await Repl.Node.AssertTrapsAsync(wasm);
    }

    [Fact]
    public Task UnsignedAllocationSizeMultiplicationChecksBothWidths() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static bool Product(uint a,uint b,uint expected,bool overflow=false) {
                try { return checked(a*b)==expected && !overflow; } catch(OverflowException) { return overflow; }
            }
            static bool Wide(ulong a,ulong b,ulong expected,bool overflow=false) {
                try { return checked(a*b)==expected && !overflow; } catch(OverflowException) { return overflow; }
            }
            public static int Main() {
                if(!Product(uint.MaxValue,0,0) || !Product(uint.MaxValue,1,uint.MaxValue) || !Product(uint.MaxValue,2,0,true)) return 1;
                if(!Product(65535,65535,4294836225) || !Product(65536,65536,0,true)) return 2;
                if(!Wide(ulong.MaxValue,0,0) || !Wide(ulong.MaxValue,1,ulong.MaxValue) || !Wide(ulong.MaxValue,2,0,true)) return 3;
                if(!Wide(ulong.MaxValue/3,3,ulong.MaxValue) || !Wide(ulong.MaxValue/3+1,3,0,true)) return 4;
                return 42;
            }
        }
        """, "",42);

    [Fact]
    public Task DynamicCharacterBuffersSupportSlicesAndStringConstruction() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static string Make(string text) {
                Span<char> buffer=stackalloc char[text.Length+2];
                buffer[0]='['; text.AsSpan().CopyTo(buffer.Slice(1)); buffer[buffer.Length-1]=']';
                return new string(buffer);
            }
            public static int Main() {
                Console.WriteLine(Make("🙂Ж")); Console.WriteLine(Make(""));
                Span<int> empty=stackalloc int[0]; Console.WriteLine(empty.IsEmpty);
                Span<double> numbers=stackalloc double[4]; numbers[0]=20.5; numbers[3]=21.5;
                return (int)(numbers[0]+numbers[3]);
            }
        }
        """, "[🙂Ж]\n[]\nTrue\n", 42);

    [Fact]
    public Task DynamicBlocksSurviveCallsAndAreReleasedOnReturnAndUnwind() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            static int Work(int count, bool fail) {
                Span<long> values=stackalloc long[count]; values[0]=40; values[count-1]=2;
                Span<byte> nested=stackalloc byte[count]; nested[0]=12;
                if(fail) throw new InvalidOperationException();
                if(count>1024 && Work(1024,false)!=42) return -1;
                return (int)(values[0]+values[count-1]);
            }
            public static int Main() {
                for(int i=0;i<500;i++) {
                    try { Work(2048,true); } catch(InvalidOperationException) { }
                    if(Work(2048,false)!=42) return -1;
                }
                return 42;
            }
        }
        """, "", 42);

    [Fact]
    public Task MultipleLoopAllocationsKeepEarlierSpansAlive() => InlineArrayTests.AssertLinked("""
        using System;
        public static class Program {
            public static int Main() {
                Span<int> first=stackalloc int[10]; first[0]=20;
                for(int i=0;i<30;i++) {
                    Span<int> next=stackalloc int[10]; next[0]=i;
                    if(first[0]!=20) return -1;
                }
                return first[0]+22;
            }
        }
        """, "", 42);
}
