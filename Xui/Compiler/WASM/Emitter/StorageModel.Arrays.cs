using Mutability = Xui.Compiler.WASM.Wasm.Section.Mutability;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    private readonly Dictionary<ArraySignature, ArrayShape> arrays = new();
    private readonly HashSet<ArrayShape> addressedArrays = new();

    public void RequireArrayAddress(ArraySignature signature)
    {
        if (!signature.IsVector) throw new NotSupportedException("Rectangular arrays require projected element addresses.");
        var shape = ArrayShape(signature);
        addressedArrays.Add(shape);
        // Struct addresses project through array elements; distinguish them from ordinary eqref arrays.
        if (shape.StructElements) ArrayIdentity.Enabled = true;
        RequireAddress(shape.Element);
    }

    public ArrayShape ArrayShape(ArraySignature signature)
    {
        if (arrays.TryGetValue(signature, out var shape)) return shape;
        var element = Bind(signature.Element);
        if (element.IsByReference) throw new NotSupportedException("Managed arrays cannot contain byrefs.");
        shape = new(signature, element);
        arrays.Add(signature, shape);
        if (!signature.IsVector) shape.Backing = ArrayShape(new(signature.Element));
        return shape;
    }

    // An explicitly preserved method may use array elements without any newarr in
    // the module. Register these shapes before assigning type indices. Reference
    // fields can discover further array signatures, so drain the finite closure.
    public void CollectArrayElementShapes()
    {
        int visited = 0;
        while (visited < arrays.Count)
        {
            var pending = arrays.Values.Skip(visited).ToArray();
            visited += pending.Length;
            foreach (var shape in pending)
                if (shape.Element.Identity is { } identity) _ = Shape(identity);
        }
    }

    private void DefineArrayTypes(ref Writer writer)
    {
        foreach (var shape in arrays.Values)
            shape.Type = shape.Signature.IsVector ? writer.ArrayType(shape.StructElements ? ValueTypeCode.EqRef : shape.Element.WasmType)
                : writer.StructType([.. Enumerable.Repeat((Wasm.Section.TypeDefinitions.ValueType)ValueTypeCode.I32, shape.Signature.Rank), ValueTypeCode.EqRef], Mutability.Immutable);
    }
}
