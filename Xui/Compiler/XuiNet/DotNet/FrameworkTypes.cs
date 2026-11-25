using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>Recognizes framework type identities in both unresolved references and supplied implementation PEs.</summary>
public static class FrameworkTypes
{
    /// <summary>Matches a framework namespace/name without treating an application's namesake as an intrinsic.</summary>
    public static bool Is(TypeIdentity identity, string ns, string name)
    {
        var module = identity.Module;
        string assembly;
        if (identity.Handle.Kind == HandleKind.TypeDefinition)
        {
            var type = module.GetType((TypeDefinitionHandle)identity.Handle);
            if (type.Namespace != ns || type.Name != name || !module.Metadata.IsAssembly) return false;
            assembly = module.Metadata.GetString(module.Metadata.GetAssemblyDefinition().Name);
        }
        else if (identity.Handle.Kind == HandleKind.TypeReference)
        {
            var type = module.GetTypeReference((TypeReferenceHandle)identity.Handle);
            if (type.Namespace != ns || type.Name != name || type.ResolutionScope.Kind != HandleKind.AssemblyReference) return false;
            assembly = module.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope).Name;
        }
        else return false;
        return assembly is "System.Private.CoreLib" or "System.Runtime" or "mscorlib" ||
            assembly == "System.Runtime.InteropServices" && ns == "System.Runtime.InteropServices";
    }

    /// <summary>Whether this is a framework ValueTuple declaration (zero through eight generic arguments).</summary>
    public static bool IsValueTuple(TypeIdentity identity)
    {
        int arity = identity.Arguments?.Values.Length ?? 0;
        return arity <= 8 && Is(identity, "System", arity == 0 ? "ValueTuple" : $"ValueTuple`{arity}");
    }

    /// <summary>Runtime marker bases do not contribute instance storage to emitted value types.</summary>
    public static bool IsRuntimeBase(TypeIdentity identity) => Is(identity, "System", "Object") ||
        Is(identity, "System", "ValueType") || Is(identity, "System", "Enum");
}
