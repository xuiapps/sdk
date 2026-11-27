namespace Xui.Compiler.WASM.Runtime;

/// <summary>Defines createJsonHost(), the optional native JSON token store.</summary>
public static class JsonJavaScript
{
    public static string Source { get; } = Read();
    private static string Read()
    {
        using var stream = typeof(JsonJavaScript).Assembly.GetManifestResourceStream("Xui.Runtime.Json")!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
