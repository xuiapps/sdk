using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A borrowed view of a class, interface, or other type declaration.</summary>
/// <remarks>Keep the owning assembly undisposed while using this view or any blob readers returned by it.</remarks>
public readonly ref struct TypeDefinition
{
    private readonly Module module;

    internal TypeDefinition(Module module, TypeDefinitionHandle handle)
    {
        this.module = module;
        Handle = handle;
    }

    /// <summary>Explicit override and interface method mappings declared by this type.</summary>
    public MethodImplementationHandleCollection MethodImplementations => Definition.GetMethodImplementations();

    /// <summary>Generic parameter declaration handles in metadata order.</summary>
    public GenericParameterHandleCollection GenericParameters => Definition.GetGenericParameters();

    /// <summary>The metadata handle within this module.</summary>
    public TypeDefinitionHandle Handle { get; }

    /// <summary>The enclosing type for a nested declaration, or a nil handle for a top-level type.</summary>
    public TypeDefinitionHandle DeclaringType => Definition.GetDeclaringType();

    private System.Reflection.Metadata.TypeDefinition Definition => module.Metadata.GetTypeDefinition(Handle);

    /// <summary>The namespace, decoded on access.</summary>
    public string Namespace => module.Metadata.GetString(Definition.Namespace);

    /// <summary>The type name, decoded on access.</summary>
    public string Name => module.Metadata.GetString(Definition.Name);

    /// <summary>The type flags.</summary>
    public TypeAttributes Attributes => Definition.Attributes;

    /// <summary>The packing and minimum size declared in PE metadata, before target-specific layout.</summary>
    public TypeLayout DeclaredLayout => Definition.GetLayout();

    /// <summary>Whether this type is an interface.</summary>
    public bool IsInterface => (Attributes & TypeAttributes.Interface) != 0;

    /// <summary>The number of declared generic parameters.</summary>
    public int GenericParameterCount => Definition.GetGenericParameters().Count;

    /// <summary>The base-type definition, reference, or specification handle.</summary>
    public EntityHandle BaseType => Definition.BaseType;

    /// <summary>Handles to the implemented-interface metadata rows.</summary>
    public InterfaceImplementationHandleCollection Interfaces => Definition.GetInterfaceImplementations();

    /// <summary>Handles to fields declared by this type.</summary>
    public FieldDefinitionHandleCollection Fields => Definition.GetFields();

    /// <summary>Handles to methods declared by this type.</summary>
    public MethodDefinitionHandleCollection Methods => Definition.GetMethods();

    /// <summary>Nested-type handles, obtained from metadata on demand.</summary>
    public ImmutableArray<TypeDefinitionHandle> NestedTypes => Definition.GetNestedTypes();
}
