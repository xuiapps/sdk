using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A view of a module's metadata, with handles for traversing its declarations.</summary>
/// <remarks>Declaration views and blob readers borrow the PE image owned by the assembly.</remarks>
public sealed partial class Module
{
    private readonly Assembly assembly;
    private readonly Dictionary<MethodDefinitionHandle, MethodBody?> bodies = new();
    private readonly Dictionary<UserStringHandle, string> strings = new();

    internal Module(Assembly assembly) => this.assembly = assembly;

    internal MetadataReader Metadata => assembly.Metadata;

    /// <summary>The explicit assembly-resolution context, if this module has been bound to one.</summary>
    public Compilation? Compilation { get; internal set; }

    internal TypeIdentity ResolveType(EntityHandle handle) =>
        Compilation?.ResolveType(this, handle) ?? new(this, handle);

    private ModuleDefinition Definition => Metadata.GetModuleDefinition();

    /// <summary>The module name, decoded on access.</summary>
    public string Name => Metadata.GetString(Definition.Name);

    /// <summary>The module version identifier.</summary>
    public Guid Mvid => Metadata.GetGuid(Definition.Mvid);

    /// <summary>The managed entry point, or a nil handle for a library.</summary>
    public MethodDefinitionHandle EntryPoint
    {
        get
        {
            int token = assembly.PE.PEHeaders.CorHeader!.EntryPointTokenOrRelativeVirtualAddress;
            return token == 0 ? default : (MethodDefinitionHandle)MetadataTokens.EntityHandle(token);
        }
    }

    /// <summary>Type declaration handles in metadata row order.</summary>
    public TypeDefinitionHandleCollection Types => Metadata.TypeDefinitions;

    /// <summary>Method declaration handles in metadata row order.</summary>
    public MethodDefinitionHandleCollection Methods => Metadata.MethodDefinitions;

    /// <summary>Field declaration handles in metadata row order.</summary>
    public FieldDefinitionHandleCollection Fields => Metadata.FieldDefinitions;

    /// <summary>Handles to unresolved assembly references.</summary>
    public AssemblyReferenceHandleCollection AssemblyReferences => Metadata.AssemblyReferences;

    /// <summary>Handles to unresolved named type references.</summary>
    public TypeReferenceHandleCollection TypeReferences => Metadata.TypeReferences;

    /// <summary>Handles to unresolved method and field references.</summary>
    public MemberReferenceHandleCollection MemberReferences => Metadata.MemberReferences;

    /// <summary>Type-specification rows in metadata order.</summary>
    public IEnumerable<TypeSpecificationHandle> TypeSpecifications
    {
        get
        {
            for (int row = 1; row <= Metadata.GetTableRowCount(TableIndex.TypeSpec); row++)
                yield return MetadataTokens.TypeSpecificationHandle(row);
        }
    }

    /// <summary>Creates a borrowed method-instantiation view.</summary>
    public MethodSpecification GetMethodSpecification(MethodSpecificationHandle handle) => new(this, handle);

    /// <summary>Gets a borrowed type specification.</summary>
    public TypeSpecification GetTypeSpecification(TypeSpecificationHandle handle) => new(this, handle);
    /// <summary>Creates a borrowed generic-parameter view.</summary>
    public GenericParameter GetGenericParameter(GenericParameterHandle handle) => new(this, handle);
    /// <summary>Reads the type referred to by a generic parameter constraint.</summary>
    public EntityHandle GetGenericConstraintType(GenericParameterConstraintHandle handle) => Metadata.GetGenericParameterConstraint(handle).Type;

    /// <summary>String literals cached by explicit GetString calls.</summary>
    public IReadOnlyDictionary<UserStringHandle, string> Strings => strings;

    /// <summary>Decodes a string literal on first request and caches it separately from IL iteration.</summary>
    public string GetString(UserStringHandle handle)
    {
        var metadata = Metadata;
        if (strings.TryGetValue(handle, out var value)) return value;
        value = metadata.GetUserString(handle);
        strings.Add(handle, value);
        return value;
    }

    /// <summary>Creates a borrowed view of a type declaration.</summary>
    public TypeDefinition GetType(TypeDefinitionHandle handle) => new(this, handle);

    /// <summary>Creates a borrowed view of a method declaration.</summary>
    public MethodDefinition GetMethod(MethodDefinitionHandle handle) => new(this, handle);

    /// <summary>Creates a borrowed view of a field declaration.</summary>
    public FieldDefinition GetField(FieldDefinitionHandle handle) => new(this, handle);

    /// <summary>Creates a borrowed view of a type reference.</summary>
    public TypeReference GetTypeReference(TypeReferenceHandle handle) => new(this, handle);

    /// <summary>Creates a borrowed view of a member reference.</summary>
    public MemberReference GetMemberReference(MemberReferenceHandle handle) => new(this, handle);

    /// <summary>Creates a borrowed view of an assembly dependency.</summary>
    public AssemblyReference GetAssemblyReference(AssemblyReferenceHandle handle) => new(this, handle);

    /// <summary>Reads the interface type referenced by an interface implementation row.</summary>
    public EntityHandle GetInterfaceType(InterfaceImplementationHandle handle) =>
        Metadata.GetInterfaceImplementation(handle).Interface;

    /// <summary>Reads the declaration and implementation tokens of an explicit method mapping.</summary>
    public (EntityHandle Declaration, EntityHandle Body) GetMethodImplementation(MethodImplementationHandle handle)
    {
        var mapping = Metadata.GetMethodImplementation(handle);
        return (mapping.MethodDeclaration, mapping.MethodBody);
    }

    internal MethodBody? GetBody(MethodDefinitionHandle handle)
    {
        var metadata = Metadata;
        if (bodies.TryGetValue(handle, out var body)) return body;
        body = MethodBody.CreateBody(assembly.PE, metadata, metadata.GetMethodDefinition(handle));
        bodies.Add(handle, body);
        return body;
    }
}
