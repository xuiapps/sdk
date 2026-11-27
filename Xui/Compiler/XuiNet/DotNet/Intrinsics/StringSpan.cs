using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Borrowed native string slices and character-span string construction.</summary>
public sealed record StringSpan(MethodSignature Signature, bool Constructor = false)
{
    /// <summary>Recognizes string AsSpan, implicit character spans, and the character-span string constructor.</summary>
    public static StringSpan? Read(Module module, EntityHandle token)
    {
        TypeIdentity owner; string name; BlobReader blob;
        if (token.Kind==HandleKind.MemberReference)
        {
            var member=module.GetMemberReference((MemberReferenceHandle)token);
            if(member.Kind!=MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition)) return null;
            owner=new(module,member.Parent);name=member.Name;blob=member.Signature;
        }
        else if(token.Kind==HandleKind.MethodDefinition)
        {
            var method=module.GetMethod((MethodDefinitionHandle)token);
            owner=new(module,method.DeclaringType);name=method.Name;blob=method.Signature;
        }
        else return null;
        if (!(name=="AsSpan" && FrameworkTypes.Is(owner,"System","MemoryExtensions") ||
              name is "op_Implicit" or ".ctor" && FrameworkTypes.Is(owner,"System","String"))) return null;
        var signature=SignatureReader.Method(module,blob);
        if(name==".ctor")
            return signature.HasThis && signature.GenericParameterCount==0 && signature.Parameters.Length==1 &&
                IsCharacterSpan(signature.Parameters[0]) && signature.Result.Code==SignatureTypeCode.Void ? new(signature,true) : null;
        if(signature.HasThis || signature.GenericParameterCount!=0 || signature.Parameters.Length is <1 or >3 ||
            signature.Parameters[0]!=new SignatureType(SignatureTypeCode.String) ||
            signature.Parameters.Skip(1).Any(p=>p!=new SignatureType(SignatureTypeCode.Int32)) ||
            !IsCharacterSpan(signature.Result)) return null;
        return new(signature);
    }

    private static bool IsCharacterSpan(SignatureType type) => !type.IsByReference && type.Identity is { } span &&
        FrameworkTypes.Is(span,"System","ReadOnlySpan`1") && span.Arguments?.Values.Length==1 &&
        span.Arguments.Values[0]==new SignatureType(SignatureTypeCode.Char);
}
