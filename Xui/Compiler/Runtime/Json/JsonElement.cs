using System.Text.Json;

namespace Xui.Compiler.Runtime.Json;

/// <summary>A copied token index with a GC-visible reference to its document.</summary>
public readonly partial struct JsonElement
{
    private readonly JsonDocument? document;
    private readonly int index;
    internal JsonElement(JsonDocument document, int index) { this.document = document; this.index = index; }
    public JsonValueKind ValueKind { get { if (document is null) return JsonValueKind.Undefined; document.Check(); return (JsonValueKind)NativeJson.Kind(document, index); } }
    private void Require(JsonValueKind kind) { if (ValueKind != kind) throw new InvalidOperationException("The JSON value has a different kind."); }
    public int GetArrayLength() { Require(JsonValueKind.Array); return NativeJson.Length(document!, index); }
    public JsonElement this[int child]
    {
        get
        {
            int length = GetArrayLength();
            if ((uint)child >= (uint)length) throw new IndexOutOfRangeException();
            return new(document!, NativeJson.Element(document!, index, child));
        }
    }
    public bool TryGetProperty(string propertyName, out JsonElement value)
    {
        ArgumentNullException.ThrowIfNull(propertyName); Require(JsonValueKind.Object);
        int child = NativeJson.Property(document!, index, propertyName);
        value = child < 0 ? default : new(document!, child);
        return child >= 0;
    }
    public JsonElement GetProperty(string propertyName) => TryGetProperty(propertyName, out var value) ? value : throw new KeyNotFoundException();
    public string? GetString()
    {
        if (ValueKind == JsonValueKind.Null) return null;
        Require(JsonValueKind.String);
        if (!NativeJson.ValidString(document!, index)) throw new InvalidOperationException("The JSON string contains an unpaired surrogate.");
        return NativeJson.String(document!, index);
    }
    public bool GetBoolean() { var kind = ValueKind; if (kind == JsonValueKind.True) return true; if (kind == JsonValueKind.False) return false; throw new InvalidOperationException(); }
    public double GetDouble() { Require(JsonValueKind.Number); return NativeJson.Double(document!, index); }
    public float GetSingle() => (float)GetDouble();
    public bool TryGetInt32(out int value) { Require(JsonValueKind.Number); bool valid = NativeJson.IntegerValid(document!, index, 32, false); value = valid ? NativeJson.Int32(document!, index) : 0; return valid; }
    public bool TryGetUInt32(out uint value) { Require(JsonValueKind.Number); bool valid = NativeJson.IntegerValid(document!, index, 32, true); value = valid ? (uint)NativeJson.Int32(document!, index) : 0; return valid; }
    public bool TryGetInt64(out long value) { Require(JsonValueKind.Number); bool valid = NativeJson.IntegerValid(document!, index, 64, false); value = valid ? NativeJson.Int64(document!, index) : 0; return valid; }
    public bool TryGetUInt64(out ulong value) { Require(JsonValueKind.Number); bool valid = NativeJson.IntegerValid(document!, index, 64, true); value = valid ? (ulong)NativeJson.Int64(document!, index) : 0; return valid; }
    public int GetInt32() => TryGetInt32(out var value) ? value : throw new FormatException();
    public uint GetUInt32() => TryGetUInt32(out var value) ? value : throw new FormatException();
    public long GetInt64() => TryGetInt64(out var value) ? value : throw new FormatException();
    public ulong GetUInt64() => TryGetUInt64(out var value) ? value : throw new FormatException();
    public ArrayEnumerator EnumerateArray() { Require(JsonValueKind.Array); return new(this); }
}
