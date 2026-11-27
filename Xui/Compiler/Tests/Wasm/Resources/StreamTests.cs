using Microsoft.CodeAnalysis;
using Xui.Compiler.Playground;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.Tests.Wasm.Gc;
using Xui.Compiler.Tests.Wasm.Repl;
using Xui.Compiler.Tests.Wasm.Structs;
using Xui.Compiler.WASM.Emitter;
using Xui.Compiler.WASM.Runtime;
using Xunit;

namespace Xui.Compiler.Tests.Wasm.Resources;

[Collection(nameof(ManagedConsoleCollection))]
public class StreamTests
{
    [Theory]
    [InlineData(OptimizationLevel.Debug)]
    [InlineData(OptimizationLevel.Release)]
    public async Task ReadAtLeastHandlesShortReadsEofAndZeroWithoutReading(OptimizationLevel optimization)
    {
        byte[] image = Frontend.Roslyn.Compile("""
            using System;
            using System.IO;
            public sealed class Chunks : Stream {
                int position;
                public int Calls;
                public override bool CanRead => true;
                public override bool CanSeek => false;
                public override bool CanWrite => false;
                public override long Length => 5;
                public override long Position { get => position; set => throw new NotSupportedException(); }
                public override int Read(Span<byte> buffer) {
                    Calls++;
                    int count = Math.Min(2, Math.Min(5 - position, buffer.Length));
                    for (int i = 0; i < count; i++) buffer[i] = (byte)++position;
                    return count;
                }
                public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
                public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
                public override void Flush() { }
                public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
                public override void SetLength(long value) => throw new NotSupportedException();
            }
            public static class Program {
                public static int Main() {
                    var buffer = new byte[8];
                    using var stream = new Chunks();
                    Console.WriteLine(stream.ReadAtLeast(buffer, 0)); Console.WriteLine(stream.Calls);
                    Console.WriteLine(stream.ReadAtLeast(buffer, 3)); Console.WriteLine(stream.Calls);
                    foreach (byte value in buffer) Console.WriteLine((int)value);
                    Console.WriteLine(stream.ReadAtLeast(buffer, 4, false)); Console.WriteLine(stream.Calls);
                    Console.WriteLine(stream.ReadAtLeast(buffer, 1, false));
                    try { stream.ReadAtLeast(buffer, 1); } catch (EndOfStreamException) { Console.WriteLine("eof"); }
                    try { stream.ReadAtLeast(buffer, -1); } catch (ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                    try { stream.ReadAtLeast(buffer, 9, false); } catch (ArgumentOutOfRangeException e) { Console.WriteLine(e.ParamName); }
                    using var exact = new Chunks();
                    exact.ReadExactly(buffer.AsSpan(1, 5));
                    foreach (byte value in buffer) Console.WriteLine((int)value);
                    return 42;
                }
            }
            """, optimizationLevel: optimization);
        var expected = GcTests.RunManaged(image);
        using var app = CanvasTests.Read(image);
        using var runtime = CanvasTests.Read(File.ReadAllBytes(typeof(Xui.Compiler.Runtime.IO.Stream).Assembly.Location));
        using var framework = new Frontend.Framework("System.IO.Compression.dll", "System.Memory.dll");
        _ = new Xui.Compiler.XuiNet.Compilation([app, runtime, .. framework.Assemblies],
            BrowserResources.Replacements(framework.Assemblies[0], framework.Assemblies.Single(a => a.Identity.Name == "System.IO.Compression"), runtime));
        Assert.Equal(expected, await Node.ExecuteAsync(new WasmEmitter().Emit(app, new ConsoleWriteLineRewriter())));
    }
}
