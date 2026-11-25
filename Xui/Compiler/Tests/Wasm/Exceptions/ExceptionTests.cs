using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Exceptions;

[Collection(nameof(ManagedConsoleCollection))]
public class ExceptionTests
{
    [Fact]
    public Task SharedExceptionTailsDefineTheirBlockTypesOnce() => GcTests.AssertExecution("""
        public static class Program {
            static int Run(bool first, bool second) {
                int value=0;
                if(first && second) value=1;
                else {
                    try { if(first) value=2; else value=3; }
                    finally { if(second) { for(int i=0;i<2;i++) value+=5; } }
                }
                return value>10 ? value : value+100;
            }
            public static int Main() => Run(true,true)+Run(true,false)+Run(false,true)+Run(false,false);
        }
        """, "", 319);

    [Fact]
    public async Task FailedCastIsCaughtAndFinallyRuns()
    {
        string source = File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Exceptions", "Casts.cs"));
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            var pe = Frontend.Roslyn.Compile(source, optimizationLevel: optimization);
            using var stream = new MemoryStream(pe);
            using var assembly = AssemblyCompiler.Read(stream);
            byte[] wasm = new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter());
            var result = await Node.ExecuteAsync(wasm);
            // CLR cast messages include runtime type names; the compiler uses the default exception message.
            Assert.Equal("Specified cast is not valid.\nfinished\n-1\nfinished\n", result.Output);
            Assert.Equal(42, result.ExitCode);
            if (optimization == OptimizationLevel.Debug) continue;
            string wat = await WasmTools.ToWatAsync(wasm);
            Snapshot.AssertMatches("Casts", $"# Catching failed interface casts\n\nSource: [Casts.cs](Fixtures/Exceptions/Casts.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{result.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{result.ExitCode}\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
        }
    }

    [Fact]
    public Task ThrowAcrossCallAndOrderedCatches() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static void Fail() { throw new InvalidOperationException("boom"); }
            public static int Main() {
                try { Fail(); return 0; }
                catch (InvalidCastException) { return 1; }
                catch (SystemException error) { Console.WriteLine(error.Message); return 42; }
                catch (Exception) { return 2; }
            }
        }
        """, "boom\n", 42);

    [Fact]
    public Task RethrowPreservesTheExceptionObject() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static void Again(Exception original) {
                try { throw original; }
                catch (Exception error) { Console.WriteLine(error == original); throw; }
            }
            public static int Main() {
                Exception original = new InvalidOperationException("again");
                try { Again(original); return 0; }
                catch (Exception error) { Console.WriteLine(error == original); Console.WriteLine(error.Message); return 42; }
            }
        }
        """, "True\nTrue\nagain\n", 42);

    [Fact]
    public Task FinallyRunsOnceWhenItThrows() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static void Fail() { try { throw new ArgumentException("first"); } finally { Console.WriteLine("cleanup"); throw new InvalidOperationException("second"); } }
            public static int Main() { try { Fail(); return 0; } catch (InvalidOperationException error) { Console.WriteLine(error.Message); return 42; } }
        }
        """, "cleanup\nsecond\n", 42);

    [Fact]
    public Task ThrowNullBecomesNullReferenceException() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            public static int Main() { try { throw null; } catch (NullReferenceException) { return 42; } }
        }
        """, "", 42);

    [Fact]
    public Task FinallyRunsOnNormalLeaveAndReturn() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static int Run() { try { Console.WriteLine("try"); return 42; } finally { Console.WriteLine("finally"); } }
            public static int Main() => Run();
        }
        """, "try\nfinally\n", 42);

    [Fact]
    public Task NullInterfaceCallsAreCatchable() => GcTests.AssertExecution("""
        using System;
        public interface I { int Read(); }
        public static class Program {
            static int Read(I value) => value.Read();
            public static int Main() { try { return Read(null); } catch (NullReferenceException) { return 42; } }
        }
        """, "", 42);

    [Fact]
    public void FiltersFailExplicitly()
    {
        var pe = Frontend.Roslyn.Compile("using System; public static class Program { public static int Main() { try { throw new Exception(); } catch(Exception e) when(e.Message != null) { return 0; } } }");
        Assert.Contains("filters", Assert.Throws<NotSupportedException>(() => GcTests.Emit(pe)).Message);
    }

    [Fact]
    public Task CatchAllHandlesManagedExceptions() => GcTests.AssertExecution("""
        using System;
        public static class Program { public static int Main() { try { throw new Exception("caught"); } catch { return 42; } } }
        """, "", 42);

    [Fact]
    public Task UnwindingRestoresLinearMemoryFrames() => GcTests.AssertExecution("""
        using System;
        public struct Pair { public int X; public int Y; }
        public static class Program {
            static void Fail(int value) { Pair p = default; p.X = value; p.Y = 1; if (p.X >= 0) throw new Exception("again"); }
            public static int Main() {
                int caught = 0;
                for (int i = 0; i < 80000; i++) { try { Fail(i); } catch (Exception) { caught++; } }
                return caught;
            }
        }
        """, "", 80000);

    [Fact]
    public Task ExceptionTypeTestsUseNominalHierarchy() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            public static int Main() {
                object error = new InvalidOperationException("value");
                Console.WriteLine(error is Exception);
                Console.WriteLine(error is SystemException);
                Console.WriteLine(error is InvalidCastException);
                Console.WriteLine((error as ArgumentException) == null);
                Console.WriteLine(((Exception)error).Message);
                return 42;
            }
        }
        """, "True\nTrue\nFalse\nTrue\nvalue\n", 42);

    [Fact]
    public Task NullFieldAccessIsCatchable() => GcTests.AssertExecution("""
        using System;
        public class Box { public int Value; }
        public static class Program {
            static void Store(Box box) { box.Value = 10; }
            public static int Main() { try { Store(null); return 0; } catch (NullReferenceException) { return 42; } }
        }
        """, "", 42);

    [Fact]
    public Task NestedCatchDoesNotReplaceOuterRethrow() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static void Fail() {
                try { throw new ArgumentException("outer"); }
                catch (Exception) {
                    try { throw new InvalidOperationException("inner"); }
                    catch (Exception error) { Console.WriteLine(error.Message); }
                    throw;
                }
            }
            public static int Main() { try { Fail(); return 0; } catch (ArgumentException error) { Console.WriteLine(error.Message); return 42; } }
        }
        """, "inner\nouter\n", 42);

    [Fact]
    public Task ThrowingFinallyReplacesNormalReturnOnce() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static int Run() { try { return 1; } finally { Console.WriteLine("cleanup"); throw new Exception("failed"); } }
            public static int Main() { try { return Run(); } catch (Exception error) { Console.WriteLine(error.Message); return 42; } }
        }
        """, "cleanup\nfailed\n", 42);

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 1)]
    public async Task FaultRunsOnlyDuringUnwinding(bool throws, int expected)
    {
        var pe = Frontend.Msil.Compile(il =>
        {
            var start = il.DefineLabel(); var fault = il.DefineLabel();
            var afterFault = il.DefineLabel(); var caught = il.DefineLabel(); var end = il.DefineLabel();
            il.MarkLabel(start);
            if (throws) { il.OpCode(System.Reflection.Metadata.ILOpCode.Ldnull); il.OpCode(System.Reflection.Metadata.ILOpCode.Throw); }
            else il.Branch(System.Reflection.Metadata.ILOpCode.Leave, afterFault);
            il.MarkLabel(fault);
            il.LoadConstantI4(1); il.StoreLocal(0); il.OpCode(System.Reflection.Metadata.ILOpCode.Endfinally);
            il.MarkLabel(afterFault);
            il.Branch(System.Reflection.Metadata.ILOpCode.Leave, end);
            il.MarkLabel(caught);
            il.OpCode(System.Reflection.Metadata.ILOpCode.Pop); il.Branch(System.Reflection.Metadata.ILOpCode.Leave, end);
            il.MarkLabel(end);
            il.LoadLocal(0); il.OpCode(System.Reflection.Metadata.ILOpCode.Ret);
            il.ControlFlowBuilder!.AddFaultRegion(start, fault, fault, afterFault);
            il.ControlFlowBuilder.AddCatchRegion(start, caught, caught, end,
                System.Reflection.Metadata.Ecma335.MetadataTokens.TypeReferenceHandle(1));
        }, [System.Reflection.Metadata.SignatureTypeCode.Int32]);
        Assert.Equal(expected, GcTests.RunManaged(pe).ExitCode);
        Assert.Equal(expected, (await Node.ExecuteAsync(GcTests.Emit(pe))).ExitCode);
    }

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
