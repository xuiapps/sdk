using System.Reflection;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A borrowed view of an unresolved assembly reference.</summary>
/// <remarks>Keep the owning assembly undisposed while using this view or any blob readers returned by it.</remarks>
public readonly ref struct AssemblyReference
{
    private readonly Module module;

    internal AssemblyReference(Module module, AssemblyReferenceHandle handle)
    {
        this.module = module;
        Handle = handle;
    }

    /// <summary>The metadata handle within this module.</summary>
    public AssemblyReferenceHandle Handle { get; }

    private System.Reflection.Metadata.AssemblyReference Definition => module.Metadata.GetAssemblyReference(Handle);

    /// <summary>The dependency name, decoded on access.</summary>
    public string Name => module.Metadata.GetString(Definition.Name);

    /// <summary>The requested assembly version.</summary>
    public Version Version => Definition.Version;

    /// <summary>The requested culture, decoded on access.</summary>
    public string Culture => module.Metadata.GetString(Definition.Culture);

    /// <summary>The assembly reference flags.</summary>
    public AssemblyFlags Flags => Definition.Flags;

    /// <summary>A borrowed reader over the public key or token.</summary>
    public BlobReader PublicKeyOrToken => module.Metadata.GetBlobReader(Definition.PublicKeyOrToken);

    /// <summary>A borrowed reader over the reference hash.</summary>
    public BlobReader HashValue => module.Metadata.GetBlobReader(Definition.HashValue);
}
