using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A managed assembly and the retained PE image backing its manifest module.</summary>
/// <remarks>
/// Dispose after consumers have finished reading method bodies. The caller's input stream
/// remains independent. Referenced assemblies and additional modules are not loaded automatically.
/// </remarks>
public sealed class Assembly : IDisposable
{
    private readonly PEReader pe;
    private readonly MetadataReader metadata;
    private bool disposed;

    internal Assembly(PEReader pe, MetadataReader metadata)
    {
        this.pe = pe;
        this.metadata = metadata;
        ManifestModule = new Module(this);
    }

    /// <summary>A borrowed view of the assembly manifest's identity.</summary>
    public AssemblyIdentity Identity => new(ManifestModule);

    internal MetadataReader Metadata
    {
        get
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            return metadata;
        }
    }

    internal PEReader PE
    {
        get
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            return pe;
        }
    }

    /// <summary>The module containing the assembly manifest.</summary>
    public Module ManifestModule { get; }

    /// <summary>Releases the PE image used for deferred method-body reads.</summary>
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        ManifestModule.DisposeResourceReaders();
        pe.Dispose();
    }
}
