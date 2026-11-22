using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A deferred view of a method body at an RVA in its assembly's retained PE image.</summary>
/// <remarks>Keep the owning assembly alive until all body reads are complete.</remarks>
public sealed class MethodBody
{
    private readonly PEReader pe;
    private MethodBodyBlock? body;

    private MethodBody(PEReader pe, int relativeVirtualAddress)
    {
        this.pe = pe;
        RelativeVirtualAddress = relativeVirtualAddress;
    }

    /// <summary>Creates a deferred IL body, or returns null for a method without a body.</summary>
    internal static MethodBody? CreateBody(PEReader pe, MetadataReader metadata,
        System.Reflection.Metadata.MethodDefinition method)
    {
        if (method.RelativeVirtualAddress == 0) return null;
        if ((method.ImplAttributes & MethodImplAttributes.CodeTypeMask) != MethodImplAttributes.IL)
            throw new NotSupportedException($"Non-IL method body: {metadata.GetString(method.Name)}");
        return new MethodBody(pe, method.RelativeVirtualAddress);
    }

    /// <summary>The body's relative virtual address in the PE image, not a file offset.</summary>
    public int RelativeVirtualAddress { get; }

    private MethodBodyBlock Body => body ??= pe.GetMethodBody(RelativeVirtualAddress);

    /// <summary>The maximum evaluation stack depth declared by the method.</summary>
    public int MaxStack => Body.MaxStack;

    /// <summary>Whether the method requests initialized local variables.</summary>
    public bool InitLocals => Body.LocalVariablesInitialized;

    /// <summary>A borrowed reader over the encoded local signature, without copying its bytes.</summary>
    public BlobReader LocalSignature
    {
        get
        {
            var handle = Body.LocalSignature;
            var metadata = pe.GetMetadataReader();
            return handle.IsNil ? default :
                metadata.GetBlobReader(metadata.GetStandaloneSignature(handle).Signature);
        }
    }

    /// <summary>Creates an independent cursor for reading opcodes and explicitly consuming their typed operands.</summary>
    /// <remarks>The cursor and switch targets borrow the owning assembly's PE image.</remarks>
    public ILReader Instructions
    {
        get
        {
            _ = pe.GetSectionData(RelativeVirtualAddress); // Check the retained PE is still alive, even for a cached body block.
            return new ILReader(Body.GetILReader());
        }
    }

    /// <summary>Reads exception regions on demand without copying the underlying method block's array.</summary>
    public ImmutableArray<ExceptionRegion> ExceptionRegions => Body.ExceptionRegions;
}
