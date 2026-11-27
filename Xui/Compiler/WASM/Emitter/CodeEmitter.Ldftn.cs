using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitLdftn(Instruction.Token instruction, int offset)
    {
        var target = ResolveCall(source, method, instruction.Value, offset);
        writer.I32Const((int)method.Storage.DelegateTargets[target].TableSlot);
        stack.Push(new(SignatureTypeCode.IntPtr) { Method = target });
    }
}
