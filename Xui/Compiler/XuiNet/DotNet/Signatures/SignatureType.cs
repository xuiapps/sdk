using System.Reflection.Metadata;

namespace Xui.Compiler.XuiNet.DotNet.Signatures;

/// <summary>A declared type, retaining named class/value-type identity and managed-byref qualification.</summary>
/// <remarks>Vector signatures retain recursively comparable element types, including jagged arrays.</remarks>
public readonly record struct SignatureType(
    SignatureTypeCode Code,
    TypeIdentity? Identity = null,
    bool IsByReference = false,
    bool IsReferenceType = false,
    int GenericParameterIndex = -1,
    bool IsReadOnlyByReference = false,
    ArraySignature? Array = null)
{
    /// <summary>Whether any nested type argument or array element is still unbound.</summary>
    public bool ContainsGenericParameters => Code is SignatureTypeCode.GenericTypeParameter or SignatureTypeCode.GenericMethodParameter ||
        Identity?.Arguments?.ContainsGenericParameters == true || Array?.Element.ContainsGenericParameters == true;
    internal int Depth => Array is { } array ? 1 + array.Element.Depth : Identity?.Arguments?.Depth ?? 0;

    /// <summary>Creates a vector signature with an exact element type.</summary>
    public static SignatureType Vector(SignatureType element) => element.IsByReference || element.Code == SignatureTypeCode.Void
        ? throw new BadImageFormatException("Invalid array element type.")
        : new(SignatureTypeCode.SZArray, IsReferenceType: true, Array: new(element));

    /// <summary>Creates the managed-address form of this type without losing its exact element type.</summary>
    public SignatureType ByReference() => IsByReference || Code == SignatureTypeCode.Void
        ? throw new BadImageFormatException("Invalid managed-byref element type.") : this with { IsByReference = true };

    /// <summary>A readable declared type, including its managed-address suffix.</summary>
    public override string ToString() => (Identity?.ToString() ?? (Code == SignatureTypeCode.GenericTypeParameter ? "!" + GenericParameterIndex :
        Code == SignatureTypeCode.GenericMethodParameter ? "!!" + GenericParameterIndex :
        Code == SignatureTypeCode.SZArray ? Array!.Element + "[]" : Code.ToString())) + (IsByReference ? "&" : "") + (IsReadOnlyByReference ? " readonly" : "");
}
