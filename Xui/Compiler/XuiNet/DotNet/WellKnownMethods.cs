using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>Exact metadata matches for runtime methods with compiler-defined behavior.</summary>
public static class WellKnownMethods
{
    /// <summary>Recognizes the parameterless System.Object constructor in the core library.</summary>
    public static bool IsObjectConstructor(Module module, EntityHandle handle)
    {
        if (handle.Kind != HandleKind.MemberReference) return false;
        var member = module.GetMemberReference((MemberReferenceHandle)handle);
        if (member.Name != ".ctor" || member.Parent.Kind != HandleKind.TypeReference) return false;
        var type = module.GetTypeReference((TypeReferenceHandle)member.Parent);
        if (type.Namespace != "System" || type.Name != "Object" || type.ResolutionScope.Kind != HandleKind.AssemblyReference) return false;
        if (module.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope).Name is not ("System.Private.CoreLib" or "System.Runtime" or "mscorlib")) return false;
        var signature = member.Signature;
        return signature.Length == 3 && signature.ReadByte() == (byte)SignatureAttributes.Instance && signature.ReadByte() == 0 && signature.ReadByte() == (byte)SignatureTypeCode.Void;
    }
}
