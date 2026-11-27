using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryStringConcatenation(Instruction.Token instruction, int offset)
    {
        var access = StringConcatenation.Read(source, MetadataTokens.EntityHandle(instruction.Value));
        if (access is null) return false;
        int count = access.Signature.Parameters.Length;
        for (int i = count - 1; i >= 0; i--)
        {
            var argument = method.StringConcatArguments[i];
            if (access.Spans)
            {
                PopAssignable(method.Storage.Bind(access.Signature.Parameters[i]), instruction.Code, offset);
                EmitCharacterSpanString();
            }
            Pop(SignatureTypeCode.String, instruction.Code, offset);
            writer.LocalSet(argument).LocalGet(argument).RefIsNull().If();
            EmitString("");
            writer.LocalSet(argument).End();
        }
        writer.LocalGet(method.StringConcatArguments[0]);
        for (int i = 1; i < count; i++)
            writer.LocalGet(method.StringConcatArguments[i]).Call(Runtime.StringConcat!.Value);
        stack.Push(new(SignatureTypeCode.String));
        return true;
    }
}
