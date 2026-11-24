using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet;

/// <summary>Identifies concrete delegate declarations whose constructor and Invoke are runtime-provided.</summary>
public static class DelegateInfo
{
    /// <summary>Whether the linked type directly derives from the framework MulticastDelegate marker.</summary>
    public static bool Is(TypeIdentity type) => type.Handle.Kind == HandleKind.TypeDefinition &&
        type.BaseType is { } parent && FrameworkTypes.Is(parent, "System", "MulticastDelegate");

    /// <summary>Whether the type is an unsupported framework delegate base API.</summary>
    public static bool IsRuntimeBase(TypeIdentity type) => FrameworkTypes.Is(type, "System", "Delegate") ||
        FrameworkTypes.Is(type, "System", "MulticastDelegate");

    /// <summary>The closed Invoke declaration, retaining the delegate's generic context.</summary>
    public static MethodIdentity Invoke(TypeIdentity type)
    {
        if (!Is(type)) throw new NotSupportedException($"Expected a linked delegate definition: {type}.");
        foreach (var handle in type.Module.GetType((TypeDefinitionHandle)type.Handle).Methods)
            if (type.Module.GetMethod(handle).Name == "Invoke") return new(type, handle);
        throw new BadImageFormatException($"Delegate has no Invoke declaration: {type}.");
    }

    /// <summary>Resolves a delegate member without treating its runtime body as managed IL.</summary>
    public static MethodIdentity? Member(Module module, EntityHandle token, MethodIdentity caller)
    {
        TypeIdentity? owner = token.Kind switch
        {
            HandleKind.MethodDefinition => new(module, module.GetMethod((MethodDefinitionHandle)token).DeclaringType),
            HandleKind.MemberReference => SignatureReader.Resolve(module, module.GetMemberReference((MemberReferenceHandle)token).Parent,
                caller.DeclaringType.Arguments, caller.Arguments).Identity,
            _ => null
        };
        if (owner is not { } type || !Is(type)) return null;
        return MemberResolver.Method(module, token, caller.DeclaringType, caller.Arguments);
    }
}
