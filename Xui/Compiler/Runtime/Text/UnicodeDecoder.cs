namespace Xui.Compiler.Runtime.Text;

/// <summary>Carries incomplete UTF-8/UTF-16 byte sequences between reads without pinning GC arrays.</summary>
internal sealed class UnicodeDecoder(Encoding encoding) : Decoder
{
    private readonly byte[] pending = new byte[4];
    private int pendingCount;
    public override void Reset() => pendingCount = 0;
    public override int GetCharCount(byte[] bytes, int index, int count) => GetCharCount(bytes, index, count, false);
    public override int GetCharCount(byte[] bytes, int index, int count, bool flush)
    {
        var input = Join(Read(bytes, index, count));
        return encoding.GetCharCount(input.Slice(0, input.Length - Tail(input, flush)));
    }
    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) =>
        GetChars(bytes, byteIndex, byteCount, chars, charIndex, false);
    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, bool flush)
    {
        var input = Join(Read(bytes, byteIndex, byteCount));
        if (chars is null) throw new ArgumentNullException(nameof(chars));
        var output = Write(chars, charIndex, chars.Length - charIndex);
        int tail = Tail(input, flush), prefix = input.Length - tail;
        int written = encoding.GetChars(input.Slice(0, prefix), output);
        for (int i = 0; i < tail; i++) pending[i] = input[prefix + i];
        pendingCount = tail;
        return written;
    }
    private ReadOnlySpan<byte> Join(ReadOnlySpan<byte> bytes)
    {
        if (pendingCount == 0) return bytes;
        var combined = new byte[checked(bytes.Length + pendingCount)];
        for (int i = 0; i < pendingCount; i++) combined[i] = pending[i];
        bytes.CopyTo(new Span<byte>(combined, pendingCount, bytes.Length));
        return combined;
    }
    private int Tail(ReadOnlySpan<byte> bytes, bool flush)
    {
        if (flush || bytes.Length == 0) return 0;
        if (encoding.CodePage is 1200 or 1201)
        {
            int tail = bytes.Length % 2, last = bytes.Length - tail - 2;
            if (last >= 0)
            {
                int unit = encoding.CodePage == 1200 ? bytes[last] + bytes[last + 1] * 256 : bytes[last] * 256 + bytes[last + 1];
                if (unit is >= 0xd800 and <= 0xdbff) tail += 2;
            }
            return tail;
        }
        int start = bytes.Length - 1;
        while (start > 0 && bytes[start] is >= 0x80 and <= 0xbf && bytes.Length - start < 4) start--;
        int lead = bytes[start], count = bytes.Length - start;
        int length = lead is >= 0xc2 and <= 0xdf ? 2 : lead is >= 0xe0 and <= 0xef ? 3 : lead is >= 0xf0 and <= 0xf4 ? 4 : 0;
        if (length == 0 || count >= length) return 0;
        for (int i = 1; i < count; i++)
        {
            int low = i == 1 && lead == 0xe0 ? 0xa0 : i == 1 && lead == 0xf0 ? 0x90 : 0x80;
            int high = i == 1 && lead == 0xed ? 0x9f : i == 1 && lead == 0xf4 ? 0x8f : 0xbf;
            if (bytes[start + i] < low || bytes[start + i] > high) return 0;
        }
        return count;
    }
}
