using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryArrayClear(Instruction.Token instruction, int offset)
    {
        var clear = ArrayClear.Read(source, MetadataTokens.EntityHandle(instruction.Value));
        if (clear is null) return false;
        if (clear.HasIndices)
        {
            Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.CopyLength);
            Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.CopyDestinationIndex);
        }
        var array = Pop(instruction.Code, offset); writer.LocalSet(method.GcOwner);
        CheckCopyNull(method.GcOwner, "array");
        if (array.Array is not { } signature) { writer.Unreachable(); return true; }
        var shape = method.Storage.ArrayShape(signature);
        writer.LocalGet(method.GcOwner).RefCast(shape.Type).ArrayLen().LocalSet(method.GcI32);
        if (clear.HasIndices)
        {
            // Unsigned comparisons also reject negative indices/lengths, without overflow.
            writer.LocalGet(method.CopyDestinationIndex).LocalGet(method.GcI32).I32GtU().If();
            ThrowException(ExceptionKind.IndexOutOfRangeException); writer.End();
            writer.LocalGet(method.CopyLength).LocalGet(method.GcI32).LocalGet(method.CopyDestinationIndex).I32Sub().I32GtU().If();
            ThrowException(ExceptionKind.IndexOutOfRangeException); writer.End();
        }
        else
            writer.I32Const(0).LocalSet(method.CopyDestinationIndex).LocalGet(method.GcI32).LocalSet(method.CopyLength);

        writer.Block(method.Storage.EmptyBlock).Loop().LocalGet(method.CopyLength).I32Eqz().BrIf(new(1));
        if (shape.StructElements)
        {
            // Keep element holders stable so outstanding ldelema references observe the reset.
            var element = method.Storage.Shape(shape.Element.Identity!.Value);
            for (int i = 0; i < element.Leaves.Count; i++)
            {
                writer.LocalGet(method.GcOwner).RefCast(shape.Type).LocalGet(method.CopyDestinationIndex).ArrayGet(shape.Type).RefCast(element.Type);
                DefaultValue(element.Leaves[i].Storage);
                writer.StructSet(element.Type, new((uint)i));
            }
        }
        else
        {
            writer.LocalGet(method.GcOwner).RefCast(shape.Type).LocalGet(method.CopyDestinationIndex);
            DefaultValue(shape.Element); writer.ArraySet(shape.Type);
        }
        writer.LocalGet(method.CopyDestinationIndex).I32Const(1).I32Add().LocalSet(method.CopyDestinationIndex);
        writer.LocalGet(method.CopyLength).I32Const(1).I32Sub().LocalSet(method.CopyLength).Br(new(0)).End().End();
        return true;
    }
}
