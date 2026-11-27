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
    public void NullableBoxNullTestsAreNotFoldedAsNonNull()
    {
        using var app=CanvasTests.Read(Frontend.Roslyn.Compile("""
            public static class Program {
                public static int Check(int? value) { if ((object)value==null) return 1; return 2; }
                public static int Main() => Check(null);
            }
            """));
        using var framework=new Frontend.Framework();
        _=new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var module=app.ManifestModule;
        var handle=module.Methods.Single(h=>module.GetMethod(h).Name=="Check");
        var error=Assert.Throws<NotSupportedException>(()=>Xui.Compiler.XuiNet.CodeFlow.Analyzer.Analyze(module,handle));
        Assert.Contains("Boxing/unboxing",error.Message);
        Assert.Contains("Nullable",error.Message);
    }

    [Fact]
    public Task NonNullableBoxTestsPreserveEvaluationAndReferenceNulls() => InlineArrayTests.AssertLinked("""
        public struct Value { public string Text; public int Number; }
        public static class Program {
            static int calls;
            static T Read<T>(T value) { calls++; return value; }
            static int Check<T>(T value) {
                if (Read(value)==null) return 1;
                if (Read(value)!=null) return 20;
                return 0;
            }
            public static int Main() {
                int result=Check(new Value { Text="kept", Number=42 })+Check("text")+Check<string>(null);
                return result==41 && calls==5 ? 42 : 1;
            }
        }
        """, "", 42);

    [Fact]
    public Task ImpossibleTypeTestPreservesReceiverEvaluation() => InlineArrayTests.AssertLinked("""
        interface IKey { }
        class Key : IKey { }
        class Other { }
        class Holder { public IKey Key; }
        public static class Program {
            static int calls;
            static Holder Read() { calls++; return new Holder { Key=new Key() }; }
            public static int Main() {
                if (Read().Key is Other) return System.Environment.ProcessorCount;
                return calls==1 ? 42 : 1;
            }
        }
        """, "", 42);

    [Fact]
    public Task TypeTestAllowsInterfaceIntroducedBySubclass() => InlineArrayTests.AssertLinked("""
        interface IKey { }
        class Base { }
        class Derived : Base, IKey { }
        class Holder { public IKey Key; }
        public static class Program {
            public static int Main() {
                var holder=new Holder { Key=new Derived() };
                return holder.Key is Base ? 42 : 1;
            }
        }
        """, "", 42);

    [Fact]
    public Task ConstantBooleanCallsKeepTrueFalseAndMutableParametersDistinct() => InlineArrayTests.AssertLinked("""
        static class Choice<T> {
            public static int Read(bool enabled) => enabled ? 40 : 2;
            public static int Assign(bool enabled) { enabled=!enabled; return enabled ? 40 : 2; }
            static void Flip(ref bool enabled) { enabled=!enabled; }
            public static int Address(bool enabled) { Flip(ref enabled); return enabled ? 40 : 2; }
        }
        public static class Program {
            public static int Main() {
                int first=Choice<string>.Read(true)+Choice<string>.Read(false);
                int second=Choice<string>.Assign(true)+Choice<string>.Assign(false);
                int third=Choice<string>.Address(true)+Choice<string>.Address(false);
                return first==42 && second==42 && third==42 ? first : 1;
            }
        }
        """, "", 42);

    [Fact]
    public Task ClosedTypeEqualityPreservesExactManagedIdentity() => InlineArrayTests.AssertLinked("""
        enum Number : int { Zero }
        class Key {}
        public static class Program {
            static bool Same<T,U>() => typeof(T)==typeof(U);
            static int Choose<T>() { if(typeof(T)==typeof(string)) return System.Environment.ProcessorCount; return 42; }
            public static int Main() => Same<int,int>() && !Same<int,uint>() && !Same<Number,int>() &&
                Same<int[],int[]>() && !Same<Key,string>() && Same<Key,Key>() ? Choose<Key>() : 1;
        }
        """, "", 42);

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
