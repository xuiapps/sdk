using System.Buffers.Binary;
using System.Text;
using Xui.Compiler.WASM.Wasm.Instructions;

namespace Xui.Compiler.WASM.Wasm.Section;

/// <summary>Writes a section or function-body payload and patches its length on disposal.</summary>
/// <remarks>
/// Reserves five bytes for a padded unsigned LEB128 length, which the WASM format permits.
/// Dispose nested function bodies before their containing section, and sections before
/// beginning the next section. Disposal restores the stream position
/// to the end of the payload and leaves the stream open.
/// </remarks>
public ref struct Writer
{
    private readonly Stream stream;
    private readonly long lengthPosition;
    private bool disposed;

    internal Writer(Stream stream)
    {
        this.stream = stream;
        lengthPosition = stream.Position;
        stream.Write([0x80, 0x80, 0x80, 0x80, 0]);
    }

    /// <summary>Writes a single byte, such as an opcode or type encoding.</summary>
    /// <param name="value">The byte to write.</param>
    /// <returns>This writer for chaining.</returns>
    public readonly Writer Byte(byte value)
    {
        stream.WriteByte(value);
        return this;
    }

    /// <summary>Writes an instruction opcode; the caller writes its immediate operands.</summary>
    internal readonly Writer Instruction(OpCode code) => Byte((byte)code);

    /// <summary>Writes a GC prefix and subopcode; the caller writes its immediate operands.</summary>
    internal readonly Writer Instruction(GcOpCode code) => Instruction(OpCode.Gc).U32((uint)code);

    /// <summary>Writes an unsigned 32-bit integer using unsigned LEB128 encoding.</summary>
    /// <param name="value">The integer to encode.</param>
    /// <returns>This writer for chaining.</returns>
    public readonly Writer U32(uint value)
    {
        do
        {
            byte part = (byte)(value & 0x7f);
            value >>= 7;
            stream.WriteByte((byte)(part | (value == 0 ? 0 : 0x80)));
        } while (value != 0);
        return this;
    }

    /// <summary>Writes a signed integer using signed LEB128 encoding.</summary>
    /// <param name="value">The integer to encode, including signed heap type indices.</param>
    /// <returns>This writer for chaining.</returns>
    public readonly Writer Signed(long value)
    {
        bool more;
        do
        {
            byte part = (byte)(value & 0x7f);
            value >>= 7;
            more = !((value == 0 && (part & 0x40) == 0) ||
                     (value == -1 && (part & 0x40) != 0));
            stream.WriteByte((byte)(part | (more ? 0x80 : 0)));
        } while (more);
        return this;
    }

    /// <summary>Writes an IEEE 754 binary32 value in little-endian byte order.</summary>
    public readonly Writer F32(float value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteSingleLittleEndian(bytes, value);
        stream.Write(bytes);
        return this;
    }

    /// <summary>Writes an IEEE 754 binary64 value in little-endian byte order.</summary>
    public readonly Writer F64(double value)
    {
        Span<byte> bytes = stackalloc byte[8];
        BinaryPrimitives.WriteDoubleLittleEndian(bytes, value);
        stream.Write(bytes);
        return this;
    }

    /// <summary>Writes a UTF-8 name prefixed by its byte length encoded as a U32.</summary>
    /// <param name="value">The name to encode.</param>
    /// <returns>This writer for chaining.</returns>
    public readonly Writer Name(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        return U32((uint)bytes.Length).Bytes(bytes);
    }

    /// <summary>Begins a length-prefixed function body within a code section.</summary>
    /// <returns>A nested scope to dispose before continuing the containing section.</returns>
    internal readonly Writer FunctionBody() => new(stream);

    private readonly Writer Bytes(ReadOnlySpan<byte> value)
    {
        stream.Write(value);
        return this;
    }

    internal readonly long ReserveU32()
    {
        long position = stream.Position;
        stream.Write([0x80, 0x80, 0x80, 0x80, 0]);
        return position;
    }

    /// <summary>Patches the payload length and restores the current stream position.</summary>
    public void Dispose()
    {
        if (disposed) return;
        uint length = checked((uint)(stream.Position - lengthPosition - 5));
        PatchU32(lengthPosition, length);
        disposed = true;
    }

    internal readonly void PatchU32(long position, uint value)
    {
        long end = stream.Position;
        stream.Position = position;
        try
        {
            for (int i = 0; i < 4; i++)
            {
                stream.WriteByte((byte)((value & 0x7f) | 0x80));
                value >>= 7;
            }
            stream.WriteByte((byte)value);
        }
        finally
        {
            stream.Position = end;
        }
    }
}
