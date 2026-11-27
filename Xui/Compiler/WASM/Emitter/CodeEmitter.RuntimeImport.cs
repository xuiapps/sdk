using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryRuntimeImport(Instruction.Token instruction, int offset)
    {
        if (RuntimeImport.Read(source, MetadataTokens.EntityHandle(instruction.Value)) is not { } import) return false;
        for (int i = import.Signature.Parameters.Length - 1; i >= 0; i--)
            PopAssignable(method.Storage.Bind(import.Signature.Parameters[i]), instruction.Code, offset);
        writer.Call(method.Storage.HostImports.Functions[import.Method]);
        if (import.Signature.Result.Code != SignatureTypeCode.Void)
            stack.Push(new(method.Storage.Bind(import.Signature.Result).StackType));
        return true;
    }
}
