using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A borrowed view of an unresolved type name and scope.</summary>
/// <remarks>Keep the owning assembly undisposed while using this view or any blob readers returned by it.</remarks>
public readonly ref struct TypeReference
{
    private readonly Module module;

    internal TypeReference(Module module, TypeReferenceHandle handle)
    {
        this.module = module;
        Handle = handle;
    }

    /// <summary>The metadata handle within this module.</summary>
    public TypeReferenceHandle Handle { get; }

    private System.Reflection.Metadata.TypeReference Definition => module.Metadata.GetTypeReference(Handle);

    /// <summary>The assembly, module, or enclosing-type scope.</summary>
    public EntityHandle ResolutionScope => Definition.ResolutionScope;

    /// <summary>The namespace, decoded on access.</summary>
    public string Namespace => module.Metadata.GetString(Definition.Namespace);

    /// <summary>The type name, decoded on access.</summary>
    public string Name => module.Metadata.GetString(Definition.Name);
}
