using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using BodyWriter = Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody.Writer;
using WasmType = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueType;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    public HashSet<MethodIdentity> DelegateMethods { get; } = new();
    public Dictionary<MethodIdentity, DelegateTarget> DelegateTargets { get; } = new();

    public void DefineDelegateTypes(ref TypeWriter writer, IReadOnlyDictionary<MethodIdentity, MethodPlan> methods)
    {
        DefineMulticastTypes(ref writer);
        foreach (var identity in DelegateMethods)
        {
            var method = methods[identity];
            if (method.Signature.HasThis && identity.DeclaringType.IsValueType)
                throw new NotSupportedException("Delegate targets on boxed value types are not implemented.");
            if (method.IsDeclaration) throw new InvalidProgramException("ldftn cannot bind an abstract method.");
            var signature = method.Signature;
            WasmType[] parameters = signature.Parameters.Select(p => (WasmType)p.WasmType).ToArray();
            if (!signature.HasThis) parameters = [(WasmType)ValueTypeCode.EqRef, .. parameters];
            parameters = [.. parameters, .. Enumerable.Repeat((WasmType)ValueTypeCode.I32, signature.ReferenceParameterCount)];
            if (signature.HasResultAddress) parameters = [.. parameters, (WasmType)ValueTypeCode.I32];
            var target = new DelegateTarget(method) { TableSlot = TableSize++ };
            target.Type = writer.FunctionType(parameters, signature.WasmResults);
            DelegateTargets.Add(identity, target);
        }
        if (methods.Values.Any(m => m.IsDelegateInvoke) && TableSize == 0) TableSize = 1;
    }

    public static void EmitDelegateAdapter(BodyWriter writer, DelegateTarget target)
    {
        int shift = target.Method.Signature.HasThis ? 0 : 1;
        for (int i = 0; i < target.Method.Signature.WasmParameterCount; i++) writer.LocalGet(new((uint)(i + shift)));
        writer.Call(target.Method.Function);
    }
}
