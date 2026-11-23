using System.Reflection;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A borrowed view of a method declaration, signature, and optional body.</summary>
/// <remarks>Keep the owning assembly undisposed while using this view or any blob readers returned by it.</remarks>
public readonly ref struct MethodDefinition
{
    private readonly Module module;

    internal MethodDefinition(Module module, MethodDefinitionHandle handle)
    {
        this.module = module;
        Handle = handle;
    }

    /// <summary>Generic parameter declaration handles in metadata order.</summary>
    public GenericParameterHandleCollection GenericParameters => Definition.GetGenericParameters();

    /// <summary>The metadata handle within this module.</summary>
    public MethodDefinitionHandle Handle { get; }

    private System.Reflection.Metadata.MethodDefinition Definition => module.Metadata.GetMethodDefinition(Handle);

    /// <summary>The declaring type handle.</summary>
    public TypeDefinitionHandle DeclaringType => Definition.GetDeclaringType();

    /// <summary>The method name, decoded on access.</summary>
    public string Name => module.Metadata.GetString(Definition.Name);

    /// <summary>The method flags.</summary>
    public MethodAttributes Attributes => Definition.Attributes;

    /// <summary>The method implementation flags.</summary>
    public MethodImplAttributes ImplementationAttributes => Definition.ImplAttributes;

    /// <summary>A borrowed reader over the encoded method signature.</summary>
    public BlobReader Signature => module.Metadata.GetBlobReader(Definition.Signature);

    /// <summary>The cached deferred body, or null for an abstract or external declaration.</summary>
    public MethodBody? Body => module.GetBody(Handle);
}
