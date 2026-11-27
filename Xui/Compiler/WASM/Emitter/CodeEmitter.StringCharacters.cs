using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryStringCharacters(Instruction.Token instruction, int offset)
    {
        if (StringCharacters.Read(source, MetadataTokens.EntityHandle(instruction.Value)) is not { } signature) return false;
        bool slice = signature.Parameters.Length == 3;
        var shape = method.Storage.ArrayShape(signature.Parameters[0].Array!);
        if (slice)
        {
            Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.StringCount);
            Pop(SignatureTypeCode.Int32, instruction.Code, offset); writer.LocalSet(method.StringIndex);
        }
        else writer.I32Const(0).LocalSet(method.StringIndex);
        PopAssignable(method.Storage.Bind(signature.Parameters[0]), instruction.Code, offset);
        writer.LocalSet(method.GcSource);
        if (slice)
        {
            writer.LocalGet(method.GcSource).RefIsNull().If(); ThrowArgumentException(ExceptionKind.ArgumentNullException, "value"); writer.End();
            writer.LocalGet(method.StringIndex).I32Const(0).I32LtS().If(); ThrowArgumentException(ExceptionKind.ArgumentOutOfRangeException, "startIndex"); writer.End();
            writer.LocalGet(method.StringCount).I32Const(0).I32LtS().If(); ThrowArgumentException(ExceptionKind.ArgumentOutOfRangeException, "length"); writer.End();
            writer.LocalGet(method.StringIndex).LocalGet(method.GcSource).RefCast(shape.Type).ArrayLen().I32GtU().If();
            ThrowArgumentException(ExceptionKind.ArgumentOutOfRangeException, "startIndex"); writer.End();
            writer.LocalGet(method.StringCount).LocalGet(method.GcSource).RefCast(shape.Type).ArrayLen().LocalGet(method.StringIndex).I32Sub().I32GtU().If();
            ThrowArgumentException(ExceptionKind.ArgumentOutOfRangeException, "startIndex"); writer.End();
        }
        else
            writer.LocalGet(method.GcSource).RefIsNull().If().I32Const(0).LocalSet(method.StringCount)
                .Else().LocalGet(method.GcSource).RefCast(shape.Type).ArrayLen().LocalSet(method.StringCount).End();
        writer.LocalGet(method.StringCount).ArrayNewDefault(Runtime.Utf16ArrayType).LocalSet(method.GcOwner);
        writer.I32Const(0).LocalSet(method.GcI32).Block(method.Storage.EmptyBlock).Loop();
        writer.LocalGet(method.GcI32).LocalGet(method.StringCount).I32LtU().I32Eqz().BrIf(new(1));
        writer.LocalGet(method.GcOwner).RefCast(Runtime.Utf16ArrayType).LocalGet(method.GcI32)
            .LocalGet(method.GcSource).RefCast(shape.Type).LocalGet(method.StringIndex).LocalGet(method.GcI32).I32Add().ArrayGet(shape.Type)
            .ArraySet(Runtime.Utf16ArrayType);
        writer.LocalGet(method.GcI32).I32Const(1).I32Add().LocalSet(method.GcI32).Br(new(0)).End().End();
        writer.LocalGet(method.GcOwner).RefCast(Runtime.Utf16ArrayType).I32Const(0).LocalGet(method.StringCount).Call(Runtime.FromCharCodeArrayFunction);
        stack.Push(new(SignatureTypeCode.String));
        return true;
    }
}
