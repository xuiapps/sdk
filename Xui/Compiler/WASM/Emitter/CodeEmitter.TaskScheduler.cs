using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryTaskScheduler(Instruction.Token instruction, int offset)
    {
        if (HostScheduler.Read(source, MetadataTokens.EntityHandle(instruction.Value)) is not { } scheduler) return false;
        PopAssignable(method.Storage.Bind(scheduler.Signature.Parameters[0]), instruction.Code, offset);
        writer.LocalSet(method.GcSource).RefFunc(methods[scheduler.Callback].Function).LocalGet(method.GcSource).Call(method.Storage.Tasks.Post);
        return true;
    }
}
