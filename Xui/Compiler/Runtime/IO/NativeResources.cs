namespace Xui.Compiler.Runtime.IO;

/// <summary>Weakly keyed host stream state; borrowed pointers are valid only for the import call.</summary>
internal static class NativeResources
{
    [RuntimeImport("xui-resource", "open")]
    public static bool Open(object stream, string assembly, string name) => throw new NotSupportedException();
    [RuntimeImport("xui-resource", "gzip")]
    public static bool Gzip(object target, object source) => throw new NotSupportedException();
    [RuntimeImport("xui-resource", "read")]
    public static int Read(object stream, nint destination, int count) => throw new NotSupportedException();
    [RuntimeImport("xui-resource", "length")]
    public static long Length(object stream) => throw new NotSupportedException();
    [RuntimeImport("xui-resource", "position")]
    public static long Position(object stream) => throw new NotSupportedException();
    [RuntimeImport("xui-resource", "seek")]
    public static void Seek(object stream, long position) => throw new NotSupportedException();
    [RuntimeImport("xui-resource", "close")]
    public static void Close(object stream) => throw new NotSupportedException();
}
