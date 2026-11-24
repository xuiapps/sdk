using System.Reflection;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Structs;

public class StructAbiTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ValueArgumentsResultsAndAliasesHaveIndependentStorage(bool debug)
    {
        const string source = """
            public struct Point {
                public int X, Y;
                public Point(int x, int y) { X = x; Y = y; }
            }
            public interface ITransform { Point Apply(Point p); }
            public class Transform : ITransform {
                public virtual Point Apply(Point p) { p.X += 10; return p; }
            }
            public class Derived : Transform {
                public override Point Apply(Point p) { p.Y += 20; return base.Apply(p); }
            }
            public static class Program {
                static Point Replace(Point p) {
                    ref Point alias = ref p;
                    p = new Point(30, 40);
                    alias.X += 1;
                    return p;
                }
                static Point Recurse(Point p, int count) {
                    if (count == 0) return p;
                    Point next = new Point(p.X + 1, p.Y + 2);
                    Point result = Recurse(next, count - 1);
                    return new Point(result.X + p.X, result.Y + p.Y);
                }
                static Point Mutate(ref Point p) { p.X = 90; return p; }
                static int First(Point a, Point b) => a.X;
                public static int Main() {
                    Point p = new Point(1, 2);
                    Point replaced = Replace(p);
                    if (p.X != 1 | replaced.X != 31 | replaced.Y != 40) return 1;
                    ITransform t = new Derived();
                    Point transformed = t.Apply(p);
                    if (transformed.X != 11 | transformed.Y != 22 | p.X != 1) return 2;
                    Transform virtualTarget = new Derived();
                    if (virtualTarget.Apply(p).Y != 22) return 3;
                    Point recursive = Recurse(p, 4);
                    if (recursive.X != 15 | recursive.Y != 30) return 4;
                    if (First(p, Mutate(ref p)) != 1 | p.X != 90) return 5;
                    Point value = default;
                    for (int i = 0; i < 100000; i++) value = new Transform().Apply(value);
                    return value.X == 1000000 ? 42 : 6;
                }
            }
            """;
        await Execute(source, debug, 42);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReturnedReferenceSlotsSurviveCalleeCleanupAndUnwind(bool debug)
    {
        const string source = """
            public ref struct Event { public string Text; public int Count; }
            public static class Program {
                static void Keep(ref Event e) { }
                static Event Make(string text, int count) { Event e = default; Keep(ref e); e.Text = text; e.Count = count; return e; }
                static Event Change(Event e) { e.Text = "changed"; e.Count += 1; return e; }
                static Event Fail(Event e) { e.Text = "unwinding"; throw new System.InvalidOperationException(); }
                public static int Main() {
                    Event e = Make("original", 10);
                    Event copy = Change(e);
                    try { Fail(e); } catch (System.InvalidOperationException) { }
                    System.Console.WriteLine(e.Text);
                    System.Console.WriteLine(copy.Text);
                    for (int i = 0; i < 100000; i++) copy = Change(copy);
                    System.Console.WriteLine(copy.Text);
                    return e.Count + copy.Count;
                }
            }
            """;
        await Execute(source, debug, 100021, "original\nchanged\nchanged\n");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReadonlyStructParametersWorkThroughInterfaceAndOverride(bool debug)
    {
        const string source = """
            public struct Point { public int X, Y; }
            public interface IRead { int Read(in Point p); }
            public class Base : IRead { public virtual int Read(in Point p) => p.X; }
            public class Derived : Base { public override int Read(in Point p) => base.Read(in p) + p.Y; }
            public static class Program {
                public static int Main() {
                    Point p = default; p.X = 20; p.Y = 22;
                    IRead reader = new Derived(); return reader.Read(in p);
                }
            }
            """;
        var image = Frontend.Roslyn.Compile(source);
        using var assembly = Canvas.CanvasTests.Read(image);
        var module = assembly.ManifestModule;
        var contract = module.GetType(module.Types.Single(h => module.GetType(h).Name == "IRead"));
        var signature = Xui.Compiler.XuiNet.DotNet.Signatures.SignatureReader.Method(module, module.GetMethod(contract.Methods.Single()).Signature);
        Assert.True(signature.Parameters[0].IsReadOnlyByReference);
        Assert.True(signature.Parameters[0].IsByReference);
        await Execute(source, debug, 42);
    }

    private static async Task Execute(string source, bool debug, int expected, string? output = null)
    {
        var image = Frontend.Roslyn.Compile(source, optimizationLevel: debug ? OptimizationLevel.Debug : OptimizationLevel.Release);
        if (output is null) Assert.Equal(expected, (int)Assembly.Load(image).EntryPoint!.Invoke(null, null)!);
        using var assembly = Canvas.CanvasTests.Read(image);
        var result = await Node.ExecuteAsync(new WasmEmitter().Emit(assembly, new ConsoleWriteLineRewriter()));
        Assert.Equal(expected, result.ExitCode);
        if (output is not null) Assert.Equal(output, result.Output);
    }
}
