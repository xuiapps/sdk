using System.Reflection;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>The assembly identity read directly from its manifest.</summary>
/// <remarks>Keep the owning assembly undisposed while using this view or any blob readers returned by it.</remarks>
public readonly ref struct AssemblyIdentity
{
    private readonly Module module;

    internal AssemblyIdentity(Module module)
    {
        this.module = module;
    }

    private System.Reflection.Metadata.AssemblyDefinition Definition => module.Metadata.GetAssemblyDefinition();

    /// <summary>The assembly name, decoded on access.</summary>
    public string Name => module.Metadata.GetString(Definition.Name);

    /// <summary>The assembly version.</summary>
    public Version Version => Definition.Version;

    /// <summary>The culture name, decoded on access.</summary>
    public string Culture => module.Metadata.GetString(Definition.Culture);

    /// <summary>The assembly flags.</summary>
    public AssemblyFlags Flags => Definition.Flags;

    /// <summary>The manifest hash algorithm.</summary>
    public AssemblyHashAlgorithm HashAlgorithm => Definition.HashAlgorithm;

    /// <summary>A borrowed reader over the public key without copying its bytes.</summary>
    public BlobReader PublicKey => module.Metadata.GetBlobReader(Definition.PublicKey);
}
