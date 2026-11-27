using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Typed buffer operations that retain managed owners instead of exposing raw GC addresses.</summary>
public sealed record ManagedBuffer(string Operation, SignatureType Element, MethodSignature Signature)
{
    /// <summary>Recognizes closed array-data references, element-relative Unsafe.Add and Buffer.Memmove.</summary>
    public static ManagedBuffer? Read(Module module, EntityHandle token, GenericArguments? context, GenericArguments? methodArguments)
    {
        if (token.Kind != HandleKind.MethodSpecification) return null;
        var spec = module.GetMethodSpecification((MethodSpecificationHandle)token);
        TypeIdentity owner; string name; BlobReader blob;
        if (spec.Method.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)spec.Method);
            if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return null;
            owner = new(module, member.Parent); name = member.Name; blob = member.Signature;
        }
        else if (spec.Method.Kind == HandleKind.MethodDefinition)
        {
            var method = module.GetMethod((MethodDefinitionHandle)spec.Method);
            owner = new(module, method.DeclaringType); name = method.Name; blob = method.Signature;
        }
        else return null;
        bool array = name == "GetArrayDataReference" && FrameworkTypes.Is(owner, "System.Runtime.InteropServices", "MemoryMarshal");
        bool add = name == "Add" && FrameworkTypes.Is(owner, "System.Runtime.CompilerServices", "Unsafe");
        bool move = name == "Memmove" && FrameworkTypes.Is(owner, "System", "Buffer");
        if (!array && !add && !move) return null;
        var arguments = spec.Read(context, methodArguments);
        if (arguments.Values.Length != 1 || arguments.Values[0].ContainsGenericParameters) return null;
        var element = arguments.Values[0];
        var signature = SignatureReader.Method(module, blob, context, arguments);
        if (signature.HasThis || signature.GenericParameterCount != 1) return null;
        var parameters = signature.Parameters;
        bool valid = array ? parameters.Length == 1 && parameters[0] == SignatureType.Vector(element) && signature.Result == element.ByReference() :
            add ? parameters.Length == 2 && parameters[0] == element.ByReference() && !parameters[1].IsByReference &&
                parameters[1].Code is SignatureTypeCode.Int32 or SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr && signature.Result == element.ByReference() :
            parameters.Length == 3 && parameters[0] == element.ByReference() && parameters[1] == element.ByReference() &&
                parameters[2] == new SignatureType(SignatureTypeCode.UIntPtr) && signature.Result.Code == SignatureTypeCode.Void;
        return valid ? new(name, element, signature) : null;
    }
}
