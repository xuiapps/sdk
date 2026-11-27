using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.CodeFlow;
using Xui.Compiler.WASM.Wasm.Section.TypeDefinitions;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void CoerceNullStrings(StackState target)
    {
        if (method.MergeLocals.Count==0 || !stack.Any(v=>v.IsNull)) return;
        if (target.Depth != stack.Count) return; // A terminal arm has no continuation stack.
        var expected = new StackEntry[stack.Count];
        for (int i=0;i<expected.Length;i++,target=target.Pop()) expected[i]=target.Entry!.Value;
        var values = stack.ToArray();
        int last = -1;
        for (int i=0;i<values.Length;i++) if (values[i].IsNull && expected[i].Type==StackType.String) last=i;
        if (last<0) return;
        for (int i=0;i<=last;i++)
        {
            if (values[i].IsNull) writer.Drop();
            else if (values[i].IsByReference)
                writer.LocalSet(method.MergeLocals[(i,ValueTypeCode.I32)]).LocalSet(method.MergeLocals[(i,ValueTypeCode.EqRef)]);
            else writer.LocalSet(method.MergeLocals[(i,MergeType(values[i]))]);
        }
        for (int i=last;i>=0;i--)
        {
            var value=values[i];
            if (value.IsNull)
            {
                bool text=expected[i].Type==StackType.String;
                writer.RefNull(text?ValueTypeCode.ExternRef:ValueTypeCode.EqRef);
                if (text) value=new(SignatureTypeCode.String);
            }
            else if (value.IsByReference)
                writer.LocalGet(method.MergeLocals[(i,ValueTypeCode.EqRef)]).LocalGet(method.MergeLocals[(i,ValueTypeCode.I32)]);
            else writer.LocalGet(method.MergeLocals[(i,MergeType(value))]);
            values[i]=value;
        }
        stack.Clear();
        for (int i=values.Length-1;i>=0;i--) stack.Push(values[i]);
    }

    private ValueTypeCode MergeType(MsilStackEntry value) => value.TypeCode switch
    {
        SignatureTypeCode.Int64 or SignatureTypeCode.UInt64 => ValueTypeCode.I64,
        SignatureTypeCode.Single or SignatureTypeCode.Double => ValueTypeCode.F64,
        SignatureTypeCode.String => ValueTypeCode.ExternRef,
        SignatureTypeCode.Object or SignatureTypeCode.SZArray or SignatureTypeCode.Array => ValueTypeCode.EqRef,
        SignatureTypeCode.TypeHandle => method.Storage.IsGc(value.Identity!.Value)?ValueTypeCode.EqRef:ValueTypeCode.I32,
        _ => ValueTypeCode.I32
    };
}
