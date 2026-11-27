using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.XuiNet.Reachability;
using Module = Xui.Compiler.XuiNet.DotNet.Module;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Resolves specialized constrained receivers, preserving struct addresses and virtual class dispatch.</summary>
public static class ConstrainedCalls
{
    /// <summary>Maps call offsets to struct targets or class receivers requiring an indirect load. Struct boxing remains unsupported.</summary>
    public static IReadOnlyDictionary<int, ConstrainedCall> Read(Module module, MethodBody body, InstructionGraph graph,
        TypeIdentity context, GenericArguments? methodArguments)
    {
        var result = new Dictionary<int, ConstrainedCall>();
        var incoming = graph.Instructions.SelectMany(n => n.Successors).ToLookup(n => n.Offset);
        var dispatch = new DispatchTable();
        var primitives = PrimitiveCalls.Read(module, body, graph, context, methodArguments);
        var reader = body.Instructions;
        for (int i = 1; i < graph.Instructions.Count; i++)
        {
            var call = graph.Instructions[i];
            var prefix = graph.Instructions[i - 1];
            if (prefix.Code != OpCode.Constrained || call.Code is not (OpCode.Callvirt or OpCode.Call)) continue;
            if (primitives.ContainsKey(call.Offset)) continue;
            if (incoming[call.Offset].Count() != 1 || prefix.Successors.Length != 1 || prefix.Successors[0] != call ||
                body.ExceptionRegions.Any(r => call.Offset == r.TryOffset || call.Offset == r.TryOffset + r.TryLength ||
                    call.Offset == r.HandlerOffset || call.Offset == r.HandlerOffset + r.HandlerLength))
                throw new InvalidProgramException("A constrained call cannot be entered separately from its prefix.");
            reader.Seek(prefix.Offset);
            var receiver = SignatureReader.StorageType(module, MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value), context.Arguments, methodArguments);
            reader.Seek(call.Offset);
            var token = MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value);
            if (receiver.Identity is null)
            {
                if (call.Code == OpCode.Callvirt && (receiver.Code == SignatureTypeCode.Object || receiver.Array is not null) &&
                    DotNet.Intrinsics.ObjectHash.Is(module, token))
                    result.Add(call.Offset, new(MemberResolver.Method(module, token, context, methodArguments), receiver));
                continue;
            }
            var type = receiver.Identity.Value;
            var target = MemberResolver.Method(module, token, context, methodArguments);
            if (call.Code == OpCode.Call)
            {
                var attributes = target.DeclaringType.Module.GetMethod(target.Handle).Attributes;
                if (!target.DeclaringType.IsInterface || (attributes & System.Reflection.MethodAttributes.Static) == 0)
                    throw new NotSupportedException("A constrained direct call requires a static interface member.");
                var staticImplementation = dispatch.InterfaceImplementation(type, target);
                var implementationAttributes = staticImplementation.DeclaringType.Module.GetMethod(staticImplementation.Handle).Attributes;
                if ((implementationAttributes & (System.Reflection.MethodAttributes.Static | System.Reflection.MethodAttributes.Abstract)) != System.Reflection.MethodAttributes.Static)
                    throw new InvalidProgramException("A static interface call must resolve to a concrete static method.");
                result.Add(call.Offset, new(staticImplementation));
                continue;
            }
            if (!type.IsValueType)
            {
                result.Add(call.Offset, new(target, receiver));
                continue;
            }
            if (!target.DeclaringType.IsInterface)
            {
                if ((FrameworkTypes.Is(target.DeclaringType, "System", "Object") || FrameworkTypes.Is(target.DeclaringType, "System", "ValueType")) &&
                    target.DeclaringType.Module.GetMethod(target.Handle).Name == "GetHashCode" &&
                    DotNet.Intrinsics.ValueHash.Find(receiver) is { } hash)
                    result.Add(call.Offset, new(hash));
                continue;
            }
            var implementation = dispatch.InterfaceImplementation(type, target);
            if (implementation.DeclaringType != type) continue;
            if ((type.Module.GetMethod(implementation.Handle).Attributes & System.Reflection.MethodAttributes.Abstract) != 0)
                throw new InvalidProgramException("A constrained struct call must target a concrete method.");
            result.Add(call.Offset, new(implementation));
        }
        return result;
    }
}
