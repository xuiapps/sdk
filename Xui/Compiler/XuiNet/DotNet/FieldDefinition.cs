using System.Reflection;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A borrowed view of a field declaration and signature.</summary>
/// <remarks>Keep the owning assembly undisposed while using this view or any blob readers returned by it.</remarks>
public readonly ref struct FieldDefinition
{
    private readonly Module module;

    internal FieldDefinition(Module module, FieldDefinitionHandle handle)
    {
        this.module = module;
        Handle = handle;
    }

    /// <summary>The metadata handle within this module.</summary>
    public FieldDefinitionHandle Handle { get; }

    private System.Reflection.Metadata.FieldDefinition Definition => module.Metadata.GetFieldDefinition(Handle);

    /// <summary>The declaring type handle.</summary>
    public TypeDefinitionHandle DeclaringType => Definition.GetDeclaringType();

    /// <summary>The field name, decoded on access.</summary>
    public string Name => module.Metadata.GetString(Definition.Name);

    /// <summary>The field flags.</summary>
    public FieldAttributes Attributes => Definition.Attributes;

    /// <summary>The offset declared in the FieldLayout table, or -1 when none is supplied.</summary>
    /// <remarks>Sequential-layout offsets are calculated by LayoutResolver, not stored here.</remarks>
    public int DeclaredOffset => Definition.GetOffset();

    /// <summary>A borrowed reader over the encoded field signature.</summary>
    public BlobReader Signature => module.Metadata.GetBlobReader(Definition.Signature);
}
