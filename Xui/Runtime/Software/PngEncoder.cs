using System.IO.Compression;
using System.Buffers.Binary;
using System.IO;
using System;

namespace Xui.Runtime.Software
{
    public class PngEncoder
    {
        static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };

        public static void SaveRGBA(string path, byte[] rgbaPixels, int width, int height)
        {
            using var file = File.Create(path);
            SaveRGBA(file, rgbaPixels, width, height);
        }

        /// <summary>Encodes PNG data without closing the caller's stream.</summary>
        public static void SaveRGBA(Stream stream, byte[] rgbaPixels, int width, int height)
        {
            using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);

            writer.Write(PngSignature);

            WriteIHDR(writer, width, height);
            WriteIDAT(writer, rgbaPixels, width, height);
            WriteChunk(writer, "IEND", Array.Empty<byte>());
        }

        static void WriteIHDR(BinaryWriter w, int width, int height)
        {
            Span<byte> data = stackalloc byte[13];
            BinaryPrimitives.WriteInt32BigEndian(data[0..4], width);
            BinaryPrimitives.WriteInt32BigEndian(data[4..8], height);
            data[8] = 8;   // bit depth
            data[9] = 6;   // RGBA
            data[10] = 0;  // compression
            data[11] = 0;  // filter
            data[12] = 0;  // interlace
            WriteChunk(w, "IHDR", data);
        }

        static void WriteIDAT(BinaryWriter w, byte[] pixels, int width, int height)
        {
            int stride = width * 4;

            using var ms = new MemoryStream();

            // First, write 2-byte zlib header (CMF and FLG)
            ms.WriteByte(0x78); // CMF (compression method & flags: deflate, 32k window)
            ms.WriteByte(0x01); // FLG (flags: no dictionary, fastest compression)

            // Prepare data to compress, and compute Adler32 checksum simultaneously
            using var rawData = new MemoryStream();

            for (int y = 0; y < height; y++)
            {
                rawData.WriteByte(0); // filter type 0 (none)
                rawData.Write(pixels, y * stride, stride);
            }

            byte[] rawBytes = rawData.ToArray();

            // Compress using raw Deflate
            using (var deflate = new DeflateStream(ms, CompressionLevel.Fastest, true))
            {
                deflate.Write(rawBytes, 0, rawBytes.Length);
            } // Important: close DeflateStream to flush properly

            // Calculate Adler32 checksum of the uncompressed data
            uint adler32 = Adler32(rawBytes);
            Span<byte> adlerBytes = stackalloc byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(adlerBytes, adler32);
            ms.Write(adlerBytes);

            // Write IDAT chunk
            WriteChunk(w, "IDAT", ms.ToArray());
        }

        static uint Adler32(byte[] data)
        {
            const uint ModAdler = 65521;
            uint a = 1, b = 0;
            foreach (byte d in data)
            {
                a = (a + d) % ModAdler;
                b = (b + a) % ModAdler;
            }
            return (b << 16) | a;
        }

        static void WriteChunk(BinaryWriter w, string type, ReadOnlySpan<byte> data)
        {
            w.Write(BinaryPrimitives.ReverseEndianness(data.Length));
            var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
            w.Write(typeBytes);
            w.Write(data);

            uint crc = Crc32(typeBytes, data);
            w.Write(BinaryPrimitives.ReverseEndianness(crc));
        }

        static uint Crc32(ReadOnlySpan<byte> type, ReadOnlySpan<byte> data)
        {
            uint crc = 0xffffffff;
            foreach (var b in type) crc = (crc >> 8) ^ CrcTable[(crc ^ b) & 0xff];
            foreach (var b in data) crc = (crc >> 8) ^ CrcTable[(crc ^ b) & 0xff];
            return crc ^ 0xffffffff;
        }

        static readonly uint[] CrcTable = GenerateCrcTable();
        static uint[] GenerateCrcTable()
        {
            uint[] crcTable = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                uint c = n;
                for (int k = 0; k < 8; k++)
                    c = (c & 1) == 1 ? 0xedb88320 ^ (c >> 1) : c >> 1;
                crcTable[n] = c;
            }
            return crcTable;
        }

        public static void SaveRGBA(string path, RGBABitmap bitmap)
        {
            int width = (int)bitmap.Width;
            int height = (int)bitmap.Height;
            byte[] rgba = new byte[width * height * 4];

            int index = 0;
            for (uint y = 0; y < bitmap.Height; y++)
            {
                for (uint x = 0; x < bitmap.Width; x++)
                {
                    var c = bitmap[x, y];

                    byte r = c.Red;
                    byte g = c.Green;
                    byte b = c.Blue;
                    byte a = c.Alpha;

                    if (a != 0)
                    {
                        r = (byte)Math.Min(255, (r * 255) / a);
                        g = (byte)Math.Min(255, (g * 255) / a);
                        b = (byte)Math.Min(255, (b * 255) / a);
                    }
                    else
                    {
                        r = g = b = 0;
                    }

                    rgba[index++] = r;
                    rgba[index++] = g;
                    rgba[index++] = b;
                    rgba[index++] = a;
                }
            }

            SaveRGBA(path, rgba, width, height);
        }
    }
}
