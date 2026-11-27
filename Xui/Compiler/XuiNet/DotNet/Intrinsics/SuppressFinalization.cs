using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;

namespace Xui.Compiler.XuiNet.DotNet.Intrinsics;

/// <summary>Recognizes the null-checking GC.SuppressFinalize entry point; the backend must reject finalizable types.</summary>
public static class SuppressFinalization
{
    public static bool Is(Module module, EntityHandle token)
    {
        TypeIdentity owner; string name; BlobReader blob;
        if (token.Kind == HandleKind.MemberReference)
        {
            var member=module.GetMemberReference((MemberReferenceHandle)token);
            if(member.Kind!=MemberReferenceKind.Method || member.Parent.Kind is not (HandleKind.TypeReference or HandleKind.TypeDefinition))return false;
            owner=new(module,member.Parent);name=member.Name;blob=member.Signature;
        }
        else if(token.Kind==HandleKind.MethodDefinition)
        {var method=module.GetMethod((MethodDefinitionHandle)token);owner=new(module,method.DeclaringType);name=method.Name;blob=method.Signature;}
        else return false;
        if(name!="SuppressFinalize" || !FrameworkTypes.Is(owner,"System","GC"))return false;
        var signature=SignatureReader.Method(module,blob);
        return !signature.HasThis && signature.Result.Code==SignatureTypeCode.Void &&
            signature.Parameters is [{Code: SignatureTypeCode.Object, IsByReference: false}];
    }
}
