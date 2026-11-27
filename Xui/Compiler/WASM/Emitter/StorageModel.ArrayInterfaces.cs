using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    public HashSet<TypeIdentity> ArrayTypeTests { get; } = new();
    private readonly Dictionary<MethodIdentity, (LocalVariable Result, TypeIndex Block)> arrayInterfaceCalls = new();
    public void RequireArrayInterface(MethodIdentity method)
    {
        if (ArrayInterface.Operation(method) is null) return;
        ArrayIdentity.Enabled = true;
        ExtraExceptionKinds.Add(XuiNet.DotNet.Exceptions.ExceptionKind.ArgumentOutOfRangeException);
        var signature = XuiNet.DotNet.Signatures.SignatureReader.Method(method.DeclaringType.Module,
            method.DeclaringType.Module.GetMethod(method.Handle).Signature, method.DeclaringType.Arguments);
        arrayInterfaceCalls.TryAdd(method, (signature.Result.Code == System.Reflection.Metadata.SignatureTypeCode.Void ? new(System.Reflection.Metadata.SignatureTypeCode.Void) : Bind(signature.Result), default));
    }
    public void ValidateArrayInterfaceUses()
    {
        if (arrays.Count == 0) return;
        var contracts = ArrayTypeTests.Concat(dependencies.Nodes.OfType<XuiNet.Reachability.Dependency.VirtualCall>().Select(c => c.Identity.DeclaringType));
        foreach (var call in dependencies.Nodes.OfType<XuiNet.Reachability.Dependency.VirtualCall>())
            if (ArrayImplementations(call.Identity.DeclaringType).Any() && ArrayInterface.Operation(call.Identity) is null)
                throw new NotSupportedException($"Array interface operation is not implemented: {call.Identity}.");
        foreach (var contract in contracts.Distinct())
        {
            if (!ArrayInterface.HasArrayImplementation(contract)) continue;
            if (contract.Arguments?.Values.Length != 1)
                throw new NotSupportedException($"Non-generic array contracts are not implemented: {contract}.");
            var element = contract.Arguments.Values[0];
            if (arrays.Keys.Any(a => ArrayInterface.RequiresUnsupportedConversion(a, contract)))
                throw new NotSupportedException($"Array collection conversion requires an exact element type: {contract}.");
            if (!ArrayInterface.Matches(new(element), contract))
            {
                if (arrays.Keys.Any(a => a.Element == element))
                    throw new NotSupportedException($"Array interface enumeration and mutation are not implemented: {contract}.");
                continue;
            }
            if (arrays.Keys.Any(a => a.Element.Code == System.Reflection.Metadata.SignatureTypeCode.String) &&
                element.Code == System.Reflection.Metadata.SignatureTypeCode.Object)
                throw new NotSupportedException("Array interface covariance from native strings to objects is not implemented.");
        }
    }

    public MethodIdentity? ArrayEnumerator(MethodIdentity target) => ArrayInterface.EnumeratorConstructor(target) is { } constructor &&
        dependencies.Nodes.Contains(new XuiNet.Reachability.Dependency.Method(constructor)) ? constructor : null;

    public IEnumerable<ArrayShape> ArrayImplementations(TypeIdentity contract) => arrays.Values.Where(a => ArrayInterface.Matches(a.Signature, contract));
    public TypeIndex ArrayInterfaceBlock(MethodIdentity method) => arrayInterfaceCalls[method].Block;
    private void DefineArrayInterfaceTypes(ref Writer writer)
    {
        foreach (var pair in arrayInterfaceCalls.ToArray())
            arrayInterfaceCalls[pair.Key] = (pair.Value.Result, pair.Value.Result.TypeCode == System.Reflection.Metadata.SignatureTypeCode.Void ? writer.FunctionType() : writer.FunctionType([], pair.Value.Result.WasmType));
    }
}
