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
            if (FrameworkTypes.Is(type, Namespace(kind), kind.ToString())) return kind;
        return null;
    }

    /// <summary>The namespace of a supported framework exception.</summary>
    public static string Namespace(ExceptionKind kind) => kind switch
    {
        ExceptionKind.EncoderFallbackException or ExceptionKind.DecoderFallbackException => "System.Text",
        ExceptionKind.IOException or ExceptionKind.EndOfStreamException or ExceptionKind.InvalidDataException => "System.IO",
        ExceptionKind.TargetInvocationException => "System.Reflection",
        ExceptionKind.MissingManifestResourceException => "System.Resources",
        ExceptionKind.XmlException => "System.Xml",
        ExceptionKind.JsonException => "System.Text.Json",
        ExceptionKind.SynchronizationLockException => "System.Threading",
        ExceptionKind.KeyNotFoundException => "System.Collections.Generic",
        _ => "System"
    };

    /// <summary>The stable full name used in diagnostic text.</summary>
    public static string TypeName(ExceptionKind kind) => Namespace(kind) + "." + kind;

    /// <summary>Recognizes the CLI catch-all type used by an untyped C# catch.</summary>
    public static bool IsCatchAll(TypeIdentity type) => FrameworkTypes.Is(type, "System", "Object");

    /// <summary>Tests the supported nominal exception hierarchy.</summary>
    public static bool IsAssignable(ExceptionKind actual, ExceptionKind target) => actual == target ||
        target == ExceptionKind.FormatException && actual == ExceptionKind.UriFormatException ||
        target == ExceptionKind.NotSupportedException && actual == ExceptionKind.PlatformNotSupportedException ||
        target == ExceptionKind.IOException && actual == ExceptionKind.EndOfStreamException ||
        target == ExceptionKind.InvalidOperationException && actual == ExceptionKind.ObjectDisposedException ||
        target == ExceptionKind.ArithmeticException && actual is ExceptionKind.DivideByZeroException or ExceptionKind.OverflowException ||
        target == ExceptionKind.ArgumentException && actual is ExceptionKind.ArgumentNullException or ExceptionKind.ArgumentOutOfRangeException or ExceptionKind.EncoderFallbackException or ExceptionKind.DecoderFallbackException ||
        target == ExceptionKind.ApplicationException && actual == ExceptionKind.TargetInvocationException ||
        target == ExceptionKind.MissingMemberException && actual is ExceptionKind.MissingMethodException or ExceptionKind.MissingFieldException ||
        target == ExceptionKind.MemberAccessException && actual is ExceptionKind.MissingMemberException or ExceptionKind.MissingMethodException or ExceptionKind.MissingFieldException ||
        target == ExceptionKind.Exception || target == ExceptionKind.SystemException && actual is not (ExceptionKind.Exception or ExceptionKind.ApplicationException or ExceptionKind.TargetInvocationException or ExceptionKind.AggregateException or ExceptionKind.JsonException);

    /// <summary>The default English message used when no message was supplied.</summary>
    public static string Message(ExceptionKind kind) => kind switch
    {
        ExceptionKind.UriFormatException => "One of the identified items was in an invalid format.",
        ExceptionKind.InsufficientExecutionStackException => "Insufficient stack to continue executing the program safely.",
        ExceptionKind.JsonException => "Exception of type 'System.Text.Json.JsonException' was thrown.",
        ExceptionKind.PlatformNotSupportedException => "Operation is not supported on this platform.",
        ExceptionKind.XmlException => "An XML error has occurred.",
        ExceptionKind.MissingManifestResourceException => "Unable to find manifest resource.",
        ExceptionKind.OutOfMemoryException => "Insufficient memory to continue the execution of the program.",
        ExceptionKind.EncoderFallbackException or ExceptionKind.DecoderFallbackException => "Value does not fall within the expected range.",
        ExceptionKind.IOException => "I/O error occurred.",
        ExceptionKind.EndOfStreamException => "Attempted to read past the end of the stream.",
        ExceptionKind.InvalidDataException => "Found invalid data while decoding.",
        ExceptionKind.ObjectDisposedException => "Cannot access a disposed object.",
        ExceptionKind.AggregateException => "One or more errors occurred.",
        ExceptionKind.SynchronizationLockException => "Object synchronization method was called from an unsynchronized block of code.",
        ExceptionKind.TypeInitializationException => "The type initializer threw an exception.",
        ExceptionKind.MemberAccessException => "Cannot access member.",
        ExceptionKind.MissingMemberException => "Attempted to access a missing member.",
        ExceptionKind.MissingMethodException => "Attempted to access a missing method.",
        ExceptionKind.MissingFieldException => "Attempted to access a non-existing field.",
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
    public static bool IsConstructor(ExceptionMember member) => member is not (ExceptionMember.Text or ExceptionMember.Message or ExceptionMember.ParamName or ExceptionMember.InnerException or ExceptionMember.TypeName or ExceptionMember.ActualValue or ExceptionMember.XmlLineNumber or ExceptionMember.XmlLinePosition or ExceptionMember.XmlSourceUri or ExceptionMember.XmlResource);

    /// <summary>Whether a member needs the parameter-name payload and message formatting.</summary>
    public static bool UsesParameterName(ExceptionMember member) => member is ExceptionMember.ParameterConstructor or
        ExceptionMember.ParameterMessageConstructor or ExceptionMember.ParameterValueMessageConstructor or ExceptionMember.MessageParameterConstructor or ExceptionMember.ParamName;

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
        if (kind == ExceptionKind.XmlException && XmlExceptionMembers.Read(name, signature) is { } xml)
        { operation = xml; return true; }
        if (name == "get_InnerException" && signature.Parameters.Length == 0 &&
            signature.Result.Identity is { } inner && Kind(inner) == ExceptionKind.Exception)
        { operation = ExceptionMember.InnerException; return true; }
        // TargetInvocationException has no default or message-only public constructors.
        if (kind is ExceptionKind.TargetInvocationException or ExceptionKind.TypeInitializationException && name == ".ctor") return false;
        if (signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.String)
        {
            if (name == "ToString") { operation = ExceptionMember.Text; return true; }
            if (name == "get_Message") { operation = ExceptionMember.Message; return true; }
            if (name == "get_TypeName" && kind == ExceptionKind.TypeInitializationException)
            { operation = ExceptionMember.TypeName; return true; }
            if (name == "get_ObjectName" && kind == ExceptionKind.ObjectDisposedException) { operation = ExceptionMember.ParamName; return true; }
            if (name == "get_ParamName" && kind is ExceptionKind.ArgumentException or ExceptionKind.ArgumentNullException or ExceptionKind.ArgumentOutOfRangeException)
            { operation = ExceptionMember.ParamName; return true; }
        }
        if (name == "get_ActualValue" && kind == ExceptionKind.ArgumentOutOfRangeException && signature.Parameters.Length == 0 && signature.Result.Code == SignatureTypeCode.Object)
        { operation = ExceptionMember.ActualValue; return true; }
        if (name != ".ctor" || signature.Result.Code != SignatureTypeCode.Void) return false;
        if (kind == ExceptionKind.ArgumentOutOfRangeException && signature.Parameters.Length == 3 &&
            signature.Parameters[0] == new SignatureType(SignatureTypeCode.String) &&
            signature.Parameters[1].Code == SignatureTypeCode.Object && !signature.Parameters[1].IsByReference &&
            signature.Parameters[2] == new SignatureType(SignatureTypeCode.String))
        { operation = ExceptionMember.ParameterValueMessageConstructor; return true; }
        if (signature.Parameters.Length == 2 && signature.Parameters[0] == new SignatureType(SignatureTypeCode.String) && !signature.Parameters[1].IsByReference &&
            signature.Parameters[1].Identity is { } exceptionType && Kind(exceptionType) == ExceptionKind.Exception)
        { operation = ExceptionMember.MessageInnerConstructor; return true; }
        if (signature.Parameters.Length == 0) return true;
        if (signature.Parameters.Any(p => p.Code != SignatureTypeCode.String || p.IsByReference)) return false;
        if (signature.Parameters.Length == 1)
        {
            operation = kind is ExceptionKind.ArgumentNullException or ExceptionKind.ArgumentOutOfRangeException or ExceptionKind.ObjectDisposedException ? ExceptionMember.ParameterConstructor : ExceptionMember.MessageConstructor;
            return true;
        }
        if (signature.Parameters.Length == 2 && kind is ExceptionKind.ArgumentException or ExceptionKind.ArgumentNullException or ExceptionKind.ArgumentOutOfRangeException or ExceptionKind.ObjectDisposedException)
        {
            operation = kind is ExceptionKind.ArgumentNullException or ExceptionKind.ArgumentOutOfRangeException or ExceptionKind.ObjectDisposedException ? ExceptionMember.ParameterMessageConstructor : ExceptionMember.MessageParameterConstructor;
            return true;
        }
        return false;
    }
}
