using System.Reflection;
using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;
using Xui.Compiler.XuiNet.DotNet.Signatures;
using MethodBody = Xui.Compiler.XuiNet.DotNet.MethodBody;

namespace Xui.Compiler.XuiNet.CodeFlow;

/// <summary>Specializes a closed generic owner's direct call when its trailing boolean argument is proven constant.</summary>
public static class BooleanCallSpecialization
{
    /// <summary>Keeps the ABI unchanged and refuses parameters whose address escapes or which the callee overwrites.</summary>
    public static MethodIdentity Target(MethodBody caller, InstructionGraph graph, int offset, MethodIdentity target)
    {
        if (target.DeclaringType.Arguments is null) return target;
        var definition = target.DeclaringType.Module.GetMethod(target.Handle);
        if ((definition.Attributes & (MethodAttributes.Virtual | MethodAttributes.Abstract)) != 0 || definition.Body is not { } body) return target;
        var signature = SignatureReader.Method(target.DeclaringType.Module, definition.Signature, target.DeclaringType.Arguments, target.Arguments);
        if (signature.Parameters.Length == 0 || signature.Parameters[^1] != new SignatureType(SignatureTypeCode.Boolean)) return target;
        int position = -1;
        for (int i = 1; i < graph.Instructions.Count; i++) if (graph.Instructions[i].Offset == offset) { position = i; break; }
        if (position < 1 || graph.Instructions[position].Code != OpCode.Call) return target;
        var call = graph.Instructions[position]; var previous = graph.Instructions[position - 1];
        if (previous.Successors.Length != 1 || previous.Successors[0] != call || graph.Instructions.Sum(n => n.Successors.Count(s => s == call)) != 1) return target;
        if (caller.ExceptionRegions.Any(r => offset == r.TryOffset || offset == r.HandlerOffset || offset == r.TryOffset + r.TryLength || offset == r.HandlerOffset + r.HandlerLength)) return target;
        int? value = graph.Specializations.TryGetValue(previous.Offset, out var constant) && constant.Kind == SpecializationKind.Int32
            ? constant.Value : previous.Code == OpCode.Ldc_i4_0 ? 0 : previous.Code == OpCode.Ldc_i4_1 ? 1 : null;
        if (value is not (0 or 1)) return target;
        int parameter = signature.Parameters.Length - 1 + (signature.HasThis ? 1 : 0);
        foreach (var node in GraphReader.Read(body.Instructions).Instructions)
        {
            if (node.Code is not (OpCode.Starg or OpCode.Starg_s or OpCode.Ldarga or OpCode.Ldarga_s)) continue;
            var reader = body.Instructions; reader.Seek(node.Offset); var instruction = reader.Read();
            int index = node.Code is OpCode.Starg_s or OpCode.Ldarga_s ? instruction.ReadShortVariable(ref reader).Index : instruction.ReadVariable(ref reader).Index;
            if (index == parameter) return target;
        }
        return target with { ConstantBoolean = new(parameter, value == 1) };
    }
}
