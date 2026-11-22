using System.Reflection;
using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A borrowed generic parameter declaration, including its index, flags, and constraint handles.</summary>
public readonly ref struct GenericParameter
{
    private readonly Module module;
    internal GenericParameter(Module module, GenericParameterHandle handle) { this.module = module; Handle = handle; }
    private System.Reflection.Metadata.GenericParameter Definition => module.Metadata.GetGenericParameter(Handle);
    public GenericParameterHandle Handle { get; }
    /// <summary>The zero-based index used by !n or !!n signature references.</summary>
    public int Index => Definition.Index;
    /// <summary>The source parameter name.</summary>
    public string Name => module.Metadata.GetString(Definition.Name);
    /// <summary>Variance and special constraint flags.</summary>
    public GenericParameterAttributes Attributes => Definition.Attributes;
    /// <summary>The declaring type or method.</summary>
    public EntityHandle Parent => Definition.Parent;
    /// <summary>Handles to declared type constraints.</summary>
    public GenericParameterConstraintHandleCollection Constraints => Definition.GetConstraints();
}
