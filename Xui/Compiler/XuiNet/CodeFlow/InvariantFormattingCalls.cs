using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>A call in a nonescaping invariant-provider/scalar.ToString pair.</summary>
public readonly record struct InvariantFormattingCall(
    bool IsProvider,
    SignatureType Provider,
    SignatureType Value,
    bool HasNullFormat = false,
    NumericParsing? Parsing = null)
{
    public SignatureTypeCode ValueType => Value.Identity is { } identity && NativeFloat.Is(identity) ? SignatureTypeCode.Single : Value.Code;
}

/// <summary>Recognizes an invariant provider consumed immediately by scalar formatting, with no alternate incoming edge.</summary>
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
            var token = MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value);
            bool nullFormat = (IntegerFormatting.Read(module, token, withFormat: true) is not null ||
                DecimalFormatting.Read(module, token, withFormat: true) is not null) && i > 0 &&
                graph.Instructions[i - 1].Code == OpCode.Ldnull && incoming[provider.Offset].Count() == 1 &&
                graph.Instructions[i - 1].Successors.Length == 1 && graph.Instructions[i - 1].Successors[0] == provider;
            var parsing = NumericParsing.Read(module, token);
            SignatureType? valueType = parsing?.Result ?? DecimalFormatting.Read(module, token, withFormat: nullFormat) ??
                IntegerFormatting.Read(module, token, withFormat: nullFormat) ?? FloatingPointFormatting.Read(module, token);
            if (valueType is null) continue;
            result.Add(provider.Offset, new(true, type.Value, valueType.Value, nullFormat, parsing));
            result.Add(call.Offset, new(false, type.Value, valueType.Value, nullFormat, parsing));
        }
        return result;
    }
}
