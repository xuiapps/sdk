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
            if (FrameworkTypes.Is(type, kind switch {
                ExceptionKind.TargetInvocationException => "System.Reflection",
                ExceptionKind.KeyNotFoundException => "System.Collections.Generic",
                _ => "System"
            }, kind.ToString())) return kind;
        return null;
    }

    /// <summary>Recognizes the CLI catch-all type used by an untyped C# catch.</summary>
    public static bool IsCatchAll(TypeIdentity type) => FrameworkTypes.Is(type, "System", "Object");

    /// <summary>Tests the supported nominal exception hierarchy.</summary>
    public static bool IsAssignable(ExceptionKind actual, ExceptionKind target) => actual == target ||
        target == ExceptionKind.ArithmeticException && actual is ExceptionKind.DivideByZeroException or ExceptionKind.OverflowException ||
        target == ExceptionKind.ArgumentException && actual is ExceptionKind.ArgumentNullException or ExceptionKind.ArgumentOutOfRangeException ||
        target == ExceptionKind.ApplicationException && actual == ExceptionKind.TargetInvocationException ||
        target == ExceptionKind.Exception || target == ExceptionKind.SystemException && actual is not (ExceptionKind.Exception or ExceptionKind.ApplicationException or ExceptionKind.TargetInvocationException);

    /// <summary>The default English message used when no message was supplied.</summary>
    public static string Message(ExceptionKind kind) => kind switch
    {
        ExceptionKind.TypeInitializationException => "The type initializer threw an exception.",
        ExceptionKind.KeyNotFoundException => "The given key was not present in the dictionary.",
        ExceptionKind.ArithmeticException => "Overflow or underflow in the arithmetic operation.",
        ExceptionKind.DivideByZeroException => "Attempted to divide by zero.",
        ExceptionKind.FormatException => "Input string was not in a correct format.",
        ExceptionKind.NotSupportedException => "Specified method is not supported.",
        ExceptionKind.Exception => "Exception of type 'System.Exception' was thrown.",
        ExceptionKind.SystemException => "System error.",
        ExceptionKind.InvalidCastException => "Specified cast is not valid.",
        ExceptionKind.NullReferenceException => "Object reference not set to an instance of an object.",
        ExceptionKind.InvalidOperationException => "Operation is not valid due to the current state of the object.",
        ExceptionKind.IndexOutOfRangeException => "Index was outside the bounds of the array.",
        ExceptionKind.OverflowException => "Arithmetic operation resulted in an overflow.",
        ExceptionKind.ArgumentOutOfRangeException => "Specified argument was out of the range of valid values.",
        ExceptionKind.ArgumentNullException => "Value cannot be null.",
        ExceptionKind.ArgumentException => "Value does not fall within the expected range.",
        ExceptionKind.ApplicationException => "Error in the application.",
        ExceptionKind.TargetInvocationException => "Exception has been thrown by the target of an invocation.",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    /// <summary>Whether a supported member constructs an exception.</summary>
    public static bool IsConstructor(ExceptionMember member) => member is not (ExceptionMember.Message or ExceptionMember.ParamName or ExceptionMember.InnerException or ExceptionMember.TypeName);

    /// <summary>Whether a member needs the parameter-name payload and message formatting.</summary>
    public static bool UsesParameterName(ExceptionMember member) => member is ExceptionMember.ParameterConstructor or
        ExceptionMember.ParameterMessageConstructor or ExceptionMember.MessageParameterConstructor or ExceptionMember.ParamName;

    /// <summary>Recognizes supported constructors, Message, and ArgumentException.ParamName.</summary>
    public static bool TryMember(Module module, EntityHandle token, out ExceptionKind kind, out ExceptionMember operation)
    {
        kind = default; operation = default;
        TypeIdentity owner;
        string name;
        BlobReader blob;
        if (token.Kind == HandleKind.MemberReference)
        {
            var member = module.GetMemberReference((MemberReferenceHandle)token);
            if (member.Kind != MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return false;
            owner = new(module, member.Parent); name = member.Name; blob = member.Signature;
        }
        else if (token.Kind == HandleKind.MethodDefinition)
        {
            var method = module.GetMethod((MethodDefinitionHandle)token);
            owner = new(module, method.DeclaringType); name = method.Name; blob = method.Signature;
        }
        else return false;
        if (Kind(owner) is not { } resolved) return false;
        kind = resolved;
        var signature = SignatureReader.Method(module, blob);
        if (!signature.HasThis || signature.GenericParameterCount != 0) return false;
        if (name == "get_InnerException" && signature.Parameters.Length == 0 &&
            signature.Result.Identity is { } inner && Kind(inner) == ExceptionKind.Exception)
        { operation = ExceptionMember.InnerException; return true; }
        // TargetInvocationException has no default or message-only public constructors.
        if (kind is ExceptionKind.TargetInvocationException or ExceptionKind.TypeInitializationException && name == ".ctor") return false;
        if (signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.String)
        {
            if (name == "get_Message") { operation = ExceptionMember.Message; return true; }
            if (name == "get_TypeName" && kind == ExceptionKind.TypeInitializationException)
            { operation = ExceptionMember.TypeName; return true; }
            if (name == "get_ParamName" && kind is ExceptionKind.ArgumentException or ExceptionKind.ArgumentNullException or ExceptionKind.ArgumentOutOfRangeException)
            { operation = ExceptionMember.ParamName; return true; }
        }
        if (name != ".ctor" || signature.Result.Code != SignatureTypeCode.Void) return false;
        if (signature.Parameters.Length == 0) return true;
        if (signature.Parameters.Any(p => p.Code != SignatureTypeCode.String || p.IsByReference)) return false;
        if (signature.Parameters.Length == 1)
        {
            operation = kind is ExceptionKind.ArgumentNullException or ExceptionKind.ArgumentOutOfRangeException ? ExceptionMember.ParameterConstructor : ExceptionMember.MessageConstructor;
            return true;
        }
        if (signature.Parameters.Length == 2 && kind is ExceptionKind.ArgumentException or ExceptionKind.ArgumentNullException or ExceptionKind.ArgumentOutOfRangeException)
        {
            operation = kind is ExceptionKind.ArgumentNullException or ExceptionKind.ArgumentOutOfRangeException ? ExceptionMember.ParameterMessageConstructor : ExceptionMember.MessageParameterConstructor;
            return true;
        }
        return false;
    }
}
