namespace Xui.Compiler.Runtime.Json;

internal static class NativeJson
{
    [RuntimeImport("xui-json", "parse")]
    internal static string? Parse(object owner, string text, int maxDepth, bool duplicates) => throw new NotSupportedException();
    [RuntimeImport("xui-json", "close")]
    internal static void Close(object owner) => throw new NotSupportedException();
    [RuntimeImport("xui-json", "kind")]
    internal static int Kind(object owner, int index) => throw new NotSupportedException();
    [RuntimeImport("xui-json", "length")]
    internal static int Length(object owner, int index) => throw new NotSupportedException();
    [RuntimeImport("xui-json", "element")]
    internal static int Element(object owner, int index, int child) => throw new NotSupportedException();
    [RuntimeImport("xui-json", "property")]
    internal static int Property(object owner, int index, string name) => throw new NotSupportedException();
    [RuntimeImport("xui-json", "string")]
    internal static string String(object owner, int index) => throw new NotSupportedException();
    [RuntimeImport("xui-json", "valid-string")]
    internal static bool ValidString(object owner, int index) => throw new NotSupportedException();
    [RuntimeImport("xui-json", "double")]
    internal static double Double(object owner, int index) => throw new NotSupportedException();
    [RuntimeImport("xui-json", "integer-valid")]
    internal static bool IntegerValid(object owner, int index, int bits, bool unsigned) => throw new NotSupportedException();
    [RuntimeImport("xui-json", "i32")]
    internal static int Int32(object owner, int index) => throw new NotSupportedException();
    [RuntimeImport("xui-json", "i64")]
    internal static long Int64(object owner, int index) => throw new NotSupportedException();
}
