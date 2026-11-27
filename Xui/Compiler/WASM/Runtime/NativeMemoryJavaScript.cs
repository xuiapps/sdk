namespace Xui.Compiler.WASM.Runtime;

/// <summary>Defines createNativeMemoryHost(), a reusable free-list allocator for one module instance.</summary>
public static class NativeMemoryJavaScript
{
    public static string Source { get; } = Read();
    private static string Read()
    {
        using var stream = typeof(NativeMemoryJavaScript).Assembly.GetManifestResourceStream("Xui.Runtime.NativeMemory")!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
