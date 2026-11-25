using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>A call in the nonescaping InvariantCulture/Int32.ToString pair.</summary>
public readonly record struct InvariantFormattingCall(bool IsProvider, SignatureType Provider);

/// <summary>Recognizes an invariant provider consumed immediately by integer formatting, with no alternate incoming edge.</summary>
public static class InvariantFormattingCalls
{
    public static IReadOnlyDictionary<int, InvariantFormattingCall> Read(Module module, MethodBody body, InstructionGraph graph)
    {
        var result = new Dictionary<int, InvariantFormattingCall>();
        var incoming = graph.Instructions.SelectMany(n => n.Successors).ToLookup(n => n.Offset);
        var reader = body.Instructions;
        for (int i = 0; i + 1 < graph.Instructions.Count; i++)
        {
            var provider = graph.Instructions[i];
            var call = graph.Instructions[i + 1];
            if (provider.Code != OpCode.Call || call.Code != OpCode.Call || incoming[call.Offset].Count() != 1 ||
                provider.Successors.Length != 1 || provider.Successors[0] != call) continue;
            if (body.ExceptionRegions.Any(r => call.Offset == r.TryOffset || call.Offset == r.TryOffset + r.TryLength ||
                call.Offset == r.HandlerOffset || call.Offset == r.HandlerOffset + r.HandlerLength)) continue;
            reader.Seek(provider.Offset);
            var type = Int32Formatting.InvariantProvider(module, MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value));
            if (type is null) continue;
            reader.Seek(call.Offset);
            if (!Int32Formatting.Is(module, MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value), withProvider: true)) continue;
            result.Add(provider.Offset, new(true, type.Value));
            result.Add(call.Offset, new(false, type.Value));
        }
        return result;
    }
}
