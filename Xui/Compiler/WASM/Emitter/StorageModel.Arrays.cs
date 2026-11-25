using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    private readonly Dictionary<ArraySignature, ArrayShape> arrays = new();
    private readonly HashSet<ArrayShape> addressedArrays = new();

    public void RequireArrayAddress(ArraySignature signature)
    {
        var shape = ArrayShape(signature);
        if (!shape.StructElements) addressedArrays.Add(shape);
        RequireAddress(shape.Element);
    }

    public ArrayShape ArrayShape(ArraySignature signature)
    {
        if (arrays.TryGetValue(signature, out var shape)) return shape;
        var element = Bind(signature.Element);
        if (element.IsByReference) throw new NotSupportedException("Managed arrays cannot contain byrefs.");
        shape = new(signature, element);
        arrays.Add(signature, shape);
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
            shape.Type = writer.ArrayType(shape.StructElements ? ValueTypeCode.EqRef : shape.Element.WasmType);
    }
}
