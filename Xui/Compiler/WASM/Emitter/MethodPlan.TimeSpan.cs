using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class MethodPlan
{
    private bool CallsTimeSpanFactory()
    {
        var reader = Body.Instructions;
        foreach (var node in Graph.Instructions)
        {
            if (node.Code != OpCode.Call) continue;
            reader.Seek(node.Offset);
            var call = reader.Read().ReadToken(ref reader);
            if (TimeSpanFactory.Read(Identity.DeclaringType.Module, MetadataTokens.EntityHandle(call.Value)) is not null) return true;
        }
        return false;
    }
}
