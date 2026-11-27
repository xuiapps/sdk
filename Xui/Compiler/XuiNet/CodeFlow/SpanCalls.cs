using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Module = Xui.Compiler.XuiNet.DotNet.Module;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Recognizes constrained character-span ToString without boxing a ref struct.</summary>
public static class SpanCalls
{
    public static IReadOnlyDictionary<int, SpanAccess> Read(Module module, MethodBody body, InstructionGraph graph, TypeIdentity context, GenericArguments? arguments)
    {
        var result=new Dictionary<int,SpanAccess>();
        var incoming=graph.Instructions.SelectMany(n=>n.Successors).ToLookup(n=>n.Offset);
        var reader=body.Instructions;
        for(int i=1;i<graph.Instructions.Count;i++)
        {
            var call=graph.Instructions[i]; var prefix=graph.Instructions[i-1];
            if(prefix.Code!=OpCode.Constrained || call.Code!=OpCode.Callvirt) continue;
            reader.Seek(prefix.Offset);
            var owner=SignatureReader.Resolve(module,MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value),context.Arguments,arguments).Identity;
            if(owner is not { } span || !SpanAccess.Is(span) || span.Arguments?.Values.Length!=1 || span.Arguments.Values[0]!=new SignatureType(SignatureTypeCode.Char)) continue;
            if(incoming[call.Offset].Count()!=1 || prefix.Successors.Length!=1 || prefix.Successors[0]!=call ||
                body.ExceptionRegions.Any(r=>call.Offset==r.TryOffset || call.Offset==r.TryOffset+r.TryLength || call.Offset==r.HandlerOffset || call.Offset==r.HandlerOffset+r.HandlerLength))
                throw new InvalidProgramException("A constrained span call cannot be entered without its prefix.");
            reader.Seek(call.Offset);
            var target=MemberResolver.Method(module,MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value),context,arguments);
            var declaration=target.DeclaringType.Module.GetMethod(target.Handle);
            var signature=SignatureReader.Method(target.DeclaringType.Module,declaration.Signature);
            if(declaration.Name=="ToString" && FrameworkTypes.Is(target.DeclaringType,"System","Object") &&
                signature.HasThis && signature.GenericParameterCount==0 && signature.Parameters.Length==0 && signature.Result==new SignatureType(SignatureTypeCode.String))
                result.Add(call.Offset,new("ToString",span,signature));
        }
        return result;
    }
}
