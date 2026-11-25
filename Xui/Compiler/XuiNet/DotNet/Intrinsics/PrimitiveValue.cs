using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Primitive values with specialized equality and hashing, including enum underlying storage.</summary>
public static class PrimitiveValue
{
    /// <summary>Tests the exact supported value representation; application structs are not substitutes.</summary>
    public static bool Supports(SignatureType element)
    {
        var storage = EnumInfo.Storage(element);
        return !element.IsByReference && (storage.Code is SignatureTypeCode.String or SignatureTypeCode.Boolean or SignatureTypeCode.Char or
            SignatureTypeCode.SByte or SignatureTypeCode.Byte or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16 or
            SignatureTypeCode.Int32 or SignatureTypeCode.UInt32 or SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 or
            SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr or SignatureTypeCode.Single or SignatureTypeCode.Double ||
            element.Identity is { } native && NativeFloat.Is(native));
    }
}
