using System.Text.Json;

namespace Xui.Compiler.Runtime.Json;

/// <summary>A disposable owner for browser JSON tokens. Elements borrow this owner's lifetime.</summary>
public sealed class JsonDocument : IDisposable
{
    private bool disposed;
    private JsonDocument() { }
    public JsonElement RootElement { get { Check(); return new(this, 0); } }

    public static JsonDocument Parse(string json, JsonDocumentOptions options = default)
    {
        ArgumentNullException.ThrowIfNull(json);
        if (options.AllowTrailingCommas || options.CommentHandling != JsonCommentHandling.Disallow)
            throw new NotSupportedException("The browser JSON profile requires standard JSON without comments or trailing commas.");
        var document = new JsonDocument();
        string? error = NativeJson.Parse(document, json, options.MaxDepth == 0 ? 64 : options.MaxDepth, options.AllowDuplicateProperties);
        if (error is not null) throw new JsonException(error);
        return document;
    }

    public static JsonDocument Parse(IO.Stream utf8Json, JsonDocumentOptions options = default)
    {
        ArgumentNullException.ThrowIfNull(utf8Json);
        var bytes = new List<byte>();
        var buffer = new byte[4096];
        int count;
        while ((count = utf8Json.Read(buffer, 0, buffer.Length)) != 0)
            for (int i = 0; i < count; i++) bytes.Add(buffer[i]);
        var data = bytes.ToArray();
        int start = data.Length >= 3 && data[0] == 0xef && data[1] == 0xbb && data[2] == 0xbf ? 3 : 0;
        string text;
        try { text = new Text.UTF8Encoding(false, true).GetString(data, start, data.Length - start); }
        catch (System.Text.DecoderFallbackException) { throw new JsonException("The JSON stream contains invalid UTF-8."); }
        return Parse(text, options);
    }

    internal void Check() { if (disposed) throw new ObjectDisposedException(nameof(JsonDocument)); }
    public void Dispose() { if (!disposed) { disposed = true; NativeJson.Close(this); } }
}
