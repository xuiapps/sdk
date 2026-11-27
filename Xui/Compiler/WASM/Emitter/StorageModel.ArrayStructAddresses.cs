using System.Reflection.Metadata;
using BodyWriter = Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody.Writer;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    // Array-backed spans address flattened leaves across stable element objects.
    private void EmitArrayStructAccessor(BodyWriter writer, ArrayShape array, LocalVariable value, bool store)
    {
        var shape = Shape(array.Element.Identity!.Value);
        for (int i = 0; i < shape.Leaves.Count; i++)
        {
            var leaf = shape.Leaves[i].Storage;
            if (IsReference(value) ? !IsReference(leaf) || (leaf.TypeCode == SignatureTypeCode.String) != (value.TypeCode == SignatureTypeCode.String)
                : leaf.TypeCode != value.TypeCode) continue;
            writer.LocalGet(new(0)).ExternConvertAny().I32Const((int)array.Type.Value).Call(ArrayIdentity.Test)
                .LocalGet(new(1)).I32Const(shape.Leaves.Count).I32RemU().I32Const(i).I32Eq().I32And().If();
            writer.LocalGet(new(0)).RefCast(array.Type).LocalGet(new(1)).I32Const(shape.Leaves.Count).I32DivU()
                .ArrayGet(array.Type).RefCast(shape.Type);
            if (store) writer.LocalGet(new(2)).StructSet(shape.Type, new((uint)i));
            else writer.StructGet(shape.Type, new((uint)i));
            writer.Return().End();
        }
    }
}
