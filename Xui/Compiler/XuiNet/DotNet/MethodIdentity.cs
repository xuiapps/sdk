using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A method definition in its declaring type's specialization context.</summary>
/// <remarks>Generic method arguments will be added when MethodSpec compilation is implemented.</remarks>
public readonly record struct MethodIdentity(
    TypeIdentity DeclaringType,
    MethodDefinitionHandle Handle)
{
    /// <summary>Creates the identity of a method on its unconstructed declaring type.</summary>
    public static MethodIdentity FromDefinition(Module module, MethodDefinitionHandle handle) =>
        new(new(module, module.GetMethod(handle).DeclaringType), handle);

    public override string ToString() => DeclaringType + "." + DeclaringType.Module.GetMethod(Handle).Name;
}
