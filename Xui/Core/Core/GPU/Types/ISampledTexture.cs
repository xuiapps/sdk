namespace Xui.GPU.Shaders.Types;

/// <summary>CPU implementation of a shader texture resource; pixel storage belongs to the renderer.</summary>
/// <typeparam name="TPixel">Pixel result type.</typeparam>
public interface ISampledTexture<TPixel> where TPixel : unmanaged
{
    /// <summary>Samples normalized coordinates at an explicit mip level.</summary>
    TPixel Sample(Sampler sampler, Float2 uv, F32 level);
}
