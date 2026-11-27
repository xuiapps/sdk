namespace Xui.Compiler.Runtime.Text;

/// <summary>Stateful managed-array decoding surface used by streaming readers.</summary>
public abstract class Decoder
{
    public abstract int GetCharCount(byte[] bytes, int index, int count);
    public virtual int GetCharCount(byte[] bytes, int index, int count, bool flush) => GetCharCount(bytes, index, count);
    public abstract int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex);
    public virtual int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, bool flush) =>
        GetChars(bytes, byteIndex, byteCount, chars, charIndex);
    public virtual void Reset() { }

    public virtual void Convert(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, int charCount,
        bool flush, out int bytesUsed, out int charsUsed, out bool completed)
    {
        _ = Read(bytes, byteIndex, byteCount);
        _ = Write(chars, charIndex, charCount);
        int count = byteCount;
        while (GetCharCount(bytes, byteIndex, count, flush) > charCount)
        {
            if (count == 0) throw new ArgumentException("The output buffer is too small.", nameof(chars));
            count /= 2; flush = false;
            if (count == 0) throw new ArgumentException("The output buffer is too small.", nameof(chars));
        }
        charsUsed = GetChars(bytes, byteIndex, count, chars, charIndex, flush);
        bytesUsed = count; completed = count == byteCount;
    }

    protected static ReadOnlySpan<byte> Read(byte[] bytes, int index, int count)
    {
        if (bytes is null) throw new ArgumentNullException(nameof(bytes));
        if ((uint)index > (uint)bytes.Length) throw new ArgumentOutOfRangeException(nameof(index));
        if ((uint)count > (uint)(bytes.Length - index)) throw new ArgumentOutOfRangeException(nameof(count));
        return new ReadOnlySpan<byte>(bytes, index, count);
    }
    protected static Span<char> Write(char[] chars, int index, int count)
    {
        if (chars is null) throw new ArgumentNullException(nameof(chars));
        if ((uint)index > (uint)chars.Length) throw new ArgumentOutOfRangeException(nameof(index));
        if ((uint)count > (uint)(chars.Length - index)) throw new ArgumentOutOfRangeException(nameof(count));
        return new Span<char>(chars, index, count);
    }
}
