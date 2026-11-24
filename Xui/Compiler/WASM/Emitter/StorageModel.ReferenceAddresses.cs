using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using BodyWriter = Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody.Writer;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class StorageModel
{
    // A managed reference is two values: a GC-visible owner and an accessor selector.
    // Null owners select caller-frame reference-table slots instead of object fields.
    public bool ReferenceAddressesEnabled { get; set; }
    public List<TypeIndex> ReferenceAccessorTypes { get; } = new();
    public List<FunctionIndex> ReferenceAccessorFunctions { get; } = new();
    private readonly List<(GcLocation Location, LocalVariable Type)> referenceFields = new();

    public int ReferenceField(GcLocation location, LocalVariable type)
    {
        int found = referenceFields.FindIndex(f => f.Location == location && f.Type == type);
        if (found >= 0) return found;
        referenceFields.Add((location, type));
        return referenceFields.Count - 1;
    }

    public FunctionIndex ReferenceAccessor(LocalVariable type, bool store) =>
        ReferenceAccessorFunctions[(type.TypeCode == SignatureTypeCode.String ? 2 : 0) + (store ? 1 : 0)];

    public void DefineReferenceAddressTypes(ref TypeWriter writer)
    {
        if (!ReferenceAddressesEnabled) return;
        foreach (var value in new[] { ValueTypeCode.EqRef, ValueTypeCode.ExternRef })
        {
            ReferenceAccessorTypes.Add(writer.FunctionType([ValueTypeCode.EqRef, ValueTypeCode.I32], value));
            ReferenceAccessorTypes.Add(writer.FunctionType([ValueTypeCode.EqRef, ValueTypeCode.I32, value]));
        }
    }

    public void EmitReferenceAccessor(BodyWriter writer, int accessor, EmissionRuntime runtime)
    {
        bool store = (accessor & 1) != 0;
        bool text = accessor >= 2;
        if (ReferenceSlotsEnabled)
        {
            writer.LocalGet(new(0)).RefIsNull().If();
            writer.LocalGet(new(1));
            var table = text ? runtime.StringReferences!.Value : runtime.ObjectReferences!.Value;
            if (store) writer.LocalGet(new(2)).TableSet(table);
            else writer.TableGet(table);
            writer.Return().End();
        }
        for (int i = 0; i < referenceFields.Count; i++)
        {
            var (location, type) = referenceFields[i];
            if ((type.TypeCode == SignatureTypeCode.String) != text) continue;
            writer.LocalGet(new(1)).I32Const(i).I32Eq().If();
            writer.LocalGet(new(0)).RefCast(location.Owner.Type);
            if (store) writer.LocalGet(new(2)).StructSet(location.Owner.Type, new((uint)location.First));
            else writer.StructGet(location.Owner.Type, new((uint)location.First));
            writer.Return().End();
        }
        writer.Unreachable(); // A selector is created only by this module's checked address operations.
    }
}
