using System.Reflection;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>CLI type initialization triggers for closed types; BeforeFieldInit types initialize on field access.</summary>
public static class TypeInitialization
{
    /// <summary>Finds the static constructor in the supplied type's specialization context.</summary>
    public static MethodIdentity? Constructor(TypeIdentity type)
    {
        if (type.Handle.Kind != HandleKind.TypeDefinition) return null;
        foreach (var handle in type.Module.GetType((TypeDefinitionHandle)type.Handle).Methods)
            if (type.Module.GetMethod(handle).Name == ".cctor") return new(type, handle);
        return null;
    }

    /// <summary>Whether executing this method requires the declaring type to have initialized.</summary>
    public static bool AtMethodEntry(MethodIdentity identity)
    {
        var type = identity.DeclaringType;
        var method = type.Module.GetMethod(identity.Handle);
        return method.Name != ".cctor" &&
            (type.Module.GetType((TypeDefinitionHandle)type.Handle).Attributes & TypeAttributes.BeforeFieldInit) == 0 &&
            ((method.Attributes & MethodAttributes.Static) != 0 || method.Name == ".ctor" || type.IsValueType);
    }

    /// <summary>Metadata name used in initialization errors, including nesting and generic arity.</summary>
    public static string Name(TypeIdentity identity)
    {
        var type = identity.Module.GetType((TypeDefinitionHandle)identity.Handle);
        return type.DeclaringType.IsNil ? (type.Namespace.Length == 0 ? type.Name : type.Namespace + "." + type.Name)
            : Name(new(identity.Module, type.DeclaringType)) + "+" + type.Name;
    }
}
