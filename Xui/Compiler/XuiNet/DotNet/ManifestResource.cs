using System.Reflection;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A borrowed view of an assembly's embedded or linked resource declaration.</summary>
/// <remarks>Keep the owning assembly alive while reading the declaration or its data.</remarks>
public readonly ref struct ManifestResource
{
    private readonly Module module;

    internal ManifestResource(Module module, ManifestResourceHandle handle)
    {
        this.module = module;
        Handle = handle;
    }

    /// <summary>The resource's metadata row.</summary>
    public ManifestResourceHandle Handle { get; }
    private System.Reflection.Metadata.ManifestResource Definition => module.Metadata.GetManifestResource(Handle);

    /// <summary>The logical manifest name, including any .resources suffix.</summary>
    public string Name => module.Metadata.GetString(Definition.Name);
    /// <summary>The resource's manifest visibility.</summary>
    public ManifestResourceAttributes Attributes => Definition.Attributes;
    /// <summary>The supplying file or assembly; nil for a resource embedded in this PE.</summary>
    public EntityHandle Implementation => Definition.Implementation;
    /// <summary>The offset within the managed resource directory or supplying file.</summary>
    public long Offset => Definition.Offset;
    /// <summary>A borrowed reader over embedded bytes, excluding their PE length prefix.</summary>
    public BlobReader Data => module.ResourceData(Handle);

    /// <summary>Reads a string or null from an embedded .resources table without deserializing objects.</summary>
    /// <remarks>This reads the supplied neutral table; it does not probe culture satellite assemblies.</remarks>
    public string? ReadString(string key) => module.ReadResourceString(Handle, key);
}
