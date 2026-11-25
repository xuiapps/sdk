using System.Reflection.Metadata;
using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.XuiNet.DotNet;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using FunctionWriter = Xui.Compiler.WASM.Wasm.Section.Function.Writer;
using BodyWriter = Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

internal sealed partial class FormattingRuntime
{
    public bool Composite { get; set; }
    private TypeIndex startType, setType, finishType, i32Type, i64Type, f64Type, boxType, textType;
    public FunctionIndex Start { get; private set; }
    public FunctionIndex Set { get; private set; }
    public FunctionIndex Finish { get; private set; }
    public FunctionIndex TextValue { get; private set; }
    public FunctionIndex BoxValue { get; private set; }
    private FunctionIndex i32Value, i64Value, f64Value;

    private void DefineCompositeTypes(ref TypeWriter types)
    {
        if (!Composite) return;
        startType = types.FunctionType([I32], ExternRef);
        setType = types.FunctionType([ExternRef, I32, ExternRef]);
        finishType = types.FunctionType([ExternRef, ExternRef], [ExternRef, I32]);
        i32Type = types.FunctionType([I32, I32], ExternRef);
        i64Type = types.FunctionType([I32, I64], ExternRef);
        f64Type = types.FunctionType([I32, F64], ExternRef);
        textType = types.FunctionType([ExternRef], ExternRef);
        boxType = types.FunctionType([EqRef], ExternRef);
    }

    private void ImportComposite(ref ImportWriter imports)
    {
        if (!Composite) return;
        TextValue = imports.Function("xui-string", "format-text", textType);
        Start = imports.Function("xui-string", "format-start", startType);
        Set = imports.Function("xui-string", "format-set", setType);
        Finish = imports.Function("xui-string", "format-finish", finishType);
        i32Value = imports.Function("xui-string", "format-i32", i32Type);
        i64Value = imports.Function("xui-string", "format-i64", i64Type);
        f64Value = imports.Function("xui-string", "format-f64", f64Type);
    }

    public void Declare(ref FunctionWriter functions)
    {
        if (Composite) BoxValue = functions.Function(boxType);
    }

    // Only scalars cross the JS boundary. WASM GC objects remain private to the module.
    public void EmitBoxValue(BodyWriter writer, StorageModel storage)
    {
        LocalIndex value = new(0);
        writer.LocalGet(value).RefIsNull().If().RefNull(ExternRef).Return().End();
        foreach (var (box, shape) in storage.Boxes)
        {
            if (EnumInfo.UnderlyingType(box.Identity) is not null) continue; // Enum names/Flags require a separate formatter.
            var kind = box.Storage.Code switch
            {
                SignatureTypeCode.Boolean => FormattingValueKind.Boolean,
                SignatureTypeCode.Char => FormattingValueKind.Char,
                SignatureTypeCode.SByte => FormattingValueKind.Signed8,
                SignatureTypeCode.Int16 => FormattingValueKind.Signed16,
                SignatureTypeCode.Byte => FormattingValueKind.Unsigned8,
                SignatureTypeCode.UInt16 => FormattingValueKind.Unsigned16,
                SignatureTypeCode.UInt32 or SignatureTypeCode.UIntPtr => FormattingValueKind.Unsigned32,
                SignatureTypeCode.Int64 => FormattingValueKind.Signed64,
                SignatureTypeCode.UInt64 => FormattingValueKind.Unsigned64,
                SignatureTypeCode.Single => FormattingValueKind.Single,
                SignatureTypeCode.Double => FormattingValueKind.Double,
                _ => FormattingValueKind.Signed32
            };
            writer.LocalGet(value).RefTest(shape.Type).If();
            writer.LocalGet(value).RefCast(storage.ClassRoot).StructGet(storage.ClassRoot, new(0))
                .GlobalGet(storage.Classes.Resolve(box.Identity).Descriptor).RefEq().If();
            writer.I32Const((int)kind).LocalGet(value).RefCast(shape.Type).StructGet(shape.Type, new(1));
            if (box.Storage.Code == SignatureTypeCode.Single) writer.F64PromoteF32();
            writer.Call(kind is FormattingValueKind.Signed64 or FormattingValueKind.Unsigned64 ? i64Value :
                kind is FormattingValueKind.Single or FormattingValueKind.Double ? f64Value : i32Value).Return().End().End();
        }
        writer.I32Const((int)FormattingValueKind.Unsupported).I32Const(0).Call(i32Value);
    }
}
