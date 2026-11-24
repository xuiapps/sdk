using Xui.Compiler.WASM.Wasm.Indices;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;
using TypeWriter = Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.Writer;
using FunctionWriter = Xui.Compiler.WASM.Wasm.Section.Function.Writer;
using ImportWriter = Xui.Compiler.WASM.Wasm.Section.Import.Writer;
using CodeWriter = Xui.Compiler.WASM.Wasm.Section.Code.Writer;
using BodyWriter = Xui.Compiler.WASM.Wasm.Section.Code.FunctionBody.Writer;
using static Xui.Compiler.WASM.Wasm.Section.TypeDefinitions.ValueTypeCode;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Mutable UTF-16 interpolation buffers rooted in frame-owned reference slots.</summary>
/// <remarks>The eight-byte handler contains a reference-slot index and character count. Copying
/// a handler copies its buffer reference and count; clearing it releases only its own reference.
/// Helpers use their own locals, so nested interpolation and recursive calls remain independent.</remarks>
internal sealed class InterpolationRuntime
{
    public bool Enabled { get; set; }
    private TypeIndex pairType, stringType, integerType, finishType, charCodeType;
    private FunctionIndex reserve, charCode;
    public FunctionIndex AppendString { get; private set; }
    public FunctionIndex AppendChar { get; private set; }
    public FunctionIndex AppendInteger { get; private set; }
    public FunctionIndex Finish { get; private set; }

    public void DefineTypes(ref TypeWriter types)
    {
        if (!Enabled) return;
        pairType = types.FunctionType([I32, I32]);
        stringType = types.FunctionType([I32, ExternRef]);
        integerType = types.FunctionType([I32, I64, I32]);
        finishType = types.FunctionType([I32, I32], ExternRef);
        charCodeType = types.FunctionType([ExternRef, I32], I32);
    }

    public void Import(ref ImportWriter imports)
    {
        if (Enabled) charCode = imports.Function("wasm:js-string", "charCodeAt", charCodeType);
    }

    public void Declare(ref FunctionWriter functions)
    {
        if (!Enabled) return;
        reserve = functions.Function(pairType);
        AppendString = functions.Function(stringType);
        AppendChar = functions.Function(pairType);
        AppendInteger = functions.Function(integerType);
        Finish = functions.Function(finishType);
    }

    public void Emit(ref CodeWriter code, EmissionRuntime runtime)
    {
        if (!Enabled) return;
        using (var body = code.FunctionBody([EqRef, EqRef, I32, I32, I32])) Reserve(body, runtime);
        using (var body = code.FunctionBody([EqRef, I32, I32, I32])) String(body, runtime);
        using (var body = code.FunctionBody()) Character(body, runtime);
        using (var body = code.FunctionBody([EqRef, I64, I32, I32, I32])) Integer(body, runtime);
        using (var body = code.FunctionBody([ExternRef])) Complete(body, runtime);
    }

    private static void Buffer(BodyWriter body, EmissionRuntime runtime) =>
        body.LocalGet(new(0)).I32Load().TableGet(runtime.ObjectReferences!.Value).RefCast(runtime.Utf16ArrayType);

    private static void LengthAddress(BodyWriter body) => body.LocalGet(new(0)).I32Const(4).I32Add();
    private static void Length(BodyWriter body) { LengthAddress(body); body.I32Load(); }

    private static void Reserve(BodyWriter b, EmissionRuntime r)
    {
        // Parameters: header, extra. Locals: old array, new array, required, capacity, used.
        LocalIndex old = new(2), next = new(3), required = new(4), capacity = new(5), used = new(6);
        Length(b); b.LocalSet(used);
        b.LocalGet(new(1)).I32Const(int.MaxValue).LocalGet(used).I32Sub().I32GtU().If().Unreachable().End();
        b.LocalGet(used).LocalGet(new(1)).I32Add().LocalSet(required);
        b.LocalGet(new(0)).I32Load().TableGet(r.ObjectReferences!.Value).LocalTee(old).RefIsNull().I32Eqz().If();
        b.LocalGet(old).RefCast(r.Utf16ArrayType).ArrayLen().LocalSet(capacity);
        b.LocalGet(required).LocalGet(capacity).I32GtU().I32Eqz().If().Return().End().End();
        // Grow geometrically; keep overflow out of the capacity calculation.
        b.LocalGet(capacity).I32Const(int.MaxValue / 2).I32GtU().If();
        b.LocalGet(required).LocalSet(capacity).Else();
        b.LocalGet(capacity).I32Const(2).I32Mul().LocalSet(capacity).End();
        b.LocalGet(capacity).I32Const(16).I32LtU().If().I32Const(16).LocalSet(capacity).End();
        b.LocalGet(capacity).LocalGet(required).I32LtU().If().LocalGet(required).LocalSet(capacity).End();
        b.LocalGet(capacity).ArrayNewDefault(r.Utf16ArrayType).LocalSet(next);
        b.LocalGet(used).If();
        b.LocalGet(next).RefCast(r.Utf16ArrayType).I32Const(0)
            .LocalGet(old).RefCast(r.Utf16ArrayType).I32Const(0).LocalGet(used).ArrayCopy(r.Utf16ArrayType, r.Utf16ArrayType).End();
        b.LocalGet(new(0)).I32Load().LocalGet(next).TableSet(r.ObjectReferences.Value);
    }

    private void String(BodyWriter b, EmissionRuntime r)
    {
        // Parameters: header, string. Locals: array, length, start, index.
        LocalIndex array = new(2), length = new(3), start = new(4), index = new(5);
        b.LocalGet(new(1)).RefIsNull().If().Return().End();
        b.LocalGet(new(1)).Call(r.StringLength!.Value).LocalTee(length).I32Eqz().If().Return().End();
        b.LocalGet(new(0)).LocalGet(length).Call(reserve);
        Buffer(b, r); b.LocalSet(array); Length(b); b.LocalSet(start);
        b.Loop();
        b.LocalGet(array).RefCast(r.Utf16ArrayType).LocalGet(start).LocalGet(index).I32Add()
            .LocalGet(new(1)).LocalGet(index).Call(charCode).ArraySet(r.Utf16ArrayType);
        b.LocalGet(index).I32Const(1).I32Add().LocalTee(index).LocalGet(length).I32LtU().BrIf(new(0)).End();
        LengthAddress(b); b.LocalGet(start).LocalGet(length).I32Add().I32Store();
    }

    private void Character(BodyWriter b, EmissionRuntime r)
    {
        b.LocalGet(new(0)).I32Const(1).Call(reserve);
        Buffer(b, r); Length(b); b.LocalGet(new(1)).ArraySet(r.Utf16ArrayType);
        LengthAddress(b); Length(b); b.I32Const(1).I32Add().I32Store();
    }

    private void Integer(BodyWriter b, EmissionRuntime r)
    {
        // Parameters: header, bits, signed. Locals: array, quotient, start, digits, cursor.
        LocalIndex array = new(3), quotient = new(4), start = new(5), digits = new(6), cursor = new(7);
        b.LocalGet(new(0)).I32Const(21).Call(reserve);
        Buffer(b, r); b.LocalSet(array); Length(b); b.LocalSet(start);
        b.LocalGet(new(2)).LocalGet(new(1)).I64Const(0).I64LtS().I32And().If();
        b.LocalGet(array).RefCast(r.Utf16ArrayType).LocalGet(start).I32Const('-').ArraySet(r.Utf16ArrayType);
        b.LocalGet(start).I32Const(1).I32Add().LocalSet(start);
        // Wrapping negation leaves MinValue's magnitude representable as an unsigned i64.
        b.I64Const(0).LocalGet(new(1)).I64Sub().LocalSet(new(1)).End();
        b.LocalGet(new(1)).LocalSet(quotient).Loop();
        b.LocalGet(digits).I32Const(1).I32Add().LocalSet(digits);
        b.LocalGet(quotient).I64Const(10).I64DivU().LocalTee(quotient).I64Eqz().I32Eqz().BrIf(new(0)).End();
        b.LocalGet(start).LocalGet(digits).I32Add().LocalSet(cursor);
        LengthAddress(b); b.LocalGet(cursor).I32Store();
        b.Loop();
        b.LocalGet(cursor).I32Const(1).I32Sub().LocalSet(cursor);
        b.LocalGet(array).RefCast(r.Utf16ArrayType).LocalGet(cursor)
            .LocalGet(new(1)).I64Const(10).I64RemU().I32WrapI64().I32Const('0').I32Add().ArraySet(r.Utf16ArrayType);
        b.LocalGet(new(1)).I64Const(10).I64DivU().LocalSet(new(1));
        b.LocalGet(cursor).LocalGet(start).I32GtU().BrIf(new(0)).End();
    }

    private void Complete(BodyWriter b, EmissionRuntime r)
    {
        b.LocalGet(new(0)).I32Const(0).Call(reserve);
        Buffer(b, r); b.I32Const(0); Length(b);
        b.Call(r.FromCharCodeArrayFunction).LocalSet(new(2));
        b.LocalGet(new(1)).If();
        b.LocalGet(new(0)).I32Load().RefNull(EqRef).TableSet(r.ObjectReferences!.Value);
        LengthAddress(b); b.I32Const(0).I32Store();
        b.End().LocalGet(new(2));
    }
}
