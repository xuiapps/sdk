using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryMonotonicClock(Instruction.Token instruction)
    {
        var clock = MonotonicClock.Read(source, MetadataTokens.EntityHandle(instruction.Value));
        if (clock is null) return false;
        writer.Call(clock.Operation == "get_TickCount64" ? method.Storage.Clock.TickCount64 : method.Storage.Clock.Timestamp);
        stack.Push(new(SignatureTypeCode.Int64));
        return true;
    }
}
