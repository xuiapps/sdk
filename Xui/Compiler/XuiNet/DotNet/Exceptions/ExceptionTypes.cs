using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Exceptions;

/// <summary>Identifies the small built-in exception surface by assembly, type, and member signature.</summary>
public static class ExceptionTypes
{
    /// <summary>Returns the built-in kind for a framework exception reference, or null.</summary>
    public static ExceptionKind? Kind(TypeIdentity type)
    {
        if (type.Handle.Kind != HandleKind.TypeReference) return null;
        var reference = type.Module.GetTypeReference((TypeReferenceHandle)type.Handle);
        if (reference.Namespace != "System" || reference.ResolutionScope.Kind != HandleKind.AssemblyReference ||
            type.Module.GetAssemblyReference((AssemblyReferenceHandle)reference.ResolutionScope).Name is not
                ("System.Runtime" or "System.Private.CoreLib" or "mscorlib")) return null;
        return Enum.TryParse<ExceptionKind>(reference.Name, out var kind) && Enum.IsDefined(kind) ? kind : null;
    }

    /// <summary>Recognizes the CLI catch-all type used by an untyped C# catch.</summary>
    public static bool IsCatchAll(TypeIdentity type)
    {
        if (type.Handle.Kind != HandleKind.TypeReference) return false;
        var reference = type.Module.GetTypeReference((TypeReferenceHandle)type.Handle);
        return reference.Namespace == "System" && reference.Name == "Object" &&
            reference.ResolutionScope.Kind == HandleKind.AssemblyReference &&
            type.Module.GetAssemblyReference((AssemblyReferenceHandle)reference.ResolutionScope).Name is
                "System.Runtime" or "System.Private.CoreLib" or "mscorlib";
    }

    /// <summary>Tests the supported nominal exception hierarchy.</summary>
    public static bool IsAssignable(ExceptionKind actual, ExceptionKind target) => actual == target ||
        target == ExceptionKind.Exception || target == ExceptionKind.SystemException && actual != ExceptionKind.Exception;

    /// <summary>The default English message used when no message was supplied.</summary>
    public static string Message(ExceptionKind kind) => kind switch
    {
        ExceptionKind.Exception => "Exception of type 'System.Exception' was thrown.",
        ExceptionKind.SystemException => "System error.",
        ExceptionKind.InvalidCastException => "Specified cast is not valid.",
        ExceptionKind.NullReferenceException => "Object reference not set to an instance of an object.",
        ExceptionKind.InvalidOperationException => "Operation is not valid due to the current state of the object.",
        ExceptionKind.ArgumentException => "Value does not fall within the expected range.",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    /// <summary>Recognizes parameterless/string constructors and the Message getter.</summary>
    public static bool TryMember(Module module, EntityHandle token, out ExceptionKind kind, out bool constructor, out bool messageArgument)
    {
        kind = default; constructor = messageArgument = false;
        if (token.Kind != HandleKind.MemberReference) return false;
        var member = module.GetMemberReference((MemberReferenceHandle)token);
        if (member.Parent.Kind != HandleKind.TypeReference || Kind(new(module, member.Parent)) is not { } resolved) return false;
        kind = resolved;
        var signature = SignatureReader.Method(module, member.Signature);
        if (!signature.HasThis) return false;
        if (member.Name == "get_Message" && signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.String) return true;
        if (member.Name != ".ctor" || signature.Result.Code != SignatureTypeCode.Void) return false;
        constructor = true;
        messageArgument = signature.Parameters.Length == 1 && signature.Parameters[0].Code == SignatureTypeCode.String;
        return signature.Parameters.Length == 0 || messageArgument;
    }
}
