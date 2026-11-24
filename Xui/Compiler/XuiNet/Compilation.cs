using System.Reflection;
using System.Reflection.Metadata;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet;

/// <summary>A fixed set of implementation assemblies whose references can be resolved together.</summary>
/// <remarks>
/// The caller owns the assemblies and must keep them alive through analysis and emission.
/// Adding an assembly makes it available for resolution; it does not make it a preservation root.
/// Framework references not supplied here remain unresolved for host rewrites and intrinsics.
/// </remarks>
public sealed class Compilation
{
    private readonly Dictionary<string, Model.Assembly> assemblies = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<(Model.Module, EntityHandle), Model.TypeIdentity> resolved = new();
    private readonly Dictionary<Model.Module, Dictionary<(TypeDefinitionHandle Parent, string Namespace, string Name), TypeDefinitionHandle>> definitions = new();

    /// <summary>Binds the supplied assemblies into one resolution context; duplicate names are rejected.</summary>
    public Compilation(params Model.Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            if (assembly.ManifestModule.Compilation is not null)
                throw new ArgumentException("An assembly already belongs to a compilation.", nameof(assemblies));
            if (!this.assemblies.TryAdd(assembly.Identity.Name, assembly))
                throw new ArgumentException($"Duplicate assembly name: {assembly.Identity.Name}.", nameof(assemblies));
        }
        Assemblies = Array.AsReadOnly(assemblies.ToArray());
        foreach (var assembly in assemblies) assembly.ManifestModule.Compilation = this;
    }

    /// <summary>The explicitly supplied assemblies, in input order.</summary>
    public IReadOnlyList<Model.Assembly> Assemblies { get; }

    /// <summary>Resolves a named TypeRef to its canonical TypeDef when its implementation assembly is supplied.</summary>
    public Model.TypeIdentity ResolveType(Model.Module module, EntityHandle handle)
    {
        if (module.Compilation != this) throw new ArgumentException("Module does not belong to this compilation.", nameof(module));
        if (handle.Kind == HandleKind.TypeDefinition) return new(module, handle);
        if (handle.Kind != HandleKind.TypeReference) throw new NotSupportedException($"Named type resolution does not support {handle.Kind}.");
        if (resolved.TryGetValue((module, handle), out var identity)) return identity;
        var reference = module.GetTypeReference((TypeReferenceHandle)handle);
        Model.Module target;
        TypeDefinitionHandle parent = default;
        switch (reference.ResolutionScope.Kind)
        {
            case HandleKind.AssemblyReference:
                var dependency = module.GetAssemblyReference((AssemblyReferenceHandle)reference.ResolutionScope);
                if (!assemblies.TryGetValue(dependency.Name, out var assembly)) return new(module, handle);
                if (!Matches(assembly.Identity, dependency))
                    throw new NotSupportedException($"Assembly identity mismatch for {dependency.Name}, Version={dependency.Version} referenced by {module.Name}.");
                target = assembly.ManifestModule;
                break;
            case HandleKind.TypeReference:
                var enclosing = ResolveType(module, reference.ResolutionScope);
                if (enclosing.Handle.Kind != HandleKind.TypeDefinition) return new(module, handle);
                target = enclosing.Module;
                parent = (TypeDefinitionHandle)enclosing.Handle;
                break;
            case HandleKind.ModuleDefinition:
                target = module;
                break;
            default:
                throw new NotSupportedException($"Type resolution scope {reference.ResolutionScope.Kind} is not implemented.");
        }
        identity = ResolveNamed(target, parent, reference.Namespace, reference.Name, new());
        resolved.Add((module, handle), identity);
        return identity;
    }

    private Model.TypeIdentity ResolveNamed(Model.Module target, TypeDefinitionHandle parent, string ns, string name,
        HashSet<Model.Module> forwarding)
    {
        if (!forwarding.Add(target)) throw new BadImageFormatException($"Cyclic type forwarding for {ns}.{name}.");
        if (!definitions.TryGetValue(target, out var types))
        {
            types = new();
            foreach (var typeHandle in target.Types)
            {
                var type = target.GetType(typeHandle);
                types.Add((target.Metadata.GetTypeDefinition(typeHandle).GetDeclaringType(), type.Namespace, type.Name), typeHandle);
            }
            definitions.Add(target, types);
        }
        if (types.TryGetValue((parent, ns, name), out var definition)) return new(target, definition);
        if (parent.IsNil)
            foreach (var handle in target.Metadata.ExportedTypes)
            {
                var export = target.Metadata.GetExportedType(handle);
                if (!export.IsForwarder || target.Metadata.GetString(export.Namespace) != ns || target.Metadata.GetString(export.Name) != name) continue;
                if (export.Implementation.Kind != HandleKind.AssemblyReference)
                    throw new NotSupportedException($"Unsupported type forwarder for {ns}.{name} in {target.Name}.");
                var dependency = target.GetAssemblyReference((AssemblyReferenceHandle)export.Implementation);
                if (!assemblies.TryGetValue(dependency.Name, out var implementation))
                    throw new NotSupportedException($"Type {ns}.{name} is forwarded to {dependency.Name}; supply its implementation assembly.");
                if (!Matches(implementation.Identity, dependency))
                    throw new NotSupportedException($"Assembly identity mismatch for forwarded type {ns}.{name} in {dependency.Name}.");
                return ResolveNamed(implementation.ManifestModule, default, ns, name, forwarding);
            }
        throw new NotSupportedException($"Cannot resolve type {ns}.{name} in {target.Name}.");
    }

    private static bool Matches(Model.AssemblyIdentity definition, Model.AssemblyReference reference)
    {
        if (definition.Version != reference.Version || !string.Equals(definition.Culture, reference.Culture, StringComparison.OrdinalIgnoreCase)) return false;
        var key = definition.PublicKey;
        var token = reference.PublicKeyOrToken;
        var name = new AssemblyName();
        name.SetPublicKey(key.ReadBytes(key.Length));
        byte[] expected = name.GetPublicKeyToken() ?? [];
        byte[] actual = token.ReadBytes(token.Length);
        if ((reference.Flags & AssemblyFlags.PublicKey) != 0)
        {
            name = new AssemblyName();
            name.SetPublicKey(actual);
            actual = name.GetPublicKeyToken() ?? [];
        }
        return expected.AsSpan().SequenceEqual(actual);
    }
}
