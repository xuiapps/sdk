using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Threading;

[Collection(nameof(ManagedConsoleCollection))]
public class MonitorTests
{
    [Fact]
    public Task ReentrantLocksAndFinallyReleaseMatchClr() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Threading;
        public static class Program {
            public static int Main() {
                object gate = new object(), other = new int[2];
                Console.WriteLine(Monitor.IsEntered(gate));
                lock (gate) {
                    Console.WriteLine(Monitor.IsEntered(gate));
                    lock (gate) { Console.WriteLine(Monitor.IsEntered(gate)); }
                    Console.WriteLine(Monitor.IsEntered(gate));
                    Console.WriteLine(Monitor.IsEntered(other));
                    lock (other) { Console.WriteLine(Monitor.IsEntered(other)); }
                }
                Console.WriteLine(Monitor.IsEntered(gate));
                try { lock (gate) { throw new InvalidOperationException(); } }
                catch (InvalidOperationException) { Console.WriteLine(Monitor.IsEntered(gate)); }
                return 42;
            }
        }
        """, "False\nTrue\nTrue\nTrue\nFalse\nTrue\nFalse\nFalse\n", 42);

    [Fact]
    public Task FlagsAliasingAndInvalidUsageMatchClr() => InlineArrayTests.AssertLinked("""
        using System;
        using System.Threading;
        public class Holder { public bool Taken; }
        public static class Program {
            public static int Main() {
                object gate = new object(); var holder = new Holder(); var flags = new bool[1];
                Monitor.Enter(gate, ref holder.Taken);
                Monitor.Enter(gate, ref flags[0]);
                Console.WriteLine(holder.Taken); Console.WriteLine(flags[0]);
                Monitor.Exit(gate); Console.WriteLine(Monitor.IsEntered(gate)); Monitor.Exit(gate);
                try { Monitor.Exit(gate); }
                catch (SynchronizationLockException e) { Console.WriteLine(e.Message); }
                bool taken = true;
                try { Monitor.Enter(null, ref taken); }
                catch (ArgumentException e) { Console.WriteLine(e.Message); }
                taken = false;
                try { Monitor.Enter(null, ref taken); }
                catch (ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                Console.WriteLine(taken);
                try { Monitor.Exit(null); }
                catch (ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                try { Monitor.IsEntered(null); }
                catch (ArgumentNullException e) { Console.WriteLine(e.ParamName); }
                Monitor.Enter(gate); Console.WriteLine(Monitor.IsEntered(gate)); Monitor.Exit(gate);
                return 42;
            }
        }
        """, "True\nTrue\nTrue\nObject synchronization method was called from an unsynchronized block of code.\nArgument must be initialized to false (Parameter 'lockTaken')\n\nFalse\n\nobj\nTrue\n", 42);

    [Fact]
    public void DirectNativeStringLocksAreExplicitlyUnsupported()
    {
        byte[] pe = Frontend.Roslyn.Compile("""
            public static class Program {
                public static void Main() { System.Threading.Monitor.Enter("key"); }
            }
            """);
        Assert.Contains("managed string identity", Assert.Throws<NotSupportedException>(() => GcTests.Emit(pe)).Message);
    }

    [Fact]
    public async Task ObjectTypedNativeStringCarriersFailBeforeEntering()
    {
        using var app = Canvas.CanvasTests.Read(Frontend.Roslyn.Compile("""
            using System;
            using System.Threading;
            public static class Program {
                static void Acquire(object value) => Monitor.Enter(value);
                public static int Main() {
                    int result = 1;
                    try { Acquire("key"); }
                    catch (NotSupportedException) { result = 42; }
                    return result;
                }
            }
            """));
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        byte[] wasm = new Xui.Compiler.WASM.Emitter.WasmEmitter().Emit(app);
        var result = await Repl.Node.ExecuteAsync(wasm);
        Assert.Equal(42, result.ExitCode);
    }
}
