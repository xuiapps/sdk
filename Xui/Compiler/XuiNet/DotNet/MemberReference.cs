using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A borrowed view of an unresolved method or field reference.</summary>
/// <remarks>Keep the owning assembly undisposed while using this view or any blob readers returned by it.</remarks>
public readonly ref struct MemberReference
{
    private readonly Module module;

    internal MemberReference(Module module, MemberReferenceHandle handle)
    {
        this.module = module;
        Handle = handle;
    }

    /// <summary>The metadata handle within this module.</summary>
    public MemberReferenceHandle Handle { get; }

    private System.Reflection.Metadata.MemberReference Definition => module.Metadata.GetMemberReference(Handle);

    /// <summary>The type or other metadata scope containing the referenced member.</summary>
    public EntityHandle Parent => Definition.Parent;

    /// <summary>The member name, decoded on access.</summary>
    public string Name => module.Metadata.GetString(Definition.Name);

    /// <summary>Whether this references a method or field.</summary>
    public MemberReferenceKind Kind => Definition.GetKind();

    /// <summary>A borrowed reader over the encoded member signature.</summary>
    public BlobReader Signature => module.Metadata.GetBlobReader(Definition.Signature);
}
