using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Module = Xui.Compiler.XuiNet.DotNet.Module;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>A statically specialized primitive instance call, including its receiver convention.</summary>
public sealed record PrimitiveCall(
    string Operation,
    SignatureType Element,
    bool AddressReceiver);

/// <summary>Recognizes direct and constrained primitive hashing and typed equality without virtual dispatch.</summary>
public static class PrimitiveCalls
{
    /// <summary>Uses stack analysis when Object.GetHashCode hides a statically known string receiver.</summary>
    public static IReadOnlyDictionary<int, PrimitiveCall> Discover(Module module, MethodBody body, InstructionGraph graph, MethodIdentity method)
    {
        var reader = body.Instructions;
        for (int i = 0; i < graph.Instructions.Count; i++)
        {
            var node = graph.Instructions[i];
            if (node.Code != OpCode.Callvirt || i > 0 && graph.Instructions[i - 1].Code == OpCode.Constrained) continue;
            reader.Seek(node.Offset);
            if (!IsObjectHash(module, MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value))) continue;
            graph = Analyzer.Analyze(module, method);
            break;
        }
        return Read(module, body, graph, method.DeclaringType, method.Arguments);
    }

    /// <summary>Recognizes the framework virtual hash declaration, without assuming its receiver type.</summary>
    public static bool IsObjectHash(Module module, EntityHandle token)
    {
        TypeIdentity owner;
        BlobReader blob;
        string name;
        if (token.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)token);
            if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return false;
            owner = new(module, member.Parent); blob = member.Signature; name = member.Name;
        }
        else if (token.Kind == HandleKind.MethodDefinition)
        {
            var method = module.GetMethod((MethodDefinitionHandle)token);
            owner = new(module, method.DeclaringType); blob = method.Signature; name = method.Name;
        }
        else return false;
        if (name != "GetHashCode" || !FrameworkTypes.Is(owner, "System", "Object")) return false;
        var signature = SignatureReader.Method(module, blob);
        return signature.HasThis && signature.GenericParameterCount == 0 && signature.Parameters.Length == 0 &&
            signature.Result == new SignatureType(SignatureTypeCode.Int32);
    }

    /// <summary>Maps call offsets to exact receiver types; custom overrides retain ordinary dispatch.</summary>
    public static IReadOnlyDictionary<int, PrimitiveCall> Read(Module module, MethodBody body, InstructionGraph graph,
        TypeIdentity context, GenericArguments? methodArguments)
    {
        var result = new Dictionary<int, PrimitiveCall>();
        var incoming = graph.Instructions.SelectMany(n => n.Successors).ToLookup(n => n.Offset);
        var reader = body.Instructions;
        for (int i = 0; i < graph.Instructions.Count; i++)
        {
            var call = graph.Instructions[i];
            if (call.Code is not (OpCode.Call or OpCode.Callvirt)) continue;
            reader.Seek(call.Offset);
            var token = MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value);
            EntityHandle parent;
            string name;
            BlobReader signature;
            if (token.Kind == HandleKind.MemberReference)
            {
                var member = module.GetMemberReference((MemberReferenceHandle)token);
                if (member.Kind != MemberReferenceKind.Method) continue;
                parent = member.Parent; name = member.Name; signature = member.Signature;
            }
            else if (token.Kind == HandleKind.MethodDefinition)
            {
                var method = module.GetMethod((MethodDefinitionHandle)token);
                parent = method.DeclaringType; name = method.Name; signature = method.Signature;
            }
            else continue;
            if (name is not ("GetHashCode" or "Equals") || parent.Kind is not
                (HandleKind.TypeDefinition or HandleKind.TypeReference or HandleKind.TypeSpecification)) continue;
            var owner = SignatureReader.Resolve(module, parent, context.Arguments, methodArguments).Identity;
            if (owner is null) continue;
            bool constrained = i > 0 && graph.Instructions[i - 1].Code == OpCode.Constrained;
            SignatureType element;
            if (constrained)
            {
                var prefix = graph.Instructions[i - 1];
                if (call.Code != OpCode.Callvirt || incoming[call.Offset].Count() != 1 ||
                    prefix.Successors.Length != 1 || prefix.Successors[0] != call) continue;
                if (body.ExceptionRegions.Any(r => call.Offset == r.TryOffset || call.Offset == r.TryOffset + r.TryLength ||
                    call.Offset == r.HandlerOffset || call.Offset == r.HandlerOffset + r.HandlerLength)) continue;
                reader.Seek(prefix.Offset);
                element = SignatureReader.StorageType(module, MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value), context.Arguments, methodArguments);
            }
            else if (call.Before?.Type is StackType.String or StackType.Null && IsObjectHash(module, token))
                element = new(SignatureTypeCode.String);
            else element = SignatureReader.StorageType(module, parent, context.Arguments, methodArguments);
            if (!PrimitiveValue.Supports(element)) continue;
            bool sameOwner = SignatureReader.StorageType(module, parent, context.Arguments, methodArguments) == element;
            bool objectHash = name == "GetHashCode" && (constrained || element.Code == SignatureTypeCode.String) && FrameworkTypes.Is(owner.Value, "System", "Object");
            bool equatable = name == "Equals" && constrained && FrameworkTypes.Is(owner.Value, "System", "IEquatable`1") &&
                owner.Value.Arguments?.Values.Length == 1 && owner.Value.Arguments.Values[0] == element;
            if (!sameOwner && !objectHash && !equatable) continue;
            var declared = SignatureReader.Method(module, signature, owner.Value.Arguments);
            if (!declared.HasThis || declared.GenericParameterCount != 0) continue;
            if (name == "GetHashCode" ? declared.Parameters.Length != 0 || declared.Result != new SignatureType(SignatureTypeCode.Int32)
                : declared.Parameters.Length != 1 || declared.Parameters[0] != element || declared.Result != new SignatureType(SignatureTypeCode.Boolean)) continue;
            result.Add(call.Offset, new(name, element, constrained || element.Code != SignatureTypeCode.String));
        }
        return result;
    }
}
