namespace Xui.Compiler.Runtime.Text;

/// <summary>UTF-8 conversion over managed spans with replacement or strict fallback.</summary>
/// <remarks>Invalid input consumes the Unicode maximal subpart. No GC pointer or locale service is required.</remarks>
public class UTF8Encoding : Encoding
{
    private readonly bool emitPreamble;
    private readonly bool strict;
    public UTF8Encoding() : this(false, false) { }
    public UTF8Encoding(bool encoderShouldEmitUTF8Identifier) : this(encoderShouldEmitUTF8Identifier, false) { }
    public UTF8Encoding(bool encoderShouldEmitUTF8Identifier, bool throwOnInvalidBytes)
    { emitPreamble = encoderShouldEmitUTF8Identifier; strict = throwOnInvalidBytes; }
    public override int CodePage => 65001;
    public override string WebName => "utf-8";
    public override byte[] GetPreamble() => emitPreamble ? [0xef, 0xbb, 0xbf] : [];
    public override int GetMaxByteCount(int charCount)
    {
        if (charCount < 0 || charCount > int.MaxValue / 3 - 1) throw new ArgumentOutOfRangeException(nameof(charCount));
        return (charCount + 1) * 3;
    }
    public override int GetMaxCharCount(int byteCount)
    {
        if (byteCount < 0 || byteCount == int.MaxValue) throw new ArgumentOutOfRangeException(nameof(byteCount));
        return byteCount + 1;
    }
    public override int GetCharCount(ReadOnlySpan<byte> bytes)
    {
        int count = 0, index = 0;
        while (index < bytes.Length) count = checked(count + (ReadScalar(bytes, ref index) > 0xffff ? 2 : 1));
        return count;
    }
    public override int GetChars(ReadOnlySpan<byte> bytes, Span<char> chars)
    {
        if (GetCharCount(bytes) > chars.Length) throw new ArgumentException("The output buffer is too small.", nameof(chars));
        int index = 0, output = 0;
        while (index < bytes.Length)
        {
            int scalar = ReadScalar(bytes, ref index);
            if (scalar <= 0xffff) chars[output++] = (char)scalar;
            else { scalar -= 0x10000; chars[output++] = (char)(0xd800 + (scalar >> 10)); chars[output++] = (char)(0xdc00 + (scalar & 0x3ff)); }
        }
        return output;
    }
    public override int GetByteCount(ReadOnlySpan<char> chars)
    {
        int count = 0, index = 0;
        while (index < chars.Length)
        {
            int scalar = ReadScalar(chars, ref index);
            count = checked(count + (scalar < 0x80 ? 1 : scalar < 0x800 ? 2 : scalar < 0x10000 ? 3 : 4));
        }
        return count;
    }
    public override int GetBytes(ReadOnlySpan<char> chars, Span<byte> bytes)
    {
        if (GetByteCount(chars) > bytes.Length) throw new ArgumentException("The output buffer is too small.", nameof(bytes));
        int index = 0, output = 0;
        while (index < chars.Length)
        {
            int scalar = ReadScalar(chars, ref index);
            if (scalar < 0x80) bytes[output++] = (byte)scalar;
            else if (scalar < 0x800)
            { bytes[output++] = (byte)(0xc0 | scalar >> 6); bytes[output++] = (byte)(0x80 | scalar & 0x3f); }
            else if (scalar < 0x10000)
            { bytes[output++] = (byte)(0xe0 | scalar >> 12); bytes[output++] = (byte)(0x80 | scalar >> 6 & 0x3f); bytes[output++] = (byte)(0x80 | scalar & 0x3f); }
            else
            { bytes[output++] = (byte)(0xf0 | scalar >> 18); bytes[output++] = (byte)(0x80 | scalar >> 12 & 0x3f); bytes[output++] = (byte)(0x80 | scalar >> 6 & 0x3f); bytes[output++] = (byte)(0x80 | scalar & 0x3f); }
        }
        return output;
    }

    private int ReadScalar(ReadOnlySpan<char> chars, ref int index)
    {
        int first = chars[index++];
        if (first < 0xd800 || first > 0xdfff) return first;
        if (first < 0xdc00 && index < chars.Length && chars[index] is >= '\udc00' and <= '\udfff')
            return 0x10000 + ((first - 0xd800) << 10) + chars[index++] - 0xdc00;
        if (strict) throw new System.Text.EncoderFallbackException();
        return 0xfffd;
    }
    private int ReadScalar(ReadOnlySpan<byte> bytes, ref int index)
    {
        int first = bytes[index++];
        if (first < 0x80) return first;
        int remaining, scalar, lower = 0x80, upper = 0xbf;
        if (first is >= 0xc2 and <= 0xdf) { remaining = 1; scalar = first & 0x1f; }
        else if (first is >= 0xe0 and <= 0xef)
        { remaining = 2; scalar = first & 0xf; if (first == 0xe0) lower = 0xa0; if (first == 0xed) upper = 0x9f; }
        else if (first is >= 0xf0 and <= 0xf4)
        { remaining = 3; scalar = first & 7; if (first == 0xf0) lower = 0x90; if (first == 0xf4) upper = 0x8f; }
        else return InvalidBytes();
        for (int part = 0; part < remaining; part++)
        {
            if (index == bytes.Length || bytes[index] < lower || bytes[index] > upper) return InvalidBytes();
            scalar = (scalar << 6) | (bytes[index++] & 0x3f);
            lower = 0x80; upper = 0xbf;
        }
        return scalar;
    }
    private int InvalidBytes()
    {
        if (strict) throw new System.Text.DecoderFallbackException();
        return 0xfffd;
    }
}
