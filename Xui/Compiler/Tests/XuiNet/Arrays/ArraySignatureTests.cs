using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using Xui.Compiler.Tests.Wasm.Canvas;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xunit;

namespace Xui.Compiler.Tests.XuiNet.Arrays;

public class ArraySignatureTests
{
    [Fact]
    public void VectorsRetainExactNestedElementsAndGenericSubstitution()
    {
        using var app = CanvasTests.Read(Frontend.Roslyn.Compile("""
            public struct Cell { public int Value; }
            public class Buffer<T> { public T[][] Items; }
            public class Holder { public int[] Integers; public string[] Strings; public Cell[] Cells; public Buffer<Cell> Buffer; }
            """, OutputKind.DynamicallyLinkedLibrary));
        var module = app.ManifestModule;
        var holder = module.GetType(module.Types.Single(h => module.GetType(h).Name == "Holder"));
        var types = holder.Fields.ToDictionary(h => module.GetField(h).Name, h => SignatureReader.Field(module, module.GetField(h).Signature));
        Assert.Equal(SignatureType.Vector(new(SignatureTypeCode.Int32)), types["Integers"]);
        Assert.Equal("String[]", types["Strings"].ToString());
        Assert.NotEqual(types["Integers"], types["Strings"]);
        var buffer = types["Buffer"].Identity!.Value;
        var field = module.GetType((TypeDefinitionHandle)buffer.Handle).Fields.Single();
        var open = SignatureReader.Field(module, module.GetField(field).Signature);
        var closed = SignatureReader.Field(module, module.GetField(field).Signature, buffer.Arguments);
        Assert.True(open.ContainsGenericParameters);
        Assert.False(closed.ContainsGenericParameters);
        Assert.Equal(types["Cells"], closed.Array!.Element);
        Assert.Equal("Cell[][]", closed.ToString());
        Assert.Equal(new GenericArguments([closed]), new GenericArguments([SignatureType.Vector(types["Cells"])]));
    }
}
