using Xui.Compiler.Tests.Wasm.InlineArrays;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.Tests.Wasm.Structs;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Delegates;

[Collection(nameof(ManagedConsoleCollection))]
public class FrameworkDelegateTests
{
    [Fact]
    public Task FrameworkActionAndFuncUseClosedSignaturesAndGenericTargets() => InlineArrayTests.AssertLinked("""
        public struct Vector { public double X; public double Y; }
        public class Counter {
            public double Total;
            public void Add(Vector value) { Total += value.X + value.Y; }
        }
        public static class Program {
            static T Identity<T>(T value) => value;
            public static int Main() {
                var counter = new Counter();
                var add = new System.Action<Vector>(counter.Add);
                var identity = new System.Func<int, int>(Identity<int>);
                add(new Vector { X = 1.5, Y = 2.5 });
                System.Console.WriteLine(counter.Total);
                return identity(42);
            }
        }
        """, "4\n", 42);

    [Fact]
    public Task CapturedFrameworkCallbacksEscapeTheirCreatingFrame() => InlineArrayTests.AssertLinked("""
        public static class Program {
            static System.Func<int, int> Create(int start) => amount => start += amount;
            public static int Main() {
                var first = Create(3); var second = Create(10);
                System.Console.WriteLine(first(2));
                return first(4) + second(1);
            }
        }
        """, "5\n", 20);
    [Fact]
    public async Task ActualXuiVectorCrossesTheFrameworkActionBoundary()
    {
        byte[] coreImage = Frontend.CoreCanvasProfile.Image;
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] image = Frontend.Roslyn.Compile("""
                using Xui.Core.Math2D;
                public class Listener {
                    public Vector Value;
                    public void Move(Vector delta) { Value = delta; }
                }
                public static class Program {
                    public static int Main() {
                        var listener = new Listener();
                        var callback = new System.Action<Vector>(listener.Move);
                        callback(new Vector(3, 4));
                        if (listener.Value.X == 3 && listener.Value.Y == 4) return 42;
                        return 0;
                    }
                }
                """, additionalReferences: [MetadataReference.CreateFromImage(coreImage),
                    MetadataReference.CreateFromFile(typeof(System.Runtime.InteropServices.NFloat).Assembly.Location)], optimizationLevel: optimization);
            using var core = CanvasTests.Read(coreImage);
            using var app = CanvasTests.Read(image);
            using var framework = new Frontend.Framework();
            _ = new Xui.Compiler.XuiNet.Compilation([app, core, .. framework.Assemblies]);
            Assert.Equal(42, (await Node.ExecuteAsync(new WasmEmitter().Emit(app))).ExitCode);
        }
    }

    [Fact]
    public void MulticastDelegatesFailExplicitly()
    {
        byte[] image = Frontend.Roslyn.Compile("""
            public static class Program {
                static void A() { }
                static void B() { }
                public static void Main() { var callback = new System.Action(A) + new System.Action(B); callback(); }
            }
            """);
        using var app = CanvasTests.Read(image);
        using var framework = new Frontend.Framework();
        _ = new Xui.Compiler.XuiNet.Compilation([app, .. framework.Assemblies]);
        Assert.Contains("multicast", Assert.Throws<NotSupportedException>(() => new WasmEmitter().Emit(app)).Message);
    }

    [Fact]
    public Task StaticObjectFactoryDelegateReturnsFreshRootObjects() => InlineArrayTests.AssertLinked("""
        public static class Program {
            static object Create() => new object();
            public static int Main() {
                var factory = new System.Func<object>(Create);
                object first = factory(); object second = factory();
                System.Console.WriteLine(first == second);
                return first != null && second != null ? 1 : 0;
            }
        }
        """, "False\n", 1);

}
