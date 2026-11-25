using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Xui.Compiler.XuiNet.Reachability;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Default equality for named classes that use the Object.Equals contract.</summary>
public static class ObjectEquality
{
    /// <summary>Typed IEquatable class implementations and boxed/object-typed values need a separate path.</summary>
    public static bool Supports(SignatureType element) => !element.IsByReference &&
        element.Identity is { Handle.Kind: HandleKind.TypeDefinition, IsValueType: false, IsInterface: false } type &&
        !FrameworkTypes.IsRuntimeBase(type) && !ManagedType.Is(type) && !DelegateInfo.Is(type) &&
        !type.Interfaces().Any(i => FrameworkTypes.Is(i, "System", "IEquatable`1") && i.Arguments?.Values.Length == 1 && i.Arguments.Values[0] == element);

    /// <summary>Finds the override of Object.Equals, ignoring hidden new virtual Equals slots.</summary>
    public static MethodIdentity? Implementation(IReadOnlyList<VirtualSlot> slots)
    {
        foreach (var slot in slots)
        {
            var declaration = slot.Declaration;
            var method = declaration.DeclaringType.Module.GetMethod(declaration.Handle);
            if (method.Name != "Equals" || (method.Attributes & MethodAttributes.NewSlot) != 0) continue;
            var signature = SignatureReader.Method(declaration.DeclaringType.Module, method.Signature, declaration.DeclaringType.Arguments);
            if (signature.HasThis && signature.Result.Code == SignatureTypeCode.Boolean && signature.Parameters.Length == 1 &&
                signature.Parameters[0].Code == SignatureTypeCode.Object && !signature.Parameters[0].IsByReference)
                return slot.Implementation;
        }
        return null;
    }
}
