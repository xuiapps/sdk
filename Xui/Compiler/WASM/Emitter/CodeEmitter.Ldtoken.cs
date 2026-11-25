using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdtoken(Instruction.Token instruction, int offset)
    {
        if (method.StaticSpans.TryGetValue(offset, out var field))
        {
            writer.I32Const(method.Storage.StaticData.Address(field.Field));
            stack.Push(new(SignatureTypeCode.IntPtr) { FieldToken = field.Field });
            return;
        }
        var type = ManagedType.Token(source, MetadataTokens.EntityHandle(instruction.Value), method.Identity.DeclaringType.Arguments, method.Identity.Arguments);
        writer.GlobalGet(method.Storage.Classes.Resolve(type).Descriptor);
        stack.Push(new(SignatureTypeCode.Object) { TypeToken = type });
    }
}
