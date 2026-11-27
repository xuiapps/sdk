using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryTextElement(Instruction.Token instruction, int offset)
    {
        var access = TextElement.Read(source, MetadataTokens.EntityHandle(instruction.Value));
        if (access is null) return false;
        PopAssignable(method.Storage.Bind(access.Signature.Parameters[0]), instruction.Code, offset);
        EmitCharacterSpanString();
        Pop(SignatureTypeCode.String, instruction.Code, offset);
        writer.Call(method.Storage.StringAccess.TextElement);
        stack.Push(new(SignatureTypeCode.Int32));
        return true;
    }
}
