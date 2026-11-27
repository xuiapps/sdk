namespace Xui.Compiler.Runtime.Text;

/// <summary>Managed-span encoding surface for browser modules without pinned GC storage.</summary>
/// <remarks>The explicit profile supplies UTF-8 and UTF-16; unsupported encodings are not substituted.</remarks>
public abstract class Encoding
{
    private static readonly Encoding utf8 = new UTF8Encoding(true);
    private static readonly Encoding unicode = new UnicodeEncoding(false, true);
    private static readonly Encoding bigEndianUnicode = new UnicodeEncoding(true, true);
    public static Encoding UTF8 => utf8;
    public static Encoding Unicode => unicode;
    public static Encoding BigEndianUnicode => bigEndianUnicode;
    /// <summary>Resolves the supported Unicode names; legacy code-page services are not part of this profile.</summary>
    public static Encoding GetEncoding(string name)
    {
        if (name is null) throw new ArgumentNullException(nameof(name));
        return name.ToLowerInvariant() switch
        {
            "utf-8" => UTF8,
            "utf-16" or "unicode" => Unicode,
            "utf-16be" or "unicodefffe" => BigEndianUnicode,
            _ => throw new NotSupportedException("The browser encoding profile supports UTF-8 and UTF-16 only.")
        };
    }
    public abstract int CodePage { get; }
    public abstract string WebName { get; }
    public virtual bool IsSingleByte => false;
    public virtual Decoder GetDecoder() => CodePage is 65001 or 1200 or 1201
        ? new UnicodeDecoder(this) : throw new NotSupportedException("Streaming decoding requires a supported Unicode encoding.");
    public abstract int GetByteCount(ReadOnlySpan<char> chars);
    public abstract int GetBytes(ReadOnlySpan<char> chars, Span<byte> bytes);
    public abstract int GetCharCount(ReadOnlySpan<byte> bytes);
    public abstract int GetChars(ReadOnlySpan<byte> bytes, Span<char> chars);
    public abstract int GetMaxByteCount(int charCount);
    public abstract int GetMaxCharCount(int byteCount);
    public virtual byte[] GetPreamble() => [];
    public virtual ReadOnlySpan<byte> Preamble => GetPreamble();

    public virtual string GetString(ReadOnlySpan<byte> bytes)
    {
        var chars = new char[GetCharCount(bytes)];
        GetChars(bytes, chars);
        return new string((ReadOnlySpan<char>)chars);
    }
    public virtual string GetString(byte[] bytes) { Require(bytes, nameof(bytes)); return GetString((ReadOnlySpan<byte>)bytes); }
    public virtual string GetString(byte[] bytes, int index, int count) => GetString(Read(bytes, index, count, nameof(bytes)));
    public virtual int GetCharCount(byte[] bytes) { Require(bytes, nameof(bytes)); return GetCharCount((ReadOnlySpan<byte>)bytes); }
    public virtual int GetCharCount(byte[] bytes, int index, int count) => GetCharCount(Read(bytes, index, count, nameof(bytes)));
    public virtual char[] GetChars(byte[] bytes) { Require(bytes, nameof(bytes)); return GetChars(bytes, 0, bytes.Length); }
    public virtual char[] GetChars(byte[] bytes, int index, int count)
    {
        var source = Read(bytes, index, count, nameof(bytes));
        var chars = new char[GetCharCount(source)]; GetChars(source, chars); return chars;
    }
    public virtual int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) =>
        GetChars(Read(bytes, byteIndex, byteCount, nameof(bytes)), Write(chars, charIndex, nameof(chars)));

    public virtual int GetByteCount(string s) { Require(s, nameof(s)); return GetByteCount(s.AsSpan()); }
    public virtual int GetByteCount(char[] chars) { Require(chars, nameof(chars)); return GetByteCount((ReadOnlySpan<char>)chars); }
    public virtual int GetByteCount(char[] chars, int index, int count) => GetByteCount(Read(chars, index, count, nameof(chars)));
    public virtual byte[] GetBytes(string s)
    {
        Require(s, nameof(s)); var bytes = new byte[GetByteCount(s.AsSpan())]; GetBytes(s.AsSpan(), bytes); return bytes;
    }
    public virtual byte[] GetBytes(char[] chars) { Require(chars, nameof(chars)); return GetBytes(chars, 0, chars.Length); }
    public virtual byte[] GetBytes(char[] chars, int index, int count)
    {
        var source = Read(chars, index, count, nameof(chars));
        var bytes = new byte[GetByteCount(source)]; GetBytes(source, bytes); return bytes;
    }
    public virtual int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) =>
        GetBytes(Read(chars, charIndex, charCount, nameof(chars)), Write(bytes, byteIndex, nameof(bytes)));
    public virtual int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        Require(s, nameof(s)); CheckRange(s.Length, charIndex, charCount);
        return GetBytes(s.AsSpan(charIndex, charCount), Write(bytes, byteIndex, nameof(bytes)));
    }

    private static ReadOnlySpan<T> Read<T>(T[] source, int index, int count, string name)
    { Require(source, name); CheckRange(source.Length, index, count); return new ReadOnlySpan<T>(source, index, count); }
    private static Span<T> Write<T>(T[] target, int index, string name)
    { Require(target, name); CheckRange(target.Length, index, 0); return new Span<T>(target, index, target.Length - index); }
    private static void CheckRange(int length, int index, int count)
    {
        if ((uint)index > (uint)length) throw new ArgumentOutOfRangeException(nameof(index));
        if ((uint)count > (uint)(length - index)) throw new ArgumentOutOfRangeException(nameof(count));
    }
    private static void Require(string? value, string name) { if (value is null) throw new ArgumentNullException(name); }
    private static void Require<T>(T[]? value, string name) { if (value is null) throw new ArgumentNullException(name); }
}
