using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitInvariantDecimalFormatting(Instruction.Token instruction, int offset, TypeIdentity type)
    {
        PopOwner(type, true, instruction.Code, offset);
        SaveAddress();
        var shape = method.Storage.Shape(type);
        // Resolve fields through metadata, so GC leaves and linear offsets use the same layout.
        foreach (var (name, code) in new[] { ("_flags", SignatureTypeCode.Int32), ("_hi32", SignatureTypeCode.UInt32), ("_lo64", SignatureTypeCode.UInt64) })
        {
            var field = shape.Fields.SingleOrDefault(pair => type.Module.GetField(pair.Key.Handle).Name == name);
            if (field.Value.Count != 1 || field.Value.Storage.TypeCode != code)
                throw new NotSupportedException($"Unsupported Decimal representation: expected {code} {name}.");
            var leaf = shape.Leaves[field.Value.First];
            ProjectAddress(method.AddressOwner, method.AddressSelector, leaf.MemoryOffset, field.Value.First, leaf.Storage);
            writer.Call(method.Storage.ReferenceAccessor(leaf.Storage, false));
        }
        writer.Call(method.Storage.Formatting.Decimal);
        stack.Push(new(SignatureTypeCode.String));
    }
}
