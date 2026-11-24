using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private LocalVariable GetArgument(ushort index, OpCode code, int offset)
    {
        if (method.Signature.HasEntryPointArguments)
            throw new NotSupportedException("Access to entry-point string[] args is not implemented.");
        if (index >= parameters.Length)
            throw new InvalidProgramException($"Invalid argument index {index} for {code} at IL_{offset:x4}.");
        return parameters[index];
    }

    private void LoadArgument(ushort index, OpCode code, int offset)
    {
        var argument = GetArgument(index, code, offset);
        if (method.Frame?.ScalarArguments.TryGetValue(index, out int scalarAddress) == true)
        { FrameAddress(scalarAddress); LoadField(argument, offset); return; }
        if (method.Frame?.ReferenceArguments.TryGetValue(index, out int address) == true) { FrameAddress(address); LoadReference(argument); }
        else writer.LocalGet(new LocalIndex(index));
        if (argument.IsReferenceAddress) writer.LocalGet(method.Signature.ReferenceParameter(index));
        if (argument.TypeCode == SignatureTypeCode.Single && !argument.IsByReference) writer.F64PromoteF32();
        if (argument.UsesGc && argument.Identity is { IsValueType: true } id && !argument.IsByReference) CloneGc(id);
        if (argument.IsLinearValue) CopyValue(argument.Identity!.Value, offset);
        stack.Push(new MsilStackEntry(argument.IsByReference ? argument.TypeCode : argument.StackType, argument.Identity, argument.IsByReference, argument.Array)
        { Location = argument.UsesGc && argument.IsByReference && !argument.IsReferenceAddress ? new(method.Storage.Shape(argument.Identity!.Value), 0) : null });
    }

    private void StoreArgument(ushort index, OpCode code, int offset)
    {
        var argument = GetArgument(index, code, offset);
        PopAssignable(argument, code, offset);
        if (method.Frame?.ScalarArguments.TryGetValue(index, out int scalarAddress) == true) StoreScalarVariable(scalarAddress, argument);
        else if (method.Frame?.ReferenceArguments.TryGetValue(index, out int address) == true) StoreReferenceVariable(address, argument);
        else if (argument.IsReferenceAddress) writer.LocalSet(method.Signature.ReferenceParameter(index)).LocalSet(new(index));
        else if (argument.UsesGc && argument.Identity is { IsValueType: true } id && !argument.IsByReference)
            StoreGcLocal(new LocalIndex(index), id);
        else if (argument.IsLinearValue)
        {
            writer.LocalSet(Frame.AddressScratch).LocalGet(new(index)).LocalGet(Frame.AddressScratch);
            CopyStruct(argument.Identity!.Value);
        }
        else { Narrow(argument); writer.LocalSet(new LocalIndex(index)); }
    }
}
