namespace Xui.Compiler.WASM.Runtime;

/// <summary>The optional host for packaged manifest-resource streams.</summary>
public static class ResourceJavaScript
{
    /// <summary>Defines async createResourceHost(catalogue, getMemory), returning synchronous WASM imports.</summary>
    public static string Source { get; } = Read();
    private static string Read()
    {
        using var stream = typeof(ResourceJavaScript).Assembly.GetManifestResourceStream("Xui.Runtime.Resources")!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
