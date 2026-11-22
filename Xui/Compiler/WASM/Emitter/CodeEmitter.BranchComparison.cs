using System.Reflection.Metadata;
using Xui.Compiler.XuiNet.DotNet.Op;

namespace Xui.Compiler.WASM.Emitter;

public readonly ref partial struct CodeEmitter
{
    private void EmitBranchComparison(OpCode code, int offset, ComparisonKind comparison, bool unsignedOrUnordered)
    {
        if (comparison is ComparisonKind.Equal or ComparisonKind.NotEqual &&
            TryReferenceComparison(code, offset, comparison == ComparisonKind.NotEqual)) return;
        var right = Pop(code, offset);
        Pop(right.TypeCode, code, offset);
        switch (right.TypeCode)
        {
            case SignatureTypeCode.Int32:
            case SignatureTypeCode.IntPtr:
                EmitIntegerBranchComparison(comparison, unsignedOrUnordered, wide: false);
                break;
            case SignatureTypeCode.Int64:
                EmitIntegerBranchComparison(comparison, unsignedOrUnordered, wide: true);
                break;
            case SignatureTypeCode.Double:
                EmitFloatBranchComparison(comparison, unsignedOrUnordered);
                break;
            default:
                throw new NotSupportedException($"Unsupported {code} operand type {right.TypeCode} at IL_{offset:x4}.");
        }
    }

    private void EmitIntegerBranchComparison(ComparisonKind comparison, bool unsigned, bool wide)
    {
        switch (comparison)
        {
            case ComparisonKind.Equal:
            case ComparisonKind.NotEqual:
                if (wide) writer.I64Eq(); else writer.I32Eq();
                break;
            case ComparisonKind.GreaterThan:
            case ComparisonKind.LessThanOrEqual:
                if (wide)
                {
                    if (unsigned) writer.I64GtU(); else writer.I64GtS();
                }
                else
                {
                    if (unsigned) writer.I32GtU(); else writer.I32GtS();
                }
                break;
            case ComparisonKind.LessThan:
            case ComparisonKind.GreaterThanOrEqual:
                if (wide)
                {
                    if (unsigned) writer.I64LtU(); else writer.I64LtS();
                }
                else
                {
                    if (unsigned) writer.I32LtU(); else writer.I32LtS();
                }
                break;
        }
        if (comparison is ComparisonKind.NotEqual or ComparisonKind.GreaterThanOrEqual or ComparisonKind.LessThanOrEqual)
            writer.I32Eqz();
    }

    private void EmitFloatBranchComparison(ComparisonKind comparison, bool unordered)
    {
        // The .un branch family treats NaN as true: invert the ordered opposite relation.
        switch (comparison)
        {
            case ComparisonKind.Equal: writer.F64Eq(); break;
            case ComparisonKind.NotEqual: writer.F64Eq().I32Eqz(); break;
            case ComparisonKind.GreaterThan:
                if (unordered) writer.F64Le().I32Eqz(); else writer.F64Gt();
                break;
            case ComparisonKind.GreaterThanOrEqual:
                if (unordered) writer.F64Lt().I32Eqz(); else writer.F64Ge();
                break;
            case ComparisonKind.LessThan:
                if (unordered) writer.F64Ge().I32Eqz(); else writer.F64Lt();
                break;
            case ComparisonKind.LessThanOrEqual:
                if (unordered) writer.F64Gt().I32Eqz(); else writer.F64Le();
                break;
        }
    }
}
