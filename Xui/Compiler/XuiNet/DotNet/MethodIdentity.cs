using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>A method definition in its declaring type's specialization context.</summary>
public readonly record struct MethodIdentity(
    TypeIdentity DeclaringType,
    MethodDefinitionHandle Handle,
    GenericArguments? Arguments = null,
    ConstantBooleanArgument? ConstantBoolean = null)
{
    /// <summary>Creates the identity of a method on its unconstructed declaring type.</summary>
    public static MethodIdentity FromDefinition(Module module, MethodDefinitionHandle handle) =>
        new(new(module, module.GetMethod(handle).DeclaringType), handle);

    public override string ToString() => DeclaringType + "." + DeclaringType.Module.GetMethod(Handle).Name + (Arguments is null ? "" : "<" + string.Join(", ", Arguments.Values.ToArray()) + ">") + (ConstantBoolean is { } argument ? $"[arg{argument.Index}={argument.Value}]" : "");
}
