using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Module = Xui.Compiler.XuiNet.DotNet.Module;

namespace Xui.Compiler.XuiNet.CodeFlow;

public static partial class StaticSpanCalls
{
    private static void ReadArrayInitializers(Module module, MethodBody body, InstructionGraph graph, TypeIdentity context,
        GenericArguments? methodArguments, Dictionary<int, StaticSpanCall> result)
    {
        var incoming = graph.Instructions.SelectMany(n => n.Successors).ToLookup(n => n.Offset);
        var reader = body.Instructions;
        for (int i = 0; i + 1 < graph.Instructions.Count; i++)
        {
            var tokenNode = graph.Instructions[i]; var call = graph.Instructions[i + 1];
            if (tokenNode.Code != OpCode.Ldtoken || call.Code != OpCode.Call || incoming[call.Offset].Count() != 1 ||
                tokenNode.Successors.Length != 1 || tokenNode.Successors[0] != call) continue;
            if (body.ExceptionRegions.Any(r => call.Offset == r.TryOffset || call.Offset == r.TryOffset + r.TryLength ||
                call.Offset == r.HandlerOffset || call.Offset == r.HandlerOffset + r.HandlerLength)) continue;
            reader.Seek(call.Offset);
            var callToken = MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value);
            if (callToken.Kind != HandleKind.MemberReference) continue;
            var member = module.GetMemberReference((MemberReferenceHandle)callToken);
            if (member.Name != "InitializeArray" || member.Kind != MemberReferenceKind.Method ||
                member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition) ||
                !FrameworkTypes.Is(new(module, member.Parent), "System.Runtime.CompilerServices", "RuntimeHelpers")) continue;
            var signature = SignatureReader.Method(module, member.Signature);
            if (signature.HasThis || signature.GenericParameterCount != 0 || signature.Result.Code != SignatureTypeCode.Void ||
                signature.Parameters.Length != 2 || signature.Parameters[0].Identity is not { } array || !FrameworkTypes.Is(array, "System", "Array") ||
                signature.Parameters[1].Identity is not { } handle || !FrameworkTypes.Is(handle, "System", "RuntimeFieldHandle")) continue;
            reader.Seek(tokenNode.Offset);
            var fieldToken = MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value);
            if (fieldToken.Kind is not (HandleKind.FieldDefinition or HandleKind.MemberReference)) continue;
            var access = Describe(module, fieldToken, context, methodArguments, new(SignatureTypeCode.Void), 1, tokenNode.Offset) with { ArrayInitializer = true };
            result.Add(tokenNode.Offset, access); result.Add(call.Offset, access);
        }
    }

    /// <summary>Width of a primitive or enum array element stored in static RVA data; zero for unsupported representations.</summary>
    public static int ArrayElementSize(SignatureType type) => Size(EnumInfo.Storage(type));
}
