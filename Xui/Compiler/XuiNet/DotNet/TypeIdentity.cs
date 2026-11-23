using Xui.Compiler.XuiNet.DotNet.Signatures;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A named type's definition and optional generic arguments, qualified by its owning module.</summary>
/// <remarks>Definition and reference handles remain distinct until assembly reference resolution is implemented.</remarks>
public readonly record struct TypeIdentity(
    Module Module,
    EntityHandle Handle,
    GenericArguments? Arguments = null)
{
    /// <summary>Whether a same-module definition derives directly from System.ValueType or System.Enum.</summary>
    public bool IsValueType
    {
        get
        {
            if (Handle.Kind != HandleKind.TypeDefinition) return false;
            var type = Module.GetType((TypeDefinitionHandle)Handle);
            if (type.BaseType.Kind != HandleKind.TypeReference) return false;
            var parent = Module.GetTypeReference((TypeReferenceHandle)type.BaseType);
            return parent.Namespace == "System" && parent.Name is "ValueType" or "Enum";
        }
    }

    /// <summary>Tests identity or a same-module class base chain; value types remain invariant.</summary>
    public bool IsAssignableTo(TypeIdentity target)
    {
        if (this == target) return true;
        if (Module != target.Module || IsValueType || target.IsValueType) return false;
        var current = this;
        var visited = new HashSet<TypeIdentity>();
        while (current.BaseType is { } parent && visited.Add(current))
        {
            if (parent == target) return true;
            current = parent;
        }
        return false;
    }

    /// <summary>The open declaration underlying this identity.</summary>
    public TypeIdentity GenericDefinition => new(Module, Handle);
    /// <summary>The base class after substituting this type's arguments, or null for a non-definition/root.</summary>
    public TypeIdentity? BaseType
    {
        get
        {
            if (Handle.Kind != HandleKind.TypeDefinition) return null;
            var parent = Module.GetType((TypeDefinitionHandle)Handle).BaseType;
            return parent.IsNil ? null : SignatureReader.Resolve(Module, parent, Arguments).Identity;
        }
    }

    /// <summary>The source type's namespace-qualified name. The assembly must remain alive.</summary>
    public override string ToString()
    {
        if (Handle.Kind == HandleKind.TypeDefinition)
        {
            var type = Module.GetType((TypeDefinitionHandle)Handle);
            return Format(Name(type.Namespace, type.Name));
        }
        if (Handle.Kind == HandleKind.TypeReference)
        {
            var type = Module.GetTypeReference((TypeReferenceHandle)Handle);
            return Format(Name(type.Namespace, type.Name));
        }
        throw new NotSupportedException($"Unsupported type identity {Handle.Kind}.");
    }

    private string Format(string name)
    {
        if (Arguments is null) return name;
        int arity = name.LastIndexOf('`');
        return (arity < 0 ? name : name[..arity]) + "<" + string.Join(", ", Arguments.Values) + ">";
    }

    private static string Name(string ns, string name) => ns.Length == 0 ? name : ns + "." + name;
}
