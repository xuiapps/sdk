using System.Reflection.Metadata;

namespace Xui.Compiler.WASM.Emitter;

/// <summary>Rejects interface substitutions requiring a representation adapter before installing a table entry.</summary>
internal static class InterfaceAbi
{
    public static void Validate(MethodPlan declaration, MethodPlan implementation)
    {
        var expected = declaration.Signature;
        var actual = implementation.Signature;
        bool matches = expected.HasResultAddress == actual.HasResultAddress &&
            expected.Parameters.Length == actual.Parameters.Length && Compatible(expected.Result, actual.Result);
        for (int i = 0; matches && i < expected.Parameters.Length; i++)
            matches = Compatible(expected.Parameters[i], actual.Parameters[i]);
        if (!matches)
            throw new NotSupportedException($"Interface dispatch requires an unsupported representation adapter: {declaration.Identity} → {implementation.Identity}.");
    }

    private static bool Compatible(LocalVariable expected, LocalVariable actual)
    {
        if (expected.TypeCode == SignatureTypeCode.Void || actual.TypeCode == SignatureTypeCode.Void)
            return expected.TypeCode == actual.TypeCode;
        if (expected.WasmType != actual.WasmType || expected.IsByReference != actual.IsByReference || expected.IsByReference != actual.IsByReference) return false;
        // Arrays have no nominal object header or covariant store checks yet, even though
        // both arrays and class references lower to eqref at the function boundary.
        if (expected.Array is not null || actual.Array is not null) return expected.Array == actual.Array;
        return true;
    }
}
