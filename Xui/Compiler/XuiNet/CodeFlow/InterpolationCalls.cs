using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Identifies the handler's constrained Object.ToString call without enabling general boxing or constrained dispatch.</summary>
public static class InterpolationCalls
{
    /// <summary>Indexes supported constrained calls by call offset, using actual instruction boundaries.</summary>
    public static IReadOnlyDictionary<int, InterpolatedStringHandler> Read(Module module, MethodBody body, InstructionGraph graph, TypeIdentity context, GenericArguments? arguments)
    {
        var result = new Dictionary<int, InterpolatedStringHandler>();
        var reader = body.Instructions;
        for (int i = 0; i + 1 < graph.Instructions.Count; i++)
        {
            var prefix = graph.Instructions[i];
            var call = graph.Instructions[i + 1];
            if (prefix.Code != OpCode.Constrained || call.Code != OpCode.Callvirt) continue;
            reader.Seek(prefix.Offset);
            var owner = SignatureReader.Resolve(module, MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value), context.Arguments, arguments).Identity;
            if (owner is not { } handler || !InterpolatedStringHandler.Is(handler)) continue;
            reader.Seek(call.Offset);
            var member = MemberResolver.Method(module, MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value), context, arguments);
            var definition = member.DeclaringType.Module.GetMethod(member.Handle);
            var signature = SignatureReader.Method(member.DeclaringType.Module, definition.Signature);
            if (definition.Name != "ToString" || !FrameworkTypes.Is(member.DeclaringType, "System", "Object") ||
                !signature.HasThis || signature.Parameters.Length != 0 || signature.Result.Code != System.Reflection.Metadata.SignatureTypeCode.String)
                throw new NotSupportedException("Only the interpolation handler's constrained Object.ToString call is implemented.");
            result.Add(call.Offset, new("ToString", handler, signature));
        }
        return result;
    }
}
