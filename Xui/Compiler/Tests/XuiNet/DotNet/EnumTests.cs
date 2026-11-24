using Microsoft.CodeAnalysis;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.XuiNet.Layout;
using Xunit;

namespace Xui.Compiler.Tests.XuiNet.DotNet;

public class EnumTests
{
    [Theory]
    [InlineData("sbyte", SignatureTypeCode.SByte, 1, StackType.Int32)]
    [InlineData("byte", SignatureTypeCode.Byte, 1, StackType.Int32)]
    [InlineData("short", SignatureTypeCode.Int16, 2, StackType.Int32)]
    [InlineData("ushort", SignatureTypeCode.UInt16, 2, StackType.Int32)]
    [InlineData("int", SignatureTypeCode.Int32, 4, StackType.Int32)]
    [InlineData("uint", SignatureTypeCode.UInt32, 4, StackType.Int32)]
    [InlineData("long", SignatureTypeCode.Int64, 8, StackType.Int64)]
    [InlineData("ulong", SignatureTypeCode.UInt64, 8, StackType.Int64)]
    public void EnumIdentityStorageAndStackCategoriesRemainDistinct(string underlying, SignatureTypeCode code, int width, StackType stack)
    {
        byte[] pe = Frontend.Roslyn.Compile($$"""
            public enum Value : {{underlying}} { None, Item }
            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
            public struct Packet { public byte Before; public Value Item; public byte After; }
            """, OutputKind.DynamicallyLinkedLibrary);
        using var stream = new MemoryStream(pe);
        using var assembly = AssemblyCompiler.Read(stream);
        var module = assembly.ManifestModule;
        var identity = new TypeIdentity(module, module.Types.Single(h => module.GetType(h).Name == "Value"));
        var packet = new TypeIdentity(module, module.Types.Single(h => module.GetType(h).Name == "Packet"));
        var item = module.GetField(module.GetType((TypeDefinitionHandle)packet.Handle).Fields.Single(h => module.GetField(h).Name == "Item"));
        var signature = SignatureReader.Field(module, item.Signature);
        Assert.Equal(identity, signature.Identity);
        Assert.Equal(SignatureTypeCode.TypeHandle, signature.Code);
        Assert.Equal(code, EnumInfo.UnderlyingType(identity));
        Assert.Null(EnumInfo.UnderlyingType(packet));
        Assert.Equal(new SignatureType(code), EnumInfo.Storage(signature));
        Assert.Equal(stack, StackEntry.From(signature).Type);
        Assert.Equal(signature.ByReference(), StackEntry.From(signature.ByReference()).DeclaredType);
        foreach (var target in new[] { TargetLayout.Wasm32, TargetLayout.X64 })
        {
            var layouts = new LayoutResolver(module, target);
            Assert.Equal(width, layouts.Resolve(identity).Size);
            Assert.Equal(width, layouts.Resolve(identity).Alignment);
            var layout = layouts.Resolve(packet);
            Assert.Equal(width + 2, layout.Size);
            Assert.Equal(1, layout.Alignment);
            Assert.Equal(new FieldLayout(1, width), layout.Fields[item.Handle]);
            var last = module.GetType((TypeDefinitionHandle)packet.Handle).Fields.Single(h => module.GetField(h).Name == "After");
            Assert.Equal(new FieldLayout(width + 1, 1), layout.Fields[last]);
        }
    }
}
