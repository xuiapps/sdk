using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.XuiNet.Layout;
using Xunit;

namespace Xui.Compiler.Tests.XuiNet.Layout;

public class InlineArrayLayoutTests
{
    [Fact]
    public void InlinePackingMatchesManagedHostOffsets()
    {
        byte[] image = Frontend.Roslyn.Compile("""
            [System.Runtime.CompilerServices.InlineArray(3)]
            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack=1)]
            public struct Packed { private long first; }
            public struct Holder { public byte Head; public Packed Values; public byte Tail; }
            """, OutputKind.DynamicallyLinkedLibrary);
        using var app = CanvasTests.Read(image);
        var module = app.ManifestModule;
        var holder = module.Types.Single(h => module.GetType(h).Name == "Holder");
        var layout = new LayoutResolver(module, TargetLayout.Wasm32).Resolve(holder);
        var managed = System.Reflection.Assembly.Load(image).GetType("Holder")!;
        int size = (int)typeof(System.Runtime.CompilerServices.Unsafe).GetMethod("SizeOf")!.MakeGenericMethod(managed).Invoke(null,null)!;
        Assert.Equal(size, layout.Size);
        foreach (var (field, storage) in layout.Fields)
            Assert.Equal(System.Runtime.InteropServices.Marshal.OffsetOf(managed, module.GetField(field).Name).ToInt32(), storage.Offset);
    }

    [Fact]
    public void ElementsRepeatWithTheirOwnStrideAndAlignment()
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile("""
            [System.Runtime.CompilerServices.InlineArray(3)] public struct Buffer<T> { private T first; }
            public struct Point { public short X; public int Y; }
            public struct Nested { public byte Head; public Buffer<Point> Values; public byte Tail; }
            public class Holder { public Buffer<int> I; public Buffer<long> L; public Buffer<string> S; public Nested N; }
            """, OutputKind.DynamicallyLinkedLibrary));
        var module = app.ManifestModule;
        var holder = module.GetType(module.Types.Single(h => module.GetType(h).Name == "Holder"));
        var types = holder.Fields.ToDictionary(h => module.GetField(h).Name,
            h => SignatureReader.Field(module, module.GetField(h).Signature).Identity!.Value);
        var resolver = new LayoutResolver(module, TargetLayout.Wasm32);
        var ints = resolver.Resolve(types["I"]); var longs = resolver.Resolve(types["L"]); var nested = resolver.Resolve(types["N"]);
        Assert.Equal((12,4), (ints.Size,ints.Alignment));
        Assert.Equal((24,8), (longs.Size,longs.Alignment));
        Assert.Equal((32,4), (nested.Size,nested.Alignment));
        Assert.Equal(new[] {0,4,28}, nested.Fields.Values.Select(f => f.Offset));
        Assert.Equal(new FieldLayout(0,4), Assert.Single(ints.Fields).Value);
        var info = InlineArrayInfo.Read(types["L"])!.Value;
        Assert.Equal(3,info.Length); Assert.Equal(SignatureTypeCode.Int64,info.Element.Code);
        Assert.Equal(StorageKind.ContainsManagedReferences, resolver.Classify(types["S"]));
        Assert.Throws<NotSupportedException>(() => resolver.Resolve(types["S"]));
    }
}
