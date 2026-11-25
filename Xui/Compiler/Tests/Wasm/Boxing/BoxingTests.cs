using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.InlineArrays;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Boxing;

[Collection(nameof(ManagedConsoleCollection))]
public class BoxingTests
{
    [Fact]
    public async Task PrimitiveBoxingReplSnapshot()
    {
        string directory=SourceDirectory();
        string source=File.ReadAllText(Path.Combine(directory, "..", "..", "Fixtures", "Boxing", "Primitives.cs"));
        byte[] image=Frontend.Roslyn.Compile(source);
        var managed=GcTests.RunManaged(image);
        Assert.Equal("Int32\nTrue\n42\nTrue\nWrong type caught\n", managed.Output);
        Assert.Equal(42, managed.ExitCode);
        using var app=CanvasTests.Read(image);
        using var framework=new Frontend.Framework();
        _=new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        byte[] wasm=new WasmEmitter().Emit(app, new Xui.Compiler.Playground.ConsoleWriteLineRewriter());
        Assert.Equal(managed, await Node.ExecuteAsync(wasm));
        string wat=await WasmTools.ToWatAsync(wasm);
        Snapshot.AssertMatches("Primitives", $"# Primitive boxing and unboxing\n\nSource: [Primitives.cs](Fixtures/Boxing/Primitives.cs)\n\n## Source\n\n```csharp\n{source.TrimEnd()}\n```\n\n## Output\n\n```text\n{managed.Output.TrimEnd()}\n```\n\n## Exit Code\n\n```text\n{managed.ExitCode}\n```\n\n## WAT\n\n```wat\n{wat.TrimEnd()}\n```\n");
    }

    private static string SourceDirectory([System.Runtime.CompilerServices.CallerFilePath] string file="") => Path.GetDirectoryName(file)!;

    [Theory]
    [InlineData("bool", "true")]
    [InlineData("char", "'\u03bb'")]
    [InlineData("sbyte", "-127")]
    [InlineData("byte", "255")]
    [InlineData("short", "-32767")]
    [InlineData("ushort", "65535")]
    [InlineData("int", "-1234567")]
    [InlineData("uint", "4000000000u")]
    [InlineData("long", "-9007199254740993L")]
    [InlineData("ulong", "18014398509481987UL")]
    [InlineData("float", "1.25f")]
    [InlineData("double", "1.25")]
    [InlineData("nint", "-42")]
    [InlineData("nuint", "42")]
    public Task PrimitiveRoundTripThroughGenericObject(string type, string value) => InlineArrayTests.AssertLinked($$"""
        public static class Program {
            static object Box<T>(T value) => value;
            static T Unbox<T>(object value) => (T)value;
            public static int Main() {
                {{type}} original = {{value}};
                object box = Box(original);
                {{type}} copy = Unbox<{{type}}>(box);
                return copy == original && box is {{type}} && box.GetType() == typeof({{type}}) ? 42 : 1;
            }
        }
        """, "", 42);

    [Fact]
    public Task BoxesCopyValuesAndHaveIndependentIdentity() => InlineArrayTests.AssertLinked("""
        public static class Program {
            static object Box(int value) => value;
            public static int Main() {
                int value=21; object a=Box(value); object alias=a; object b=Box(value);
                value=99; int copy=(int)a; copy=0;
                return a==alias && a!=b && (int)a+(int)b==42 ? 42 : 1;
            }
        }
        """, "", 42);

    [Fact]
    public Task WrongTypesAndNullThrowManagedExceptions() => InlineArrayTests.AssertLinked("""
        using System;
        public class Other { }
        public static class Program {
            static int Read(object value) => (int)value;
            public static int Main() {
                int count=0;
                try { Read(null); } catch(NullReferenceException) { count++; }
                try { Read(1u); } catch(InvalidCastException) { count++; }
                try { Read(1L); } catch(InvalidCastException) { count++; }
                try { Read(1f); } catch(InvalidCastException) { count++; }
                try { Read(new Other()); } catch(InvalidCastException) { count++; }
                try { Read(true); } catch(InvalidCastException) { count++; }
                return count==6 ? 42 : 1;
            }
        }
        """, "", 42);

    [Fact]
    public Task EnumsKeepNominalIdentityButAllowUnderlyingUnboxing() => InlineArrayTests.AssertLinked("""
        using System;
        enum A : byte { Answer=42 }
        enum B : byte { Answer=42 }
        enum C : int { Answer=42 }
        public static class Program {
            public static int Main() {
                object a=A.Answer, b=(byte)42;
                Console.WriteLine(a.GetType().Name);
                Console.WriteLine(a is A); Console.WriteLine(a is B); Console.WriteLine(a is byte);
                if((byte)a!=42 || (B)a!=B.Answer || (A)b!=A.Answer) return 1;
                try { C c=(C)a; return 2; } catch(InvalidCastException) { return 42; }
            }
        }
        """, "A\nTrue\nFalse\nFalse\n", 42);

    [Fact]
    public Task ReferenceGenericBoxAndUnboxPreserveIdentityAndNull() => InlineArrayTests.AssertLinked("""
        public class Item { public int Value; }
        public static class Program {
            static object Box<T>(T value) => value;
            static T Unbox<T>(object value) => (T)value;
            public static int Main() {
                Item item=new Item { Value=42 };
                return Unbox<Item>(Box(item))==item && Unbox<Item>(null)==null ? item.Value : 1;
            }
        }
        """, "", 42);

    [Fact]
    public Task BoxesParticipateInValueTypeEnumAndInterfaceTests() => InlineArrayTests.AssertLinked("""
        using System;
        enum Flag : int { On=1 }
        public static class Program {
            static bool Matches(object value) => value is IEquatable<int>;
            public static int Main() {
                object integer=42, flag=Flag.On;
                Console.WriteLine(integer is ValueType); Console.WriteLine(integer is Enum);
                Console.WriteLine(flag is ValueType); Console.WriteLine(flag is Enum);
                Console.WriteLine(Matches(integer)); Console.WriteLine(Matches(flag)); Console.WriteLine(Matches(null));
                return 42;
            }
        }
        """, "True\nFalse\nTrue\nTrue\nTrue\nFalse\nFalse\n", 42);

    [Fact]
    public Task ObjectGenericUnboxingIsIdentity() => InlineArrayTests.AssertLinked("""
        public static class Program {
            static T Read<T>(object value) => (T)value;
            static bool Is<T>(object value) => value is T;
            public static int Main() {
                object value=42;
                return Read<object>(value)==value && Read<object>(null)==null && Is<object>(value) && !Is<object>(null) ? 42 : 1;
            }
        }
        """, "", 42);

    [Theory]
    [InlineData("double.NaN", "double", "copy != copy")]
    [InlineData("float.NaN", "float", "copy != copy")]
    [InlineData("-0.0", "double", "1.0 / copy == double.NegativeInfinity")]
    [InlineData("-0.0f", "float", "1.0f / copy == float.NegativeInfinity")]
    public Task FloatingBoxesPreserveSpecialValues(string value, string type, string check) => InlineArrayTests.AssertLinked($$"""
        public static class Program { public static int Main() {
            object box={{value}}; {{type}} copy=({{type}})box;
            return {{check}} ? 42 : 1;
        } }
        """, "", 42);

    [Fact]
    public Task NativeFloatKeepsItsOwnBoxIdentity() => InlineArrayTests.AssertLinked("""
        using System.Runtime.InteropServices;
        public static class Program { public static int Main() {
            object box=(NFloat)1.25;
            return (NFloat)box==(NFloat)1.25 && box is NFloat && !(box is float) ? 42 : 1;
        } }
        """, "", 42, typeof(System.Runtime.InteropServices.NFloat).Assembly);

    [Theory]
    [InlineData("struct Value { public int X; }", "object value=new Value(); return Escape(value)==null ? 1 : 42;", "primitives, enums, and NFloat only")]
    [InlineData("", "int? x=42; object value=x; return Escape(value)==null ? 1 : 42;", "primitives, enums, and NFloat only")]
    [InlineData("", "object value=42; return ((System.IEquatable<int>)value).Equals(42) ? 42 : 1;", "unboxing adapter")]
    public void UnsupportedBoxesAndBoxedDispatchFailDuringCompilation(string declarations, string body, string diagnostic)
    {
        byte[] image=Frontend.Roslyn.Compile($"{declarations} public static class Program {{ static object Escape(object value)=>value; public static int Main() {{ {body} return 0; }} }}");
        using var app=CanvasTests.Read(image);
        using var framework=new Frontend.Framework();
        _=new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        var error=Assert.Throws<NotSupportedException>(()=>new WasmEmitter().Emit(app));
        Assert.Contains(diagnostic, error.ToString());
    }

    [Fact]
    public async Task UnboxAddressMutatesTheExistingBox()
    {
        TypeReferenceHandle integer = default;
        byte[] image = Frontend.Msil.Compile(il =>
        {
            il.LoadConstantI4(10); il.OpCode(ILOpCode.Box); il.Token(integer); il.StoreLocal(0);
            il.LoadLocal(0); il.StoreLocal(1);
            il.LoadLocal(0); il.OpCode(ILOpCode.Unbox); il.Token(integer);
            il.LoadConstantI4(42); il.OpCode(ILOpCode.Stind_i4);
            il.LoadLocal(1); il.OpCode(ILOpCode.Unbox_any); il.Token(integer); il.OpCode(ILOpCode.Ret);
        }, [SignatureTypeCode.Object, SignatureTypeCode.Object], configureMetadata: metadata =>
            integer = metadata.AddTypeReference(MetadataTokens.AssemblyReferenceHandle(1), metadata.GetOrAddString("System"), metadata.GetOrAddString("Int32")));
        var managed = GcTests.RunManaged(image);
        Assert.Equal(42, managed.ExitCode);
        using var app = CanvasTests.Read(image);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        Assert.Equal(managed, await Node.ExecuteAsync(new WasmEmitter().Emit(app)));
    }
}
