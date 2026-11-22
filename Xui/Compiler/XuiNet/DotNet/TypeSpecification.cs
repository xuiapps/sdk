using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A borrowed type-specification signature, such as Box&lt;int&gt; or Box&lt;!0&gt;.</summary>
public readonly ref struct TypeSpecification
{
    private readonly Module module;
    internal TypeSpecification(Module module, TypeSpecificationHandle handle) { this.module = module; Handle = handle; }
    /// <summary>The row handle in the source module.</summary>
    public TypeSpecificationHandle Handle { get; }
    /// <summary>The borrowed signature blob.</summary>
    public BlobReader Signature => module.Metadata.GetBlobReader(module.Metadata.GetTypeSpecification(Handle).Signature);
    /// <summary>Decodes the signature, optionally substituting the containing type's arguments.</summary>
    public SignatureType Read(GenericArguments? arguments = null)
    {
        var reader = Signature;
        var type = SignatureReader.Type(module, ref reader, arguments);
        if (reader.RemainingBytes != 0) throw new BadImageFormatException("Unexpected trailing type-specification bytes.");
        return type;
    }
}
