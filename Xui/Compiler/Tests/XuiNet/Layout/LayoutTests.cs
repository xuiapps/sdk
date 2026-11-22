using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.Layout;
using Xunit;

namespace Xui.Compiler.Tests.XuiNet.Layout;

public class LayoutTests
{
    [Theory]
    [InlineData("Padded", 24, 8, "0:1,8:8,16:2")]
    [InlineData("Packed", 8, 2, "0:1,2:4,6:2")]
    [InlineData("Nested", 12, 2, "0:1,2:8,10:2")]
    [InlineData("Union", 16, 4, "0:4,0:4,12:2")]
    [InlineData("Sized", 11, 4, "0:4")]
    [InlineData("MinimumSize", 4, 4, "0:4")]
    [InlineData("Empty", 1, 1, "")]
    [InlineData("Kind", 2, 2, "0:2")]
    [InlineData("SmallFields", 6, 2, "0:1,2:2,4:2")]
    [InlineData("NativeFields", 8, 4, "0:4,4:4")]
    [InlineData("WithStatic", 4, 4, "0:4")]
    public void Wasm32LayoutHonorsFieldStorageAndMetadata(string name, int size, int alignment, string offsetsAndSizes)
    {
        using var stream = new MemoryStream(CompileFixture("Layouts"));
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        var handle = module.Types.Single(h => module.GetType(h).Name == name);
        var resolver = new LayoutResolver(module, TargetLayout.Wasm32);
        var layout = resolver.Resolve(handle);
        Assert.Equal(size, layout.Size);
        Assert.Equal(alignment, layout.Alignment);
        Assert.Equal(offsetsAndSizes, string.Join(",", module.GetType(handle).Fields
            .Where(h => layout.Fields.ContainsKey(h)).Select(h => $"{layout.Fields[h].Offset}:{layout.Fields[h].Size}")));
        Assert.Same(layout, resolver.Resolve(handle));
    }

    [Theory]
    [InlineData("Point")]
    [InlineData("Vector")]
    public void NativeFloatLayoutUsesTheTargetInsteadOfTheHost(string name)
    {
        using var stream = new MemoryStream(CompileFixture("PointVector"));
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        var handle = module.Types.Single(h => module.GetType(h).Name == name);
        var wasm32 = new LayoutResolver(module, TargetLayout.Wasm32).Resolve(handle);
        var wide = new LayoutResolver(module, TargetLayout.X64).Resolve(handle);
        Assert.Equal(8, wasm32.Size);
        Assert.Equal(4, wasm32.Alignment);
        Assert.Equal(16, wide.Size);
        Assert.Equal(8, wide.Alignment);
        var y = module.GetType(handle).Fields.Single(h => module.GetField(h).Name == "Y");
        Assert.Equal(new FieldLayout(4, 4), wasm32.Fields[y]);
        Assert.Equal(new FieldLayout(8, 8), wide.Fields[y]);
        Assert.Equal(-1, module.GetField(y).DeclaredOffset); // Sequential offset is calculated, not stored in PE.
    }

    [Fact]
    public void MatchingScalarLayoutsAgreeWithClrSizesAndBlittableOffsets()
    {
        byte[] image = CompileFixture("Layouts");
        var managed = Assembly.Load(image);
        using var stream = new MemoryStream(image);
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        var resolver = new LayoutResolver(module, IntPtr.Size == 8 ? TargetLayout.X64 : TargetLayout.Wasm32);
        foreach (var name in new[] { "Padded", "Packed", "Nested", "Union", "Sized", "MinimumSize", "Empty", "SmallFields", "NativeFields" })
        {
            var managedType = managed.GetType("Samples.Layout." + name)!;
            int managedSize = (int)typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf))!.MakeGenericMethod(managedType).Invoke(null, null)!;
            var handle = module.Types.Single(h => module.GetType(h).Name == name);
            var layout = resolver.Resolve(handle);
            Assert.Equal(managedSize, layout.Size);
            // Marshal.OffsetOf is an unmanaged-layout API. bool/char marshalling can differ from managed storage.
            if (name == "SmallFields") continue;
            foreach (var (field, storage) in layout.Fields)
                Assert.Equal(Marshal.OffsetOf(managedType, module.GetField(field).Name).ToInt32(), storage.Offset);
        }
    }

    [Fact]
    public void ExplicitOffsetsAndPackingRemainAvailableAsMetadataViews()
    {
        using var stream = new MemoryStream(CompileFixture("Layouts"));
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        var packed = module.Types.Single(h => module.GetType(h).Name == "Packed");
        Assert.Equal(2, module.GetType(packed).DeclaredLayout.PackingSize);
        var union = module.Types.Single(h => module.GetType(h).Name == "Union");
        Assert.Equal(16, module.GetType(union).DeclaredLayout.Size);
        var tail = module.GetType(union).Fields.Single(h => module.GetField(h).Name == "Tail");
        Assert.Equal(12, module.GetField(tail).DeclaredOffset);
    }

    [Theory]
    [InlineData("public struct Sample { public string Text; }", "Field storage type String")]
    [InlineData("public class Sample { public int Value; }", "Only unboxed value-type")]
    [InlineData("[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)] public struct Sample { public int Value; }", "Auto layout")]
    [InlineData("public struct Sample<T> { public T Value; }", "Generic layout")]
    [InlineData("[System.Runtime.CompilerServices.InlineArray(4)] public struct Sample { private int element; }", "Inline-array layout")]
    [InlineData("public struct Sample { public System.DateTime Value; }", "External value-type")]
    [InlineData("public struct Sample { public object Value; }", "Field storage type Object")]
    public void UnsupportedLayoutsFailWithoutGuessing(string source, string reason)
    {
        using var stream = new MemoryStream(Frontend.Roslyn.Compile(source, OutputKind.DynamicallyLinkedLibrary));
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        var handle = module.Types.Single(h => module.GetType(h).Name.StartsWith("Sample", StringComparison.Ordinal));
        var resolver = new LayoutResolver(module, TargetLayout.Wasm32);
        Assert.Contains(reason, Assert.Throws<NotSupportedException>(() => resolver.Resolve(handle)).Message);
        // A failed calculation must not leave an entry marked as recursively resolving.
        Assert.Contains(reason, Assert.Throws<NotSupportedException>(() => resolver.Resolve(handle)).Message);
    }

    private static byte[] CompileFixture(string fixture) => Frontend.Roslyn.Compile(
        File.ReadAllText(Path.Combine(SourceDirectory(), "..", "..", "Fixtures", "Structs", fixture + ".cs")),
        OutputKind.DynamicallyLinkedLibrary, "Layout." + fixture);

    private static string SourceDirectory([CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;
}
