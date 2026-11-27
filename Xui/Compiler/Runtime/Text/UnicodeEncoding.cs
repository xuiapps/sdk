namespace Xui.Compiler.Runtime.Text;

/// <summary>UTF-16 little/big-endian conversion over managed spans, with strict or replacement fallback.</summary>
public class UnicodeEncoding : Encoding
{
    private readonly bool bigEndian, emitPreamble, strict;
    public UnicodeEncoding() : this(false, true, false) { }
    public UnicodeEncoding(bool bigEndian, bool byteOrderMark) : this(bigEndian, byteOrderMark, false) { }
    public UnicodeEncoding(bool bigEndian, bool byteOrderMark, bool throwOnInvalidBytes)
    { this.bigEndian = bigEndian; emitPreamble = byteOrderMark; strict = throwOnInvalidBytes; }
    public override int CodePage => bigEndian ? 1201 : 1200;
    public override string WebName => bigEndian ? "utf-16BE" : "utf-16";
    public override byte[] GetPreamble() => !emitPreamble ? [] : bigEndian ? [0xfe, 0xff] : [0xff, 0xfe];
    public override int GetMaxByteCount(int charCount)
    {
        if (charCount < 0 || charCount > int.MaxValue / 2 - 1) throw new ArgumentOutOfRangeException(nameof(charCount));
        return (charCount + 1) * 2;
    }
    public override int GetMaxCharCount(int byteCount)
    {
        if (byteCount < 0) throw new ArgumentOutOfRangeException(nameof(byteCount));
        return byteCount / 2 + byteCount % 2 + 1;
    }
    public override int GetByteCount(ReadOnlySpan<char> chars)
    {
        int index = 0;
        while (index < chars.Length) _ = ReadScalar(chars, ref index);
        return checked(chars.Length + chars.Length);
    }
    public override int GetBytes(ReadOnlySpan<char> chars, Span<byte> bytes)
    {
        if (GetByteCount(chars) > bytes.Length) throw new ArgumentException("The output buffer is too small.", nameof(bytes));
        int input = 0, output = 0;
        while (input < chars.Length)
        {
            int scalar = ReadScalar(chars, ref input);
            if (scalar <= 0xffff) WriteUnit(bytes, ref output, scalar);
            else
            {
                scalar -= 0x10000;
                WriteUnit(bytes, ref output, 0xd800 + (scalar >> 10));
                WriteUnit(bytes, ref output, 0xdc00 + (scalar & 0x3ff));
            }
        }
        return output;
    }
    public override int GetCharCount(ReadOnlySpan<byte> bytes)
    {
        int index = 0, count = 0;
        while (index < bytes.Length) count += ReadScalar(bytes, ref index) > 0xffff ? 2 : 1;
        return count;
    }
    public override int GetChars(ReadOnlySpan<byte> bytes, Span<char> chars)
    {
        if (GetCharCount(bytes) > chars.Length) throw new ArgumentException("The output buffer is too small.", nameof(chars));
        int input = 0, output = 0;
        while (input < bytes.Length)
        {
            int scalar = ReadScalar(bytes, ref input);
            if (scalar <= 0xffff) chars[output++] = (char)scalar;
            else
            {
                scalar -= 0x10000;
                chars[output++] = (char)(0xd800 + (scalar >> 10));
                chars[output++] = (char)(0xdc00 + (scalar & 0x3ff));
            }
        }
        return output;
    }
    private int ReadScalar(ReadOnlySpan<char> chars, ref int index)
    {
        int unit = chars[index++];
        if (unit < 0xd800 || unit > 0xdfff) return unit;
        if (unit < 0xdc00 && index < chars.Length && chars[index] is >= '\udc00' and <= '\udfff')
            return 0x10000 + ((unit - 0xd800) << 10) + chars[index++] - 0xdc00;
        if (strict) throw new System.Text.EncoderFallbackException();
        return 0xfffd;
    }
    private int ReadScalar(ReadOnlySpan<byte> bytes, ref int index)
    {
        if (bytes.Length - index < 2) { index++; return InvalidBytes(); }
        int unit = ReadUnit(bytes, index); index += 2;
        if (unit < 0xd800 || unit > 0xdfff) return unit;
        if (unit < 0xdc00 && bytes.Length - index >= 2)
        {
            int next = ReadUnit(bytes, index);
            if (next is >= 0xdc00 and <= 0xdfff)
            { index += 2; return 0x10000 + ((unit - 0xd800) << 10) + next - 0xdc00; }
        }
        return InvalidBytes();
    }
    private int ReadUnit(ReadOnlySpan<byte> bytes, int index) => bigEndian
        ? bytes[index] * 256 + bytes[index + 1] : bytes[index] + bytes[index + 1] * 256;
    private void WriteUnit(Span<byte> bytes, ref int index, int unit)
    {
        bytes[index++] = (byte)(bigEndian ? unit >> 8 : unit);
        bytes[index++] = (byte)(bigEndian ? unit : unit >> 8);
    }
    private int InvalidBytes()
    {
        if (strict) throw new System.Text.DecoderFallbackException();
        return 0xfffd;
    }
}
