using System;
using System.Collections.Generic;
using System.Linq;
using Xui.GPU.Hardware;
using Xui.GPU.Shaders.Types;

namespace Xui.GPU.Software;

/// <summary>Deterministic RGBA8 sampling with bilinear filtering and trilinear mip selection.</summary>
public sealed class SampledTexture : ISampledTexture<Color4>
{
    private readonly byte[][] levels;
    private readonly int width, height;

    /// <summary>Copies validated premultiplied RGBA levels, largest first.</summary>
    public SampledTexture(int width, int height, IReadOnlyList<byte[]> mipLevels)
    {
        GpuTextureValidation.Validate(width, height, mipLevels);
        this.width = width; this.height = height;
        levels = mipLevels.Select(x => x.ToArray()).ToArray();
    }

    /// <inheritdoc/>
    public Color4 Sample(Sampler sampler, Float2 uv, F32 level)
    {
        float lod = Math.Clamp((float)level, 0, levels.Length - 1);
        int low = (int)lod, high = Math.Min(low + 1, levels.Length - 1);
        float weight = lod - low;
        var filter = (float)level <= 0 ? sampler.MagFilter : sampler.MinFilter;
        return At(low, uv, sampler, filter) * (1 - weight) + At(high, uv, sampler, filter) * weight;
    }

    private Color4 At(int level, Float2 uv, Sampler sampler, FilterMode filter)
    {
        int w = Math.Max(1, width >> level), h = Math.Max(1, height >> level);
        float x = (float)uv.X * w - .5f, y = (float)uv.Y * h - .5f;
        int ix = (int)MathF.Floor(x), iy = (int)MathF.Floor(y);
        if (filter == FilterMode.Point) return Texel((int)MathF.Floor(x + .5f), (int)MathF.Floor(y + .5f));
        float fx = x - ix, fy = y - iy;
        return (Texel(ix, iy) * (1 - fx) + Texel(ix + 1, iy) * fx) * (1 - fy)
            + (Texel(ix, iy + 1) * (1 - fx) + Texel(ix + 1, iy + 1) * fx) * fy;

        Color4 Texel(int tx, int ty)
        {
            tx = Address(tx, w, sampler.AddressU); ty = Address(ty, h, sampler.AddressV);
            if (tx < 0 || ty < 0) return Color4.Transparent;
            int i = (ty * w + tx) * 4;
            var data = levels[level];
            return new(data[i] / 255f, data[i + 1] / 255f, data[i + 2] / 255f, data[i + 3] / 255f);
        }
    }

    private static int Address(int x, int size, AddressMode mode) => mode switch
    {
        AddressMode.ClampToEdge => Math.Clamp(x, 0, size - 1),
        AddressMode.ClampToBorder => x < 0 || x >= size ? -1 : x,
        AddressMode.Repeat => (x % size + size) % size,
        AddressMode.MirroredRepeat => Mirror((x % (size * 2) + size * 2) % (size * 2), size),
        _ => throw new ArgumentOutOfRangeException(nameof(mode)),
    };
    private static int Mirror(int x, int size) => x < size ? x : size * 2 - 1 - x;
}
