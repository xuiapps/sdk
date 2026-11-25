using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryResourceString(Instruction.Token instruction)
    {
        if (ResourceString.Read(source, MetadataTokens.EntityHandle(instruction.Value)) is not { } text) return false;
        EmitString(text);
        stack.Push(new(SignatureTypeCode.String));
        return true;
    }
}
