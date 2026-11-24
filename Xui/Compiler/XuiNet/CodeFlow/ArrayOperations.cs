using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Checks the storage category selected by an MSIL vector element instruction.</summary>
public static class ArrayOperations
{
    public static void Validate(OpCode code, SignatureType element, SignatureType? explicitElement = null)
    {
        var storage = DotNet.EnumInfo.Storage(element);
        bool valid = code switch
        {
            OpCode.Ldelema or OpCode.Ldelem or OpCode.Stelem => element == explicitElement,
            OpCode.Ldelem_i1 or OpCode.Ldelem_u1 or OpCode.Stelem_i1 => storage.Code is SignatureTypeCode.Boolean or SignatureTypeCode.SByte or SignatureTypeCode.Byte,
            OpCode.Ldelem_i2 or OpCode.Ldelem_u2 or OpCode.Stelem_i2 => storage.Code is SignatureTypeCode.Char or SignatureTypeCode.Int16 or SignatureTypeCode.UInt16,
            OpCode.Ldelem_i4 or OpCode.Ldelem_u4 or OpCode.Stelem_i4 => storage.Code is SignatureTypeCode.Int32 or SignatureTypeCode.UInt32,
            OpCode.Ldelem_i8 or OpCode.Stelem_i8 => storage.Code is SignatureTypeCode.Int64 or SignatureTypeCode.UInt64,
            OpCode.Ldelem_i or OpCode.Stelem_i => storage.Code is SignatureTypeCode.IntPtr or SignatureTypeCode.UIntPtr,
            OpCode.Ldelem_r4 or OpCode.Stelem_r4 => storage.Code == SignatureTypeCode.Single,
            OpCode.Ldelem_r8 or OpCode.Stelem_r8 => storage.Code == SignatureTypeCode.Double,
            OpCode.Ldelem_ref or OpCode.Stelem_ref => element.IsReferenceType || element.Code is SignatureTypeCode.String or SignatureTypeCode.Object or SignatureTypeCode.SZArray,
            _ => false
        };
        if (!valid) throw new InvalidProgramException($"{code} does not match array element {element}.");
    }
}
