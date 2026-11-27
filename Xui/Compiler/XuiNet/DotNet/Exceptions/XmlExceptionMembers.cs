using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Exceptions;

/// <summary>XML error constructors carrying messages, resource arguments and source positions.</summary>
public static class XmlExceptionMembers
{
    public static ExceptionMember? Read(string name, MethodSignature signature)
    {
        var parameters = signature.Parameters;
        if (parameters.Length == 0)
        {
            if (signature.Result.Code == SignatureTypeCode.Int32)
                return name switch { "get_LineNumber" => ExceptionMember.XmlLineNumber, "get_LinePosition" => ExceptionMember.XmlLinePosition, _ => null };
            if (signature.Result.Code == SignatureTypeCode.String)
                return name switch { "get_SourceUri" => ExceptionMember.XmlSourceUri, "get_ResString" => ExceptionMember.XmlResource, _ => null };
        }
        if (name != ".ctor" || signature.Result.Code != SignatureTypeCode.Void) return null;
        if (parameters.Length == 0) return ExceptionMember.XmlConstructor;
        if (parameters.Any(p => p.IsByReference) || parameters[0].Code != SignatureTypeCode.String) return null;
        if (parameters.Length == 1) return ExceptionMember.XmlConstructor;
        int next = 2;
        bool resource = IsResource(signature);
        if (!resource && !IsException(parameters[1])) return null;
        if (resource && parameters.Length > next && IsException(parameters[next])) next++;
        if (parameters.Length >= next + 2 && parameters[next].Code == SignatureTypeCode.Int32 && parameters[next + 1].Code == SignatureTypeCode.Int32)
            next += 2;
        if (parameters.Length == next || parameters.Length == next + 1 && parameters[next].Code == SignatureTypeCode.String)
            return ExceptionMember.XmlConstructor;
        return null;
    }

    public static bool IsResource(MethodSignature signature) => signature.Parameters.Length > 1 &&
        (signature.Parameters[1].Code == SignatureTypeCode.String ||
         signature.Parameters[1].Code == SignatureTypeCode.SZArray && signature.Parameters[1].Array?.Element.Code == SignatureTypeCode.String);

    public static bool IsException(SignatureType type) => type.Identity is { } identity && ExceptionTypes.Kind(identity) == ExceptionKind.Exception;
}
