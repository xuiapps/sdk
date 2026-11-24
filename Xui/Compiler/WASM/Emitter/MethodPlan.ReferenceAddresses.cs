using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class MethodPlan
{
    private readonly HashSet<int> addressedReferenceLocals = new();
    private readonly HashSet<int> addressedReferenceArguments = new();

    private void CollectReferenceVariables()
    {
        var reader = Body.Instructions;
        foreach (var node in Graph.Instructions)
        {
            if (Flow is not null && !node.IsReachable) continue;
            if (node.Code is not (OpCode.Ldloca or OpCode.Ldloca_s or OpCode.Ldarga or OpCode.Ldarga_s)) continue;
            reader.Seek(node.Offset);
            var instruction = reader.Read();
            int index = node.Code is OpCode.Ldloca_s or OpCode.Ldarga_s
                ? instruction.ReadShortVariable(ref reader).Index : instruction.ReadVariable(ref reader).Index;
            bool argument = node.Code is OpCode.Ldarga or OpCode.Ldarga_s;
            var variable = argument ? Signature.Parameters[index] : Locals[index];
            if (variable.IsByReference || !StorageModel.IsReference(variable)) continue;
            (argument ? addressedReferenceArguments : addressedReferenceLocals).Add(index);
        }
        if (addressedReferenceLocals.Count + addressedReferenceArguments.Count == 0) return;
        Storage.ReferenceSlotsEnabled = Storage.ReferenceAddressesEnabled = Storage.ExceptionsEnabled = true;
        RequireGcTemporaries();
    }
}
