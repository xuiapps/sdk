using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Threading;

[Collection(nameof(ManagedConsoleCollection))]
public class MemoryBarrierTests
{
    [Fact]
    public Task SingleAgentFencesPreserveWritesAndSubsequentReads() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Threading;
        public static class Program {
            static int state;
            static object published;
            public static int Main() {
                state=20;
                Thread.MemoryBarrier();
                published=new object();
                Interlocked.MemoryBarrier();
                int result=state;
                state=22;
                Thread.MemoryBarrier();
                Console.WriteLine(published!=null);
                return result+state;
            }
        }
        ""","True\n",42,typeof(Thread).Assembly);
}
