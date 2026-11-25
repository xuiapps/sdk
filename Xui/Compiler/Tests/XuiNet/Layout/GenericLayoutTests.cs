using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.XuiNet.Layout;
using Xunit;

namespace Xui.Compiler.Tests.XuiNet.Layout;

public class GenericLayoutTests
{
    [Fact]
    public void SpecializationsHaveIndependentLayoutsAndClassification()
    {
        var image = Frontend.Roslyn.Compile("""
            public struct Pair<T> { public byte Head; public T Value; public byte Tail; }
            public struct Nested<T> { public Pair<T> First; public T Last; }
            public class Holder {
                public Pair<int> I;
                public Pair<long> L;
                public Pair<string> S;
                public Pair<string[]> A;
                public Nested<short> N;
            }
            """, OutputKind.DynamicallyLinkedLibrary);
        using var assembly = Wasm.Canvas.CanvasTests.Read(image);
        var module = assembly.ManifestModule;
        var holder = module.GetType(module.Types.Single(h => module.GetType(h).Name == "Holder"));
        var fields = holder.Fields.ToDictionary(h => module.GetField(h).Name,
            h => SignatureReader.Field(module, module.GetField(h).Signature).Identity!.Value);
        var resolver = new LayoutResolver(module, TargetLayout.Wasm32);
        var ints = resolver.Resolve(fields["I"]);
        var longs = resolver.Resolve(fields["L"]);
        var nested = resolver.Resolve(fields["N"]);
        Assert.Equal((12, 4), (ints.Size, ints.Alignment));
        Assert.Equal((24, 8), (longs.Size, longs.Alignment));
        Assert.Equal((8, 2), (nested.Size, nested.Alignment));
        Assert.Equal(new[] { 0, 4, 8 }, ints.Fields.Values.Select(f => f.Offset));
        Assert.Equal(new[] { 0, 8, 16 }, longs.Fields.Values.Select(f => f.Offset));
        Assert.Same(ints, resolver.Resolve(fields["I"]));
        Assert.Equal(StorageKind.LinearMemory, resolver.Classify(fields["I"]));
        Assert.Equal(StorageKind.ContainsManagedReferences, resolver.Classify(fields["S"]));
        Assert.Equal(StorageKind.ContainsManagedReferences, resolver.Classify(fields["A"]));
        Assert.Throws<NotSupportedException>(() => resolver.Resolve(fields["S"]));
        Assert.Same(longs, resolver.Resolve(fields["L"]));
    }

    [Fact]
    public void FrameworkTupleForwardingResolvesOneIdentityAndTargetLayout()
    {
        var image = Frontend.Roslyn.Compile("public class Holder { public (int, double) Pair; }", OutputKind.DynamicallyLinkedLibrary);
        using var assembly = Wasm.Canvas.CanvasTests.Read(image);
        using var framework = new Frontend.Framework();
        var compilation = new Xui.Compiler.XuiNet.Compilation([assembly, .. framework.Assemblies]);
        var module = assembly.ManifestModule;
        var holder = module.GetType(module.Types.Single(h => module.GetType(h).Name == "Holder"));
        var tuple = SignatureReader.Field(module, module.GetField(holder.Fields.Single()).Signature).Identity!.Value;
        Assert.Equal("System.Private.CoreLib.dll", tuple.Module.Name);
        Assert.True(tuple.IsValueType);
        var layout = new LayoutResolver(module, TargetLayout.Wasm32).Resolve(tuple);
        Assert.Equal((16, 8), (layout.Size, layout.Alignment));
        Assert.Equal(new[] { 0, 8 }, layout.Fields.Values.Select(f => f.Offset));
        // Resolve the facade's exported type using a real Core TypeRef to the same framework definition.
        using var core = Wasm.Canvas.CanvasTests.Read(File.ReadAllBytes(typeof(Xui.Core.Math2D.Point).Assembly.Location));
        using var framework2 = new Frontend.Framework();
        var context = new Xui.Compiler.XuiNet.Compilation([core, .. framework2.Assemblies]);
        var coreModule = core.ManifestModule;
        var reference = coreModule.TypeReferences.Single(h => coreModule.GetTypeReference(h).Name == "ValueTuple`2");
        var resolved = context.ResolveType(coreModule, reference);
        Assert.Equal("System.Private.CoreLib.dll", resolved.Module.Name);
        Assert.Equal(HandleKind.TypeDefinition, resolved.Handle.Kind);
        Assert.Same(resolved.Module, framework2.Assemblies[0].ManifestModule);
        var nativeRef = coreModule.TypeReferences.Single(h => coreModule.GetTypeReference(h).Name == "NFloat");
        var native = context.ResolveType(coreModule, nativeRef);
        var nativeLayout = new LayoutResolver(coreModule, TargetLayout.Wasm32).Resolve(native);
        Assert.Equal(4, nativeLayout.Size);
        Assert.Equal(new FieldLayout(0, 4), Assert.Single(nativeLayout.Fields).Value);
    }
}
