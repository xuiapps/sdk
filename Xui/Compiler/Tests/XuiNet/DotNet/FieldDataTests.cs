using System.Reflection;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xunit;

namespace Xui.Compiler.Tests.XuiNet.DotNet;

public class FieldDataTests
{
    [Fact]
    public void FieldInitialDataBorrowsBoundedBytesFromItsPe()
    {
        var image = Frontend.Roslyn.Compile("""
            using System;
            public static class Program {
                public static int Counter;
                public static ReadOnlySpan<int> Values => [2, 3, 5, 7];
                public static int Main() => Values[0];
            }
            """);
        using var assembly = CanvasTests.Read(image);
        var module = assembly.ManifestModule;
        var handle = module.Fields.Single(h => (module.GetField(h).Attributes & FieldAttributes.HasFieldRVA) != 0);
        var field = module.GetField(handle);
        Assert.True(field.RelativeVirtualAddress > 0);
        var reader = field.InitialData(16);
        Assert.Equal(2, reader.ReadInt32()); Assert.Equal(3, reader.ReadInt32());
        Assert.Equal(5, reader.ReadInt32()); Assert.Equal(7, reader.ReadInt32());
        Assert.Equal(0, reader.RemainingBytes);
        Assert.Throws<ArgumentOutOfRangeException>(() => module.GetField(handle).InitialData(-1));
        var counter = module.Fields.Single(h => module.GetField(h).Name == "Counter");
        Assert.Equal(0, module.GetField(counter).RelativeVirtualAddress);
        Assert.Throws<InvalidOperationException>(() => module.GetField(counter).InitialData(4));
        assembly.Dispose();
        Assert.Throws<ObjectDisposedException>(() => module.GetField(handle).InitialData(4));
    }
}
