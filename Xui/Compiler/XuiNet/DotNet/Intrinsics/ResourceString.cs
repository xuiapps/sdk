using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Resolves constant-key BCL resource getters against the supplied PE's neutral string table.</summary>
/// <remarks>Only the exact ldstr/GetResourceString/ret getter shape is folded. Culture switching is not implemented.</remarks>
public static class ResourceString
{
    /// <summary>Returns a getter's resource text, or null when the call is not a supported resource getter.</summary>
    public static string? Read(Module module, EntityHandle token)
    {
        TypeIdentity owner;
        string name;
        if (token.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)token);
            if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return null;
            owner = new(module, member.Parent); name = member.Name;
        }
        else if (token.Kind == HandleKind.MethodDefinition)
        {
            var method = module.GetMethod((MethodDefinitionHandle)token);
            owner = new(module, method.DeclaringType); name = method.Name;
        }
        else return null;
        if (!name.StartsWith("get_", StringComparison.Ordinal) || !IsResourceOwner(owner)) return null;
        var identity = MemberResolver.Method(module, token, owner);
        var source = identity.DeclaringType.Module;
        var getter = source.GetMethod(identity.Handle);
        var signature = SignatureReader.Method(source, getter.Signature);
        if (signature.HasThis || signature.GenericParameterCount != 0 || signature.Parameters.Length != 0 ||
            signature.Result.Code != SignatureTypeCode.String || getter.Body is not { } body || body.ExceptionRegions.Length != 0) return null;
        var reader = body.Instructions;
        if (!reader.HasRemaining) return null;
        var literal = reader.Read();
        if (literal.Code != OpCode.Ldstr) return null;
        string key = source.GetString(MetadataTokens.UserStringHandle(literal.ReadToken(ref reader).Value & 0xffffff));
        if (!reader.HasRemaining) return null;
        var call = reader.Read();
        if (call.Code != OpCode.Call) return null;
        var target = MemberResolver.Method(source, MetadataTokens.EntityHandle(call.ReadToken(ref reader).Value), identity.DeclaringType);
        if (target.DeclaringType != identity.DeclaringType) return null;
        var lookup = source.GetMethod(target.Handle);
        var lookupSignature = SignatureReader.Method(source, lookup.Signature);
        if (lookup.Name != "GetResourceString" || lookupSignature.HasThis || lookupSignature.GenericParameterCount != 0 ||
            lookupSignature.Result.Code != SignatureTypeCode.String || lookupSignature.Parameters.Length != 1 ||
            lookupSignature.Parameters[0].Code != SignatureTypeCode.String || lookupSignature.Parameters[0].IsByReference) return null;
        if (!reader.HasRemaining || reader.Read().Code != OpCode.Ret || reader.HasRemaining) return null;

        string assemblyName = source.Metadata.GetString(source.Metadata.GetAssemblyDefinition().Name);
        string resourceName = assemblyName == "System.Private.CoreLib" ? assemblyName + ".Strings.resources" :
            "FxResources." + assemblyName + ".SR.resources";
        foreach (var handle in source.ManifestResources)
        {
            var resource = source.GetManifestResource(handle);
            if (resource.Name == resourceName)
                return resource.ReadString(key) ?? throw new NotSupportedException($"Null BCL resource string: {resourceName}/{key}.");
        }
        throw new NotSupportedException($"The supplied BCL PE has no neutral resource table '{resourceName}'.");
    }

    private static bool IsResourceOwner(TypeIdentity owner)
    {
        if (FrameworkTypes.Is(owner, "System", "SR")) return true;
        string assembly;
        if (owner.Handle.Kind == HandleKind.TypeDefinition)
        {
            var type = owner.Module.GetType((TypeDefinitionHandle)owner.Handle);
            if (type.Namespace != "System" || type.Name != "SR") return false;
            assembly = owner.Module.Metadata.GetString(owner.Module.Metadata.GetAssemblyDefinition().Name);
        }
        else if (owner.Handle.Kind == HandleKind.TypeReference)
        {
            var type = owner.Module.GetTypeReference((TypeReferenceHandle)owner.Handle);
            if (type.Namespace != "System" || type.Name != "SR" || type.ResolutionScope.Kind != HandleKind.AssemblyReference) return false;
            assembly = owner.Module.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope).Name;
        }
        else return false;
        return assembly is "System.Private.Xml.Linq" or "System.Private.Xml" or "System.Private.Uri" or
            "System.Text.Json" or "System.IO.Compression" or "System.Linq";
    }
}
