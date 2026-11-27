using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Module = Xui.Compiler.XuiNet.DotNet.Module;
using MethodBody = Xui.Compiler.XuiNet.DotNet.MethodBody;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Recognizes closed primitive spans backed by bounded static RVA data in supplied PEs.</summary>
public static partial class StaticSpanCalls
{
    /// <summary>Maps both the field-token and consuming call offsets; tokens cannot escape this adjacent pair.</summary>
    public static IReadOnlyDictionary<int, StaticSpanCall> Read(Module module, MethodBody body, InstructionGraph graph, TypeIdentity context, GenericArguments? methodArguments)
    {
        var result = new Dictionary<int, StaticSpanCall>();
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
            if (callToken.Kind != HandleKind.MethodSpecification) continue;
            var spec = module.GetMethodSpecification((MethodSpecificationHandle)callToken);
            TypeIdentity owner; string name; BlobReader blob;
            if (spec.Method.Kind == HandleKind.MemberReference)
            {
                var member = module.GetMemberReference((MemberReferenceHandle)spec.Method);
                if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) continue;
                owner = new(module, member.Parent); name = member.Name; blob = member.Signature;
            }
            else if (spec.Method.Kind == HandleKind.MethodDefinition)
            {
                var method = module.GetMethod((MethodDefinitionHandle)spec.Method);
                owner = new(module, method.DeclaringType); name = method.Name; blob = method.Signature;
            }
            else continue;
            if (name != "CreateSpan" || !FrameworkTypes.Is(owner, "System.Runtime.CompilerServices", "RuntimeHelpers")) continue;
            var arguments = spec.Read(context.Arguments, methodArguments);
            var signature = SignatureReader.Method(module, blob, methodArguments: arguments);
            if (arguments.Values.Length != 1 || signature.HasThis || signature.GenericParameterCount != 1 ||
                signature.Parameters.Length != 1 || signature.Parameters[0].Identity is not { } fieldHandle ||
                !FrameworkTypes.Is(fieldHandle, "System", "RuntimeFieldHandle") || signature.Parameters[0].IsByReference ||
                signature.Result.Identity is not { } span || !FrameworkTypes.Is(span, "System", "ReadOnlySpan`1") ||
                span.Arguments?.Values.Length != 1 || span.Arguments.Values[0] != arguments.Values[0]) continue;
            int elementSize = Size(EnumInfo.Storage(arguments.Values[0]));
            if (elementSize == 0) throw new NotSupportedException("Static spans require fixed-width primitive or enum elements.");
            reader.Seek(tokenNode.Offset);
            var fieldToken = MetadataTokens.EntityHandle(reader.Read().ReadToken(ref reader).Value);
            if (fieldToken.Kind is not (HandleKind.FieldDefinition or HandleKind.MemberReference)) continue;
            var access = Describe(module, fieldToken, context, methodArguments, signature.Result, elementSize, tokenNode.Offset);
            result.Add(tokenNode.Offset, access); result.Add(call.Offset, access);
        }
        ReadPointerConstructors(module, body, graph, context, methodArguments, result);
        ReadArrayInitializers(module, body, graph, context, methodArguments, result);
        return result;
    }

    private static StaticSpanCall Describe(Module module, EntityHandle fieldToken, TypeIdentity context, GenericArguments? methodArguments,
        SignatureType resultType, int elementSize, int tokenOffset, int? count = null)
    {
        var identity = MemberResolver.Field(module, fieldToken, context, methodArguments);
        var field = identity.DeclaringType.Module.GetField(identity.Handle);
        if ((field.Attributes & (FieldAttributes.Static | FieldAttributes.HasFieldRVA)) != (FieldAttributes.Static | FieldAttributes.HasFieldRVA) || field.RelativeVirtualAddress == 0)
            throw new NotSupportedException("Static span fields must have initial RVA data.");
        var storage = SignatureReader.Field(identity.DeclaringType.Module, field.Signature, identity.DeclaringType.Arguments);
        int length = Size(storage);
        if (length == 0 && storage.Identity is { Handle.Kind: HandleKind.TypeDefinition } data)
            length = data.Module.GetType((TypeDefinitionHandle)data.Handle).DeclaredLayout.Size;
        if (length <= 0 || length % elementSize != 0) throw new BadImageFormatException("Static span data length must contain whole elements.");
        _ = field.InitialData(length); // Check the PE bounds before exposing an address.
        int elementCount = count ?? length / elementSize;
        if (elementCount < 0 || (long)elementCount * elementSize > length)
            throw new NotSupportedException("Static span constructor length is outside its RVA field.");
        return new(identity, resultType, length, elementSize, tokenOffset, elementCount, count is not null);
    }

    private static int Size(SignatureType type) => type.IsByReference || type.Identity is not null ? 0 : type.Code switch {
        SignatureTypeCode.Boolean or SignatureTypeCode.Byte or SignatureTypeCode.SByte => 1,
        SignatureTypeCode.Char or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 => 2,
        SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.Single => 4,
        SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 or SignatureTypeCode.Double => 8,
        _ => 0
    };
}
