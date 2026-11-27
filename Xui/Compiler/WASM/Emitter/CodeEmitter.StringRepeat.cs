using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryStringRepeat(Instruction.Token instruction, int offset)
    {
        if (!StringRepeat.Is(source, MetadataTokens.EntityHandle(instruction.Value))) return false;
        Pop(SignatureTypeCode.Int32, instruction.Code, offset);
        writer.LocalSet(method.GcI32);
        Pop(SignatureTypeCode.Int32, instruction.Code, offset);
        writer.LocalGet(method.GcI32).I32Const(0).I32LtS().If();
        ThrowArgumentException(ExceptionKind.ArgumentOutOfRangeException, "count"); writer.End();
        // Packed i16 storage preserves individual UTF-16 units, including unpaired surrogates.
        writer.LocalGet(method.GcI32).ArrayNew(Runtime.Utf16ArrayType).I32Const(0).LocalGet(method.GcI32)
            .Call(Runtime.FromCharCodeArrayFunction);
        stack.Push(new(SignatureTypeCode.String));
        return true;
    }
}
