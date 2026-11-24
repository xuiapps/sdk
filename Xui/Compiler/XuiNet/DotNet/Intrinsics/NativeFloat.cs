using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Identity/signature-checked NFloat intrinsics. Metadata stays nominal; code analysis uses the floating stack category.</summary>
public static class NativeFloat
{
    /// <summary>Recognizes the framework native floating-point value type.</summary>
    public static bool Is(TypeIdentity type) => FrameworkTypes.Is(type, "System.Runtime.InteropServices", "NFloat");

    /// <summary>Recognizes scalar conversions and basic arithmetic, excluding instance methods and unchecked signatures.</summary>
    public static bool TryMethod(Module module, EntityHandle token, out string operation, out MethodSignature signature)
    {
        operation = ""; signature = default!;
        if (token.Kind != HandleKind.MemberReference) return false;
        var member = module.GetMemberReference((MemberReferenceHandle)token);
        if (member.Kind != MemberReferenceKind.Method || !Is(new(module, member.Parent))) return false;
        signature = SignatureReader.Method(module, member.Signature);
        if (signature.HasThis || signature.Parameters.Any(p => p.IsByReference) || signature.Result.IsByReference) return false;
        operation = member.Name;
        bool N(SignatureType type) => type.Identity is { } identity && Is(identity);
        bool Scalar(SignatureType type) => type.Identity is null && type.Code is SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.Single or SignatureTypeCode.Double;
        return operation switch
        {
            "op_Implicit" or "op_Explicit" => signature.Parameters.Length == 1 &&
                (N(signature.Result) && Scalar(signature.Parameters[0]) || N(signature.Parameters[0]) && signature.Result.Identity is null && signature.Result.Code is SignatureTypeCode.Single or SignatureTypeCode.Double),
            "op_Addition" or "op_Subtraction" or "op_Multiply" or "op_Division" => signature.Parameters.Length == 2 && N(signature.Result) && signature.Parameters.All(N),
            "op_Equality" or "op_Inequality" or "op_LessThan" or "op_LessThanOrEqual" or "op_GreaterThan" or "op_GreaterThanOrEqual" =>
                signature.Parameters.Length == 2 && signature.Result.Code == SignatureTypeCode.Boolean && signature.Parameters.All(N),
            "get_PositiveInfinity" or "get_NegativeInfinity" or "get_NaN" => signature.Parameters.Length == 0 && N(signature.Result),
            _ => false
        };
    }
}
