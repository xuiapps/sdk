namespace Xui.Compiler.WASM.Runtime;

/// <summary>The shared JavaScript source defining xuiRuntimeImports for generated WASM modules.</summary>
public static class JavaScript
{
    /// <summary>Standalone source suitable for embedding in HTML or a Node script before instantiation.</summary>
    public static string Source { get; } = ReadSource();

    private static string ReadSource()
    {
        using var stream = typeof(JavaScript).Assembly.GetManifestResourceStream("Xui.Runtime.JavaScript")!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
