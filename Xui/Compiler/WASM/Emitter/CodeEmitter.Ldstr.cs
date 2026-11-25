using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Op;
using IR = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdstr(Instruction.Token instruction)
    {
        EmitString(GetString(source, instruction.Value));
        stack.Push(new MsilStackEntry(SignatureTypeCode.String));
    }

    private void EmitString(string literal)
    {
        var (data, global) = literals[literal];
        writer.GlobalGet(global)
            .RefIsNull()
            .If();
        writer.I32Const(0) // data byte offset
            .I32Const(literal.Length)
            .ArrayNewData(Runtime.Utf16ArrayType, data);
        writer.I32Const(0) // first UTF-16 code unit
            .I32Const(literal.Length)
            .Call(Runtime.FromCharCodeArrayFunction)
            .GlobalSet(global)
            .End();
        writer.GlobalGet(global);
    }

    private static string GetString(IR.Module module, int token) =>
        module.GetString(MetadataTokens.UserStringHandle(token & 0xffffff));
}
