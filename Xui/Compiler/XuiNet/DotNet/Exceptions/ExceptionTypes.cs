using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Exceptions;

/// <summary>Identifies the small built-in exception surface by assembly, type, and member signature.</summary>
public static class ExceptionTypes
{
    /// <summary>Returns the built-in kind for a framework exception reference or resolved definition, or null.</summary>
    public static ExceptionKind? Kind(TypeIdentity type)
    {
        foreach (var kind in Enum.GetValues<ExceptionKind>())
            if (FrameworkTypes.Is(type, "System", kind.ToString())) return kind;
        return null;
    }

    /// <summary>Recognizes the CLI catch-all type used by an untyped C# catch.</summary>
    public static bool IsCatchAll(TypeIdentity type) => FrameworkTypes.Is(type, "System", "Object");

    /// <summary>Tests the supported nominal exception hierarchy.</summary>
    public static bool IsAssignable(ExceptionKind actual, ExceptionKind target) => actual == target ||
        target == ExceptionKind.ArgumentException && actual == ExceptionKind.ArgumentNullException ||
        target == ExceptionKind.Exception || target == ExceptionKind.SystemException && actual != ExceptionKind.Exception;

    /// <summary>The default English message used when no message was supplied.</summary>
    public static string Message(ExceptionKind kind) => kind switch
    {
        ExceptionKind.Exception => "Exception of type 'System.Exception' was thrown.",
        ExceptionKind.SystemException => "System error.",
        ExceptionKind.InvalidCastException => "Specified cast is not valid.",
        ExceptionKind.NullReferenceException => "Object reference not set to an instance of an object.",
        ExceptionKind.InvalidOperationException => "Operation is not valid due to the current state of the object.",
        ExceptionKind.IndexOutOfRangeException => "Index was outside the bounds of the array.",
        ExceptionKind.OverflowException => "Arithmetic operation resulted in an overflow.",
        ExceptionKind.ArgumentNullException => "Value cannot be null.",
        ExceptionKind.ArgumentException => "Value does not fall within the expected range.",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    /// <summary>Whether a supported member constructs an exception.</summary>
    public static bool IsConstructor(ExceptionMember member) => member is not (ExceptionMember.Message or ExceptionMember.ParamName);

    /// <summary>Whether a member needs the parameter-name payload and message formatting.</summary>
    public static bool UsesParameterName(ExceptionMember member) => member is ExceptionMember.ParameterConstructor or
        ExceptionMember.ParameterMessageConstructor or ExceptionMember.MessageParameterConstructor or ExceptionMember.ParamName;

    /// <summary>Recognizes supported constructors, Message, and ArgumentException.ParamName.</summary>
    public static bool TryMember(Module module, EntityHandle token, out ExceptionKind kind, out ExceptionMember operation)
    {
        kind = default; operation = default;
        if (token.Kind != HandleKind.MemberReference) return false;
        var member = module.GetMemberReference((MemberReferenceHandle)token);
        if (member.Parent.Kind != HandleKind.TypeReference || Kind(new(module, member.Parent)) is not { } resolved) return false;
        kind = resolved;
        var signature = SignatureReader.Method(module, member.Signature);
        if (!signature.HasThis) return false;
        if (signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.String)
        {
            if (member.Name == "get_Message") { operation = ExceptionMember.Message; return true; }
            if (member.Name == "get_ParamName" && kind is ExceptionKind.ArgumentException or ExceptionKind.ArgumentNullException)
            { operation = ExceptionMember.ParamName; return true; }
        }
        if (member.Name != ".ctor" || signature.Result.Code != SignatureTypeCode.Void) return false;
        if (signature.Parameters.Length == 0) return true;
        if (signature.Parameters.Any(p => p.Code != SignatureTypeCode.String || p.IsByReference)) return false;
        if (signature.Parameters.Length == 1)
        {
            operation = kind == ExceptionKind.ArgumentNullException ? ExceptionMember.ParameterConstructor : ExceptionMember.MessageConstructor;
            return true;
        }
        if (signature.Parameters.Length == 2 && kind is ExceptionKind.ArgumentException or ExceptionKind.ArgumentNullException)
        {
            operation = kind == ExceptionKind.ArgumentNullException ? ExceptionMember.ParameterMessageConstructor : ExceptionMember.MessageParameterConstructor;
            return true;
        }
        return false;
    }
}
