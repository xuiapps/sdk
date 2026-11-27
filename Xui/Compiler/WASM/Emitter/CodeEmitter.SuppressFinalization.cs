using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Exceptions;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TrySuppressFinalization(Instruction.Token instruction, int offset)
    {
        if (!SuppressFinalization.Is(source, MetadataTokens.EntityHandle(instruction.Value))) return false;
        Pop(instruction.Code, offset);
        writer.RefIsNull().If();
        ThrowArgumentException(ExceptionKind.ArgumentNullException, "obj");
        writer.End();
        return true;
    }
}
