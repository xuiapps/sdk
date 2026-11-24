using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Signatures;

/// <summary>A declared type, retaining named class/value-type identity and managed-byref qualification.</summary>
/// <remarks>SZArray currently represents only string[], used by entry-point signatures.</remarks>
public readonly record struct SignatureType(
    SignatureTypeCode Code,
    TypeIdentity? Identity = null,
    bool IsByReference = false,
    bool IsReferenceType = false,
    int GenericParameterIndex = -1,
    bool IsReadOnlyByReference = false)
{
    /// <summary>Creates the managed-address form of this type without losing its exact element type.</summary>
    public SignatureType ByReference() => IsByReference || Code == SignatureTypeCode.Void
        ? throw new BadImageFormatException("Invalid managed-byref element type.") : this with { IsByReference = true };

    /// <summary>A readable declared type, including its managed-address suffix.</summary>
    public override string ToString() => (Identity?.ToString() ?? (Code == SignatureTypeCode.GenericTypeParameter ? "!" + GenericParameterIndex :
        Code == SignatureTypeCode.GenericMethodParameter ? "!!" + GenericParameterIndex :
        Code == SignatureTypeCode.SZArray ? "String[]" : Code.ToString())) + (IsByReference ? "&" : "") + (IsReadOnlyByReference ? " readonly" : "");
}
