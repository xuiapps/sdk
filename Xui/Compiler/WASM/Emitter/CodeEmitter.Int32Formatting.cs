using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryInt32Formatting(Instruction.Token instruction, int offset)
    {
        var integerType = SignatureTypeCode.Int32;
        if (method.InvariantFormatting.TryGetValue(offset, out var formatting))
        {
            if (formatting.IsProvider)
            {
                // This marker cannot escape: the graph proves the next instruction consumes it.
                writer.RefNull(Wasm.Section.TypeDefinitions.ValueTypeCode.EqRef);
                stack.Push(new(formatting.Provider.Code, formatting.Provider.Identity));
                return true;
            }
            var provider = Pop(instruction.Code, offset);
            if (provider.Identity != formatting.Provider.Identity || provider.IsByReference)
                throw new InvalidProgramException("Invariant formatting requires its paired provider.");
            writer.Drop();
            if (formatting.Parsing is { } parsing)
            { EmitNumericParsing(instruction, offset, parsing); return true; }
            if (formatting.HasNullFormat) { Pop(SignatureTypeCode.String, instruction.Code, offset); writer.Drop(); }
            if (formatting.Value.Identity is { } decimalType && XuiNet.DotNet.FrameworkTypes.Is(decimalType, "System", "Decimal"))
            {
                EmitInvariantDecimalFormatting(instruction, offset, decimalType);
                return true;
            }
            if (formatting.ValueType is SignatureTypeCode.Double or SignatureTypeCode.Single)
            {
                EmitInvariantFloatingPointFormatting(instruction, offset, formatting.ValueType);
                return true;
            }
            integerType = formatting.ValueType;
        }
        else if (Int32Formatting.InvariantProvider(source, MetadataTokens.EntityHandle(instruction.Value)) is { } invariant)
        {
            writer.GlobalGet(method.Storage.InvariantCultures[invariant.Identity!.Value]);
            stack.Push(new(invariant.Code, invariant.Identity));
            return true;
        }
        else if (!Int32Formatting.Is(source, MetadataTokens.EntityHandle(instruction.Value))) return false;
        ReadScalarAddress(Pop(instruction.Code, offset), new(integerType));
        writer.Call(method.Storage.Formatting.Integer(integerType));
        stack.Push(new(SignatureTypeCode.String));
        return true;
    }
}
