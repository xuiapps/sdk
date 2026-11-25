using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A borrowed generic method instantiation and its argument signature.</summary>
public readonly ref struct MethodSpecification
{
    private readonly Module module;
    internal MethodSpecification(Module module, MethodSpecificationHandle handle) { this.module = module; Handle = handle; }
    /// <summary>The metadata row identifying this instantiation.</summary>
    public MethodSpecificationHandle Handle { get; }
    /// <summary>The generic method declaration (MethodDef or MemberRef).</summary>
    public EntityHandle Method => module.Metadata.GetMethodSpecification(Handle).Method;
    /// <summary>The borrowed instantiation signature.</summary>
    public BlobReader Signature => module.Metadata.GetBlobReader(module.Metadata.GetMethodSpecification(Handle).Signature);
    /// <summary>Reads arguments in the caller's independent type and method contexts.</summary>
    public GenericArguments Read(GenericArguments? typeArguments = null, GenericArguments? methodArguments = null)
    {
        var reader = Signature;
        if (reader.ReadSignatureHeader().Kind != SignatureKind.MethodSpecification)
            throw new BadImageFormatException("Expected a method instantiation signature.");
        int count = reader.ReadCompressedInteger();
        if (count <= 0) throw new BadImageFormatException("Method instantiation requires arguments.");
        var values = new SignatureType[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = SignatureReader.Type(module, ref reader, typeArguments, methodArguments);
            if (values[i].Code == SignatureTypeCode.Void || values[i].IsByReference)
                throw new NotSupportedException("Void and byref method arguments are not supported.");
        }
        if (reader.RemainingBytes != 0) throw new BadImageFormatException("Unexpected trailing method instantiation bytes.");
        return new(values);
    }
}
