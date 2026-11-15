namespace Xui.GPU.Hardware;

/// <summary>
/// Represents a GPU command list / draw context for recording rendering commands.
/// </summary>
public interface IGpuCommandList : IDisposable
{
    /// <summary>
    /// Begins a render pass targeting the given render target.
    /// </summary>
    void BeginRenderPass(IGpuRenderTarget renderTarget, Xui.GPU.Shaders.Types.Color4 clearColor);

    /// <summary>
    /// Ends the current render pass.
    /// </summary>
    void EndRenderPass();

    /// <summary>
    /// Sets the pipeline state for subsequent draw calls.
    /// </summary>
    void SetPipeline(GpuPipelineDesc pipeline);

    /// <summary>
    /// Sets the vertex buffer for subsequent draw calls.
    /// </summary>
    /// <param name="data">Pointer to vertex data.</param>
    /// <param name="desc">Vertex buffer description.</param>
    unsafe void SetVertexBuffer(void* data, GpuVertexBufferDesc desc);

    /// <summary>
    /// Sets the uniform/constant buffer data.
    /// </summary>
    /// <param name="data">Pointer to constant data.</param>
    /// <param name="sizeInBytes">Size of the constant data in bytes.</param>
    unsafe void SetConstantBuffer(void* data, int sizeInBytes);

    /// <summary>
    /// Draws primitives using the current pipeline and vertex buffer.
    /// </summary>
    /// <param name="vertexCount">Number of vertices to draw.</param>
    void Draw(int vertexCount);

    /// <summary>
    /// Submits all recorded commands. Native presentation targets schedule presentation
    /// asynchronously; submission does not mean that the frame has finished displaying.
    /// Offscreen readback uses the backend's completion/synchronization mechanism.
    /// </summary>
    void Commit();
}
