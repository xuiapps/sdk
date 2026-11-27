using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.XuiNet;
using Xunit;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.Tests.XuiNet.DotNet;

public class TypeReplacementTests
{
    [Theory]
    [InlineData(OptimizationLevel.Release)]
    [InlineData(OptimizationLevel.Debug)]
    public async Task ReferencesDefinitionsGenericsAndByrefsBindToTheSelectedImplementation(OptimizationLevel optimization)
    {
        byte[] originalImage = Frontend.Roslyn.Compile("""
            namespace Original;
            public class Counter {
                public int Value;
                public Counter(int value) { Value = -100; }
                public virtual int Read() => -100;
            }
            public class Cell<T> { public T Value; public T Read() => default; }
            public struct Awaiter<T> { public T Value; public T GetResult() => default; }
            public static class Bridge {
                public static Counter Create(int value) => new Counter(value);
                public static int Read(Counter value) => value.Read();
                public static void Set(Counter value, int number) { value.Value = number; }
                public static T Read<T>(Cell<T> cell) => cell.Read();
                public static T Read<T>(ref Awaiter<T> value) => value.GetResult();
            }
            """, OutputKind.DynamicallyLinkedLibrary, "Original", optimizationLevel: optimization);
        byte[] runtimeImage = Frontend.Roslyn.Compile("""
            namespace Browser;
            public class Counter {
                private int padding = 10;
                public int Value;
                public Counter(int value) { Value = value; }
                public virtual int Read() => Value + padding;
            }
            public class Cell<T> { public T Value; public T Read() => Value; }
            public struct Awaiter<T> { public int Padding; public T Value; public T GetResult() => Value; }
            """, OutputKind.DynamicallyLinkedLibrary, "BrowserRuntime", optimizationLevel: optimization);
        byte[] appImage = Frontend.Roslyn.Compile("""
            using Original;
            public class Derived : Counter { public Derived() : base(3) {} public override int Read() => Value + 1; }
            public static class Program {
                public static int Main() {
                    Counter c = Bridge.Create(5); Bridge.Set(c, 7);
                    var cell = new Cell<int> { Value = 9 };
                    var awaiter = new Awaiter<int> { Value = 12 };
                    return Bridge.Read(c) + Bridge.Read(cell) + Bridge.Read(ref awaiter) + Bridge.Read(new Derived());
                }
            }
            """, additionalReferences: [MetadataReference.CreateFromImage(originalImage)], optimizationLevel: optimization);
        using var original = CanvasTests.Read(originalImage);
        using var runtime = CanvasTests.Read(runtimeImage);
        using var app = CanvasTests.Read(appImage);
        var replacements = new[] { "Counter", "Cell`1", "Awaiter`1" }
            .Select(name => new TypeReplacement(Named(original, name), Named(runtime, name))).ToArray();
        var compilation = new Xui.Compiler.XuiNet.Compilation([app, original, runtime], replacements);
        foreach (var replacement in replacements)
            Assert.Equal(replacement.Implementation, compilation.ResolveType(replacement.Source.Module, replacement.Source.Handle));
        var emitter = new WasmEmitter();
        var result = await Node.ExecuteAsync(emitter.Emit(app));
        Assert.Equal(42, result.ExitCode);
        Assert.DoesNotContain(emitter.Dependencies!.Methods, method => replacements.Any(r => r.Source == method.DeclaringType.GenericDefinition));
    }

    [Fact]
    public void InvalidOrCyclicMappingsDoNotBindTheAssemblies()
    {
        using var assembly = CanvasTests.Read(Frontend.Roslyn.Compile("""
            public class A {} public class B {} public struct Value {} public class Generic<T> {}
            """, OutputKind.DynamicallyLinkedLibrary));
        var a = Named(assembly, "A"); var b = Named(assembly, "B");
        Assert.Throws<ArgumentException>(() => new Xui.Compiler.XuiNet.Compilation([assembly], [new(a, b), new(b, a)]));
        Assert.Null(assembly.ManifestModule.Compilation);
        Assert.Throws<ArgumentException>(() => new Xui.Compiler.XuiNet.Compilation([assembly], [new(a, Named(assembly, "Value"))]));
        Assert.Null(assembly.ManifestModule.Compilation);
        Assert.Throws<ArgumentException>(() => new Xui.Compiler.XuiNet.Compilation([assembly], [new(a, Named(assembly, "Generic`1"))]));
        Assert.Null(assembly.ManifestModule.Compilation);
    }

    [Fact]
    public void MissingReplacementMembersDoNotFallBackToTheOriginalBody()
    {
        using var original = CanvasTests.Read(Frontend.Roslyn.Compile("public class Original { public static int Missing() => 42; }", OutputKind.DynamicallyLinkedLibrary, "Original"));
        using var runtime = CanvasTests.Read(Frontend.Roslyn.Compile("public class Implementation {}", OutputKind.DynamicallyLinkedLibrary, "Runtime"));
        var type = Named(original, "Original");
        _ = new Xui.Compiler.XuiNet.Compilation([original, runtime], [new(type, Named(runtime, "Implementation"))]);
        var method = original.ManifestModule.Methods.Single(h => original.ManifestModule.GetMethod(h).Name == "Missing");
        Assert.Contains("Implementation.Missing", Assert.Throws<NotSupportedException>(() =>
            Model.MemberResolver.Method(original.ManifestModule, method, type)).Message);
    }

    internal static Model.TypeIdentity Named(Model.Assembly assembly, string name) =>
        new(assembly.ManifestModule, assembly.ManifestModule.Types.Single(h => assembly.ManifestModule.GetType(h).Name == name));
}
