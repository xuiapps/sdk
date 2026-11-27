using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>A declared browser runtime import or a narrowly recognized BCL host operation.</summary>
public sealed record RuntimeImport(MethodIdentity Method, string ModuleName, string Name, MethodSignature Signature)
{
    public static RuntimeImport? Read(Module module, EntityHandle token)
    {
        EntityHandle parent;
        if (token.Kind == HandleKind.MethodDefinition) parent = module.GetMethod((MethodDefinitionHandle)token).DeclaringType;
        else if (token.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)token);
            if (member.Kind != MemberReferenceKind.Method) return null;
            parent = member.Parent;
        }
        else return null;
        if (parent.Kind is not (HandleKind.TypeDefinition or HandleKind.TypeReference)) return null;
        var owner = module.ResolveType(parent);
        if (FrameworkTypes.Is(owner, "System", "HashCode"))
        {
            var seed = MemberResolver.Method(module, token, owner);
            var definition = owner.Module.GetMethod(seed.Handle);
            var signature = SignatureReader.Method(owner.Module, definition.Signature);
            if (definition.Name == "GenerateGlobalSeed" && !signature.HasThis && signature.GenericParameterCount == 0 &&
                signature.Parameters.Length == 0 && signature.Result == new SignatureType(SignatureTypeCode.UInt32))
                return new(seed, "xui-hash", "seed", signature);
        }
        if (owner.Handle.Kind != HandleKind.TypeDefinition ||
            owner.Module.Metadata.GetString(owner.Module.Metadata.GetAssemblyDefinition().Name) != "Xui.Compiler.Runtime") return null;
        var method = MemberResolver.Method(module, token, owner);
        var metadata = method.DeclaringType.Module.Metadata;
        foreach (var handle in metadata.GetMethodDefinition(method.Handle).GetCustomAttributes())
        {
            var attribute = metadata.GetCustomAttribute(handle);
            var ctor = MemberResolver.Method(method.DeclaringType.Module, attribute.Constructor, owner);
            var type = ctor.DeclaringType.Module.GetType((TypeDefinitionHandle)ctor.DeclaringType.Handle);
            if (ctor.DeclaringType.Module != owner.Module || type.Namespace != "Xui.Compiler.Runtime" || type.Name != "RuntimeImportAttribute") continue;
            var blob = metadata.GetBlobReader(attribute.Value);
            if (blob.ReadUInt16() != 1) throw new BadImageFormatException("Invalid runtime import attribute.");
            string moduleName = blob.ReadSerializedString() ?? throw new BadImageFormatException("Missing import module.");
            string name = blob.ReadSerializedString() ?? throw new BadImageFormatException("Missing import name.");
            if (blob.ReadUInt16() != 0 || blob.RemainingBytes != 0) throw new BadImageFormatException("Unexpected runtime import arguments.");
            var signature = SignatureReader.Method(owner.Module, owner.Module.GetMethod(method.Handle).Signature);
            if (signature.HasThis || signature.GenericParameterCount != 0 || !Allowed(signature.Result, true) || signature.Parameters.Any(p => !Allowed(p, false)))
                throw new NotSupportedException($"Unsupported browser runtime import signature: {method}.");
            return new(method, moduleName, name, signature);
        }
        return null;
    }

    private static bool Allowed(SignatureType type, bool result) => !type.IsByReference && type.Identity is null && type.Array is null &&
        (result && type.Code == SignatureTypeCode.Void || type.Code is SignatureTypeCode.Object or SignatureTypeCode.String or
            SignatureTypeCode.Boolean or SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.Int64 or
            SignatureTypeCode.UInt64 or SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr or SignatureTypeCode.Double);
}
