namespace Xui.GPU.Hardware;

/// <summary>Shared validation for tightly packed RGBA8 mip chains.</summary>
public static class GpuTextureValidation
{
    /// <summary>Checks dimensions and each mip's byte length before any native allocation.</summary>
    public static void Validate(int width, int height, IReadOnlyList<byte[]> levels)
    {
        ArgumentNullException.ThrowIfNull(levels);
        if (width <= 0 || height <= 0 || levels.Count == 0) throw new ArgumentOutOfRangeException(nameof(width));
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i] is null || levels[i].Length != checked(width * height * 4))
                throw new ArgumentException("Invalid RGBA mip length.", nameof(levels));
            if (width == 1 && height == 1 && i != levels.Count - 1)
                throw new ArgumentException("Too many mip levels.", nameof(levels));
            width = Math.Max(1, width / 2); height = Math.Max(1, height / 2);
        }
    }
}
