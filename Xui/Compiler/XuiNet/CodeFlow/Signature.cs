using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using Model = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Declared arguments and normalized return type for stack analysis.</summary>
internal sealed record Signature(
    SignatureType[] Parameters,
    StackEntry? Result)
{
    public static Signature Read(Model.Module module, BlobReader reader, Model.TypeIdentity? receiver = null)
    {
        var signature = SignatureReader.Method(module, reader, receiver?.Arguments);
        var parameters = signature.Parameters.ToList();
        if (signature.HasThis)
        {
            if (receiver is not { } type)
                throw new NotSupportedException("Only direct same-module instance calls are currently analyzed.");
            parameters.Insert(0, new(SignatureTypeCode.TypeHandle, type, IsByReference: type.IsValueType, IsReferenceType: !type.IsValueType));
        }
        return new(parameters.ToArray(), signature.Result.Code == SignatureTypeCode.Void ? null : StackEntry.From(signature.Result));
    }

    internal static bool IsValueType(Model.TypeIdentity type) => type.IsValueType;
}
