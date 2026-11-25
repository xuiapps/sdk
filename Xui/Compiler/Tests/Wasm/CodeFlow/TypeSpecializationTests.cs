using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.CodeFlow;

[Collection(nameof(ManagedConsoleCollection))]
public class TypeSpecializationTests
{
    [Fact]
    public Task ClosedTypeQueriesNeedNoRuntimeReflection() => InlineArrayTests.AssertLinked("""
        public struct Value { public int X; }
        public class Reference { }
        public static class Program {
            static bool ValueType<T>() => typeof(T).IsValueType;
            public static int Main() => ValueType<int>() && ValueType<Value>() && ValueType<int?>() &&
                !ValueType<string>() && !ValueType<object>() && !ValueType<Reference>() && !ValueType<int[]>() ? 42 : 1;
        }
        """, "", 42);

    [Fact]
    public async Task ProvenFalseConjunctionDoesNotRetainUnsupportedCode()
    {
        const string source="""
            public static class Program {
                static int Unreachable() => System.Environment.ProcessorCount;
                static int Check<T>(bool flag) { if (!typeof(T).IsValueType & flag) return Unreachable(); return 42; }
                public static int Main() => Check<int>(true);
            }
            """;
        byte[] image=Frontend.Roslyn.Compile(source, optimizationLevel: OptimizationLevel.Release);
        using var app=CanvasTests.Read(image);
        using var framework=new Frontend.Framework();
        _=new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var emitter=new WasmEmitter();
        Assert.Equal(42, (await Node.ExecuteAsync(emitter.Emit(app))).ExitCode);
        Assert.DoesNotContain(emitter.Dependencies!.Methods, m=>m.DeclaringType.Module.GetMethod(m.Handle).Name=="Unreachable");
    }

    [Fact]
    public Task ReferenceSpecializationsPreserveTheOtherBranchAndSideEffects() => InlineArrayTests.AssertLinked("""
        public static class Program {
            static bool SideEffect(ref int count) { count++; return true; }
            static int Choose<T>(ref int count) {
                if (typeof(T).IsValueType && SideEffect(ref count)) return 40;
                return 2;
            }
            public static int Main() { int count=0; int answer=Choose<int>(ref count)+Choose<string>(ref count); return count==1 ? answer : 1; }
        }
        """, "", 42);

    [Fact]
    public Task SearchLoopsMayReturnOrThrowBeforeTheirContinuation() => GcTests.AssertExecution("""
        using System;
        public static class Program {
            static int Search(int wanted, bool first) {
                if(first) {
                    for(int i=0;i<10;i++) {
                        if(i==wanted) { if(i==3) return 21; if(i==5) throw new ArgumentException(); return 1; }
                    }
                } else {
                    for(int i=0;i<10;i++) { if(i==wanted) return 21; }
                }
                return 0;
            }
            public static int Main() {
                int sum=Search(3,true)+Search(7,false);
                try { Search(5,true); return 1; } catch(ArgumentException) { return Search(20,false)==0 ? sum : 1; }
            }
        }
        """, "", 42);
}
