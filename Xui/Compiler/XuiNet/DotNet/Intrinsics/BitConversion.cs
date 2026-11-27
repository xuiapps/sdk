using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Scalar BitConverter operations represented directly by WASM reinterpret instructions.</summary>
public sealed record BitConversion(
    SignatureType Input,
    SignatureType Result)
{
    public static BitConversion? Read(Module module, EntityHandle token)
    {
        TypeIdentity owner;string name;BlobReader blob;
        if(token.Kind==HandleKind.MemberReference)
        {
            var member=module.GetMemberReference((MemberReferenceHandle)token);
            if(member.Kind!=MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition))return null;
            owner=new(module,member.Parent);name=member.Name;blob=member.Signature;
        }
        else if(token.Kind==HandleKind.MethodDefinition)
        {var method=module.GetMethod((MethodDefinitionHandle)token);owner=new(module,method.DeclaringType);name=method.Name;blob=method.Signature;}
        else return null;
        if(!FrameworkTypes.Is(owner,"System","BitConverter"))return null;
        (SignatureTypeCode input,SignatureTypeCode result)=name switch {
            "SingleToInt32Bits" => (SignatureTypeCode.Single,SignatureTypeCode.Int32),
            "SingleToUInt32Bits" => (SignatureTypeCode.Single,SignatureTypeCode.UInt32),
            "Int32BitsToSingle" => (SignatureTypeCode.Int32,SignatureTypeCode.Single),
            "UInt32BitsToSingle" => (SignatureTypeCode.UInt32,SignatureTypeCode.Single),
            "DoubleToInt64Bits" => (SignatureTypeCode.Double,SignatureTypeCode.Int64),
            "DoubleToUInt64Bits" => (SignatureTypeCode.Double,SignatureTypeCode.UInt64),
            "Int64BitsToDouble" => (SignatureTypeCode.Int64,SignatureTypeCode.Double),
            "UInt64BitsToDouble" => (SignatureTypeCode.UInt64,SignatureTypeCode.Double),
            _ => (SignatureTypeCode.Void,SignatureTypeCode.Void)
        };
        if(input==SignatureTypeCode.Void)return null;
        var signature=SignatureReader.Method(module,blob);
        return !signature.HasThis && signature.Result==new SignatureType(result) && signature.Parameters.SequenceEqual([new SignatureType(input)])
            ? new(new(input),new(result)) : null;
    }
}
