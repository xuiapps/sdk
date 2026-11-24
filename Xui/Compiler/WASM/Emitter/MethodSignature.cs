using Xui.Compiler.WASM.Wasm.Indices;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using IR = Xui.Compiler.XuiNet.DotNet;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>The method ABI, including struct copies, caller-owned results, and instance receivers.</summary>
internal sealed class MethodSignature
{
    public required LocalVariable[] Parameters { get; init; }
    public required SignatureType DeclaredReturnType { get; init; }
    public SignatureTypeCode ReturnType => DeclaredReturnType.Code;
    // A trailing destination address keeps MSIL argument indices unchanged. The caller owns
    // the returned bytes; no pointer into a released callee frame escapes the call.
    public bool HasResultAddress => Result.IsLinearValue;
    public int WasmParameterCount => Parameters.Length + ReferenceParameterCount + (HasResultAddress ? 1 : 0);
    public LocalIndex ResultAddress => new((uint)(Parameters.Length + ReferenceParameterCount));
    // Primary slots preserve MSIL argument indices. Byref selectors follow all primary
    // parameters, then the optional result address. The evaluation stack uses adjacent pairs.
    public int ReferenceParameterCount => Parameters.Count(p => p.IsReferenceAddress);
    public LocalIndex ReferenceParameter(int index) => new((uint)(Parameters.Length + Parameters.Take(index).Count(p => p.IsReferenceAddress)));
    public bool HasThis { get; init; }
    public bool IsConstructor { get; init; }
    public required LocalVariable Result { get; init; }
    public bool HasEntryPointArguments { get; init; }

    public static MethodSignature Read(IR.Module module, IR.MethodDefinition method, IR.TypeIdentity owner, bool entryPoint, StorageModel storage, GenericArguments? methodArguments = null)
    {
        var signature = SignatureReader.Method(module, method.Signature, owner.Arguments, methodArguments);
        if (entryPoint && signature.HasThis) throw new NotSupportedException("Instance entry points are not supported.");
        var result = signature.Result;
        if (entryPoint && result.Code is not (SignatureTypeCode.Void or SignatureTypeCode.Int32))
            throw new NotSupportedException($"Unsupported entry point result: {result}.");
        var resultStorage = storage.Bind(result);
        if (result.IsByReference)
            throw new NotSupportedException("Managed-address return values are not implemented.");
        if (result.Code != SignatureTypeCode.Void) _ = resultStorage.WasmType;
        if (entryPoint && signature.Parameters.Length == 1 && signature.Parameters[0].Array?.Element.Code == SignatureTypeCode.String && !signature.Parameters[0].IsByReference)
            return new() { Parameters = [], DeclaredReturnType = result, Result = resultStorage, HasEntryPointArguments = true };
        if (entryPoint && signature.Parameters.Length != 0)
            throw new NotSupportedException("Entry point parameters are unsupported except an unused string[] args.");
        var parameters = new LocalVariable[signature.Parameters.Length + (signature.HasThis ? 1 : 0)];
        int first = signature.HasThis ? 1 : 0;
        if (signature.HasThis)
        {
            if (!storage.IsGc(owner)) _ = storage.StackLayout(owner);
            parameters[0] = storage.Bind(new(SignatureTypeCode.TypeHandle, owner, IsByReference: owner.IsValueType, IsReferenceType: !owner.IsValueType));
        }
        for (int i = first; i < parameters.Length; i++)
        {
            var type = signature.Parameters[i - first];
            parameters[i] = storage.Bind(type);
            if (type.IsByReference && !parameters[i].IsReferenceAddress && !parameters[i].IsScalar && (type.Identity is not { IsValueType: true } || parameters[i].UsesGc))
                throw new NotSupportedException("This managed-address parameter representation is not implemented.");
            if (parameters[i].IsReferenceAddress) storage.ReferenceAddressesEnabled = true;
            else if (type.IsByReference && !parameters[i].IsScalar) _ = storage.StackLayout(type.Identity!.Value);
            _ = parameters[i].WasmType;
        }
        return new() { Parameters = parameters, DeclaredReturnType = result, Result = resultStorage, HasThis = signature.HasThis, IsConstructor = method.Name == ".ctor" };
    }
}
