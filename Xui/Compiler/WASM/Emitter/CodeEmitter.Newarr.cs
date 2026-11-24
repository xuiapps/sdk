using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Op;
using System.Reflection.Metadata;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitNewarr(Instruction.Token instruction, int offset)
    {
        PopArrayIndex(instruction.Code, offset);
        var shape = method.Storage.ArrayShape(new(ArrayElement(instruction.Value)));
        writer.LocalTee(method.ArrayIndex).I32Const(0).I32LtS().If();
        ThrowException(ExceptionKind.OverflowException);
        writer.End().LocalGet(method.ArrayIndex).ArrayNewDefault(shape.Type);
        if (shape.StructElements)
        {
            writer.LocalSet(method.ArrayReference).I32Const(0).LocalSet(method.ArrayIndex);
            writer.Block(method.Storage.EmptyBlock).Loop(method.Storage.EmptyBlock);
            writer.LocalGet(method.ArrayIndex).LocalGet(method.ArrayReference).RefCast(shape.Type).ArrayLen().I32LtU().I32Eqz().If().Br(new(2)).End();
            writer.LocalGet(method.ArrayReference).RefCast(shape.Type).LocalGet(method.ArrayIndex)
                .StructNewDefault(method.Storage.Shape(shape.Element.Identity!.Value).Type).ArraySet(shape.Type);
            writer.LocalGet(method.ArrayIndex).I32Const(1).I32Add().LocalSet(method.ArrayIndex).Br(new(0));
            writer.End().End().LocalGet(method.ArrayReference);
        }
        stack.Push(new(SignatureTypeCode.SZArray, array: shape.Signature));
    }
}
