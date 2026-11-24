using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.WASM.Emitter;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Generics;

public class LinkedMethodTests
{
    [Fact]
    public async Task ClosedMethodArgumentsResolveAcrossAssembliesAndNestedTypeSpecifications()
    {
        foreach (var optimization in new[] { OptimizationLevel.Release, OptimizationLevel.Debug })
        {
            byte[] library = Frontend.Roslyn.Compile("""
                public class Cell<T> { public T Value; }
                public static class Library {
                    public static U Identity<T, U>(Cell<T> cell, U value) => value;
                    public static void Replace<T>(ref T field, T value) { field = value; }
                    public static Cell<T> Make<T>(T value) { return new Cell<T> { Value = value }; }
                }
                """, OutputKind.DynamicallyLinkedLibrary, "GenericLibrary", optimizationLevel: optimization);
            byte[] program = Frontend.Roslyn.Compile("""
                public static class Program {
                    static Cell<T> Forward<T>(T value) => Library.Make<T>(value);
                    static int Recurse<T>(T value, int count) => count == 0 ? 3 : Recurse<T>(value, count - 1) + 1;
                    public static int Main() {
                        var holder = new Cell<Cell<int>>();
                        Library.Replace(ref holder.Value, Forward<int>(9));
                        return Library.Identity<int, int>(holder.Value, holder.Value.Value) + Recurse<string>("x", 4);
                    }
                }
                """, additionalReferences: [MetadataReference.CreateFromImage(library)], optimizationLevel: optimization);
            using var dependency = CanvasTests.Read(library);
            using var app = CanvasTests.Read(program);
            _ = new Xui.Compiler.XuiNet.Compilation(app, dependency);
            var emitter = new WasmEmitter();
            var result = await Node.ExecuteAsync(emitter.Emit(app));
            Assert.Equal(16, result.ExitCode);
            Assert.Equal(1, emitter.Dependencies!.Methods.Count(m => m.DeclaringType.Module.GetMethod(m.Handle).Name == "Recurse"));
        }
    }
}
