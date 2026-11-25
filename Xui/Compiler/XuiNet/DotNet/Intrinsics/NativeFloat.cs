using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Identity/signature-checked NFloat intrinsics. Metadata stays nominal; code analysis uses the floating stack category.</summary>
public static class NativeFloat
{
    /// <summary>Recognizes the framework native floating-point value type.</summary>
    public static bool Is(TypeIdentity type) => FrameworkTypes.Is(type, "System.Runtime.InteropServices", "NFloat");

    /// <summary>Recognizes scalar conversions, arithmetic, and typed instance equality with checked signatures.</summary>
    public static bool TryMethod(Module module, EntityHandle token, out string operation, out MethodSignature signature)
    {
        operation = ""; signature = default!;
        if (token.Kind != HandleKind.MemberReference) return false;
        var member = module.GetMemberReference((MemberReferenceHandle)token);
        if (member.Kind != MemberReferenceKind.Method || !Is(new(module, member.Parent))) return false;
        signature = SignatureReader.Method(module, member.Signature);
        operation = member.Name;
        bool N(SignatureType type) => type.Identity is { } identity && Is(identity);
        if (signature.HasThis) return operation == "Equals" && signature.Parameters.Length == 1 &&
            N(signature.Parameters[0]) && !signature.Parameters[0].IsByReference && signature.Result.Code == SignatureTypeCode.Boolean;
        if (signature.Parameters.Any(p => p.IsByReference) || signature.Result.IsByReference) return false;
        bool Scalar(SignatureType type) => type.Identity is null && type.Code is
            SignatureTypeCode.SByte or SignatureTypeCode.Byte or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 or
            SignatureTypeCode.Char or SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.Int64 or
            SignatureTypeCode.UInt64 or SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr or SignatureTypeCode.Single or SignatureTypeCode.Double;
        return operation switch
        {
            "op_Implicit" or "op_Explicit" => signature.Parameters.Length == 1 &&
                (N(signature.Result) && Scalar(signature.Parameters[0]) || N(signature.Parameters[0]) && signature.Result.Identity is null && signature.Result.Code is SignatureTypeCode.Single or SignatureTypeCode.Double),
            "IsFinite" or "IsNaN" or "IsInfinity" => signature.Parameters.Length == 1 && N(signature.Parameters[0]) && signature.Result.Code == SignatureTypeCode.Boolean,
            "Abs" or "op_UnaryNegation" => signature.Parameters.Length == 1 && N(signature.Parameters[0]) && N(signature.Result),
            "Clamp" => signature.Parameters.Length == 3 && signature.Parameters.All(N) && N(signature.Result),
            "Min" or "Max" => signature.Parameters.Length == 2 && signature.Parameters.All(N) && N(signature.Result),
            "op_Addition" or "op_Subtraction" or "op_Multiply" or "op_Division" => signature.Parameters.Length == 2 && N(signature.Result) && signature.Parameters.All(N),
            "op_Equality" or "op_Inequality" or "op_LessThan" or "op_LessThanOrEqual" or "op_GreaterThan" or "op_GreaterThanOrEqual" =>
                signature.Parameters.Length == 2 && signature.Result.Code == SignatureTypeCode.Boolean && signature.Parameters.All(N),
            "get_PositiveInfinity" or "get_NegativeInfinity" or "get_NaN" or "get_Pi" => signature.Parameters.Length == 0 && N(signature.Result),
            _ => false
        };
    }
}
