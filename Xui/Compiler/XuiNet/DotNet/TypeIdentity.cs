using Xui.Compiler.XuiNet.DotNet.Signatures;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A named type's definition and optional generic arguments, qualified by its owning module.</summary>
/// <remarks>Signature readers canonicalize references into supplied assemblies; unresolved external references retain their source module.</remarks>
public readonly record struct TypeIdentity(
    Module Module,
    EntityHandle Handle,
    GenericArguments? Arguments = null)
{
    /// <summary>Whether this resolved declaration is an interface.</summary>
    public bool IsInterface => Handle.Kind == HandleKind.TypeDefinition && Module.GetType((TypeDefinitionHandle)Handle).IsInterface;

    /// <summary>All implemented/inherited interfaces with generic arguments substituted, without duplicates.</summary>
    public IReadOnlyList<TypeIdentity> Interfaces()
    {
        var result = new List<TypeIdentity>();
        var visited = new HashSet<TypeIdentity>();
        void Visit(TypeIdentity type)
        {
            if (type.Handle.Kind != HandleKind.TypeDefinition || !visited.Add(type)) return;
            var definition = type.Module.GetType((TypeDefinitionHandle)type.Handle);
            foreach (var handle in definition.Interfaces)
            {
                var contract = SignatureReader.Resolve(type.Module, type.Module.GetInterfaceType(handle), type.Arguments).Identity!.Value;
                if (!result.Contains(contract)) result.Add(contract);
                Visit(contract);
            }
            if (type.BaseType is { } parent) Visit(parent);
        }
        Visit(this);
        return result;
    }

    /// <summary>Whether a definition derives directly from the framework ValueType or Enum marker.</summary>
    public bool IsValueType
    {
        get
        {
            if (Handle.Kind != HandleKind.TypeDefinition || FrameworkTypes.IsRuntimeBase(this)) return false;
            var type = Module.GetType((TypeDefinitionHandle)Handle);
            if (type.BaseType.IsNil) return false;
            var parent = new TypeIdentity(Module, type.BaseType);
            return FrameworkTypes.Is(parent, "System", "ValueType") || FrameworkTypes.Is(parent, "System", "Enum");
        }
    }

    /// <summary>Tests identity, class inheritance, and reference conversions through variant interfaces.</summary>
    public bool IsAssignableTo(TypeIdentity target) => TypeRelations.IsAssignable(this, target);

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
