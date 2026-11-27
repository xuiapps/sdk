namespace Xui.Compiler.Runtime.Text;

/// <summary>A mutable UTF-16 buffer which materializes native strings without pinning GC storage.</summary>
/// <remarks>This explicit browser profile implements the append/clear/slice surface used by XML and records.</remarks>
public sealed class StringBuilder
{
    private char[] buffer;
    private int length;

    public StringBuilder() : this(16) { }
    public StringBuilder(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        buffer = new char[capacity == 0 ? 16 : capacity];
    }
    public StringBuilder(string? value) : this(value, 16) { }
    public StringBuilder(string? value, int capacity) : this(capacity)
    {
        if (value is not null && value.Length > Capacity) Capacity = value.Length;
        Append(value);
    }

    public int MaxCapacity => int.MaxValue;
    public int Capacity
    {
        get => buffer.Length;
        set
        {
            if (value < 0 || value < length) throw new ArgumentOutOfRangeException(nameof(value));
            if (value == buffer.Length) return;
            var replacement = new char[value];
            new ReadOnlySpan<char>(buffer, 0, length).CopyTo(replacement);
            buffer = replacement;
        }
    }
    public int Length
    {
        get => length;
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            EnsureCapacity(value);
            for (int i = length; i < value; i++) buffer[i] = '\0';
            length = value;
        }
    }
    [System.Runtime.CompilerServices.IndexerName("Chars")]
    public char this[int index]
    {
        get => (uint)index < (uint)length ? buffer[index] : throw new IndexOutOfRangeException();
        set
        {
            if ((uint)index >= (uint)length) throw new ArgumentOutOfRangeException(nameof(index));
            buffer[index] = value;
        }
    }
    public int EnsureCapacity(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (capacity > buffer.Length)
        {
            int doubled = buffer.Length <= int.MaxValue / 2 ? buffer.Length + buffer.Length : int.MaxValue;
            Capacity = capacity > doubled ? capacity : doubled;
        }
        return buffer.Length;
    }
    public StringBuilder Clear() { length = 0; return this; }
    public StringBuilder Append(char value)
    {
        EnsureCapacity(checked(length + 1));
        buffer[length++] = value;
        return this;
    }
    public StringBuilder Append(char value, int repeatCount)
    {
        if (repeatCount < 0) throw new ArgumentOutOfRangeException(nameof(repeatCount));
        int end = checked(length + repeatCount);
        EnsureCapacity(end);
        while (length < end) buffer[length++] = value;
        return this;
    }
    public StringBuilder Append(string? value) => value is null ? this : Append(value.AsSpan());
    public StringBuilder Append(object? value) => Append(value?.ToString());
    public StringBuilder Append(char[]? value) => value is null ? this : Append((ReadOnlySpan<char>)value);
    public StringBuilder Append(ReadOnlySpan<char> value)
    {
        int end = checked(length + value.Length);
        EnsureCapacity(end);
        value.CopyTo(new Span<char>(buffer, length, value.Length));
        length = end;
        return this;
    }
    public StringBuilder Append(string? value, int startIndex, int count)
    {
        if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (value is null)
        {
            if (startIndex == 0 && count == 0) return this;
            throw new ArgumentNullException(nameof(value));
        }
        if (startIndex > value.Length - count) throw new ArgumentOutOfRangeException(nameof(startIndex));
        return Append(value.AsSpan(startIndex, count));
    }
    public StringBuilder Append(char[]? value, int startIndex, int charCount)
    {
        if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
        if (charCount < 0) throw new ArgumentOutOfRangeException(nameof(charCount));
        if (value is null)
        {
            if (startIndex == 0 && charCount == 0) return this;
            throw new ArgumentNullException(nameof(value));
        }
        if (startIndex > value.Length - charCount) throw new ArgumentOutOfRangeException(nameof(charCount));
        return Append(new ReadOnlySpan<char>(value, startIndex, charCount));
    }
    public StringBuilder AppendLine() => Append('\n');
    public StringBuilder AppendLine(string? value) => Append(value).AppendLine();
    public override string ToString() => new(buffer, 0, length);
    public string ToString(int startIndex, int length)
    {
        if (startIndex < 0 || startIndex > this.length) throw new ArgumentOutOfRangeException(nameof(startIndex));
        if (length < 0 || length > this.length - startIndex) throw new ArgumentOutOfRangeException(nameof(length));
        return new string(buffer, startIndex, length);
    }
}
