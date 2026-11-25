using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using Xui.Compiler.XuiNet.DotNet.Intrinsics;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private bool TryInt32Formatting(Instruction.Token instruction, int offset)
    {
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
        }
        else if (!Int32Formatting.Is(source, MetadataTokens.EntityHandle(instruction.Value))) return false;
        ReadScalarAddress(Pop(instruction.Code, offset), new(SignatureTypeCode.Int32));
        writer.Call(method.Storage.Formatting.Int32);
        stack.Push(new(SignatureTypeCode.String));
        return true;
    }
}
