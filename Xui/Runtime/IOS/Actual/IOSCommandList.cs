using Xui.GPU.Shaders.Types;
using NativeMetal = global::Xui.Runtime.IOS.Metal;
using System;
using System.Runtime.InteropServices;

namespace Xui.GPU.Hardware.Metal;

/// <summary>
/// Metal GPU command list for recording and executing rendering commands.
/// </summary>
internal sealed unsafe class IOSCommandList : IGpuCommandList
{
    private readonly nint _device;
    private readonly nint _commandQueue;

    // Current frame state
    private nint _commandBuffer;     // id<MTLCommandBuffer>
    private nint _encoder;           // id<MTLRenderCommandEncoder>
    private nint _depthStencilState;
    private bool _depthTest, _depthWrite;
    private nint _pipelineState;     // id<MTLRenderPipelineState>
    private IIOSRenderTarget? _currentRenderTarget;
    private IOSVertexShader? _currentVertexShader;
    private IOSFragmentShader? _currentFragmentShader;
    private nint _vertexBuffer;
    private int _vertexCount;
    private nint _constantBuffer;
    private bool _disposed;

    internal IOSCommandList(nint device, nint commandQueue)
    {
        _device = device;
        _commandQueue = commandQueue;
    }

    /// <inheritdoc/>
    public void BeginRenderPass(IGpuRenderTarget renderTarget, Color4 clearColor)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _currentRenderTarget = renderTarget as IIOSRenderTarget
            ?? throw new ArgumentException("The target must belong to the iOS Metal backend.", nameof(renderTarget));
        if (_currentRenderTarget.NativeTexture == 0)
            throw new InvalidOperationException("The presentation target is only valid during its render callback.");

        // Create command buffer (autoreleased — must retain for our ownership)
        _commandBuffer = NativeMetal.Retain(NativeMetal.CommandQueue_CommandBuffer(_commandQueue));
        if (_commandBuffer == 0)
            throw new InvalidOperationException("Failed to create Metal command buffer.");

        // Create render pass descriptor with our render target (including depth texture)
        var passDesc = NativeMetal.CreateRenderPassDescriptor(
            _currentRenderTarget.NativeTexture, clearColor, _currentRenderTarget.NativeDepthTexture);

        // Create render command encoder (autoreleased — must retain for our ownership)
        _encoder = NativeMetal.Retain(
            NativeMetal.CommandBuffer_RenderCommandEncoderWithDescriptor(_commandBuffer, passDesc));

        NativeMetal.Release(passDesc);

        if (_encoder == 0)
            throw new InvalidOperationException("Failed to create Metal render command encoder.");
    }

    /// <inheritdoc/>
    public void EndRenderPass()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        NativeMetal.Encoder_EndEncoding(_encoder);
        NativeMetal.Release(_encoder);
        _encoder = 0;
    }

    /// <inheritdoc/>
    public void SetPipeline(GpuPipelineDesc pipeline)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_pipelineState != 0 && ReferenceEquals(_currentVertexShader, pipeline.VertexShader) &&
            ReferenceEquals(_currentFragmentShader, pipeline.FragmentShader) &&
            _depthTest == pipeline.DepthTestEnabled && _depthWrite == pipeline.DepthWriteEnabled)
        {
            NativeMetal.Encoder_SetRenderPipelineState(_encoder, _pipelineState);
            NativeMetal.Encoder_SetDepthStencilState(_encoder, _depthStencilState);
            ApplyRasterState(pipeline);
            return;
        }

        _currentVertexShader = pipeline.VertexShader as IOSVertexShader;
        _currentFragmentShader = pipeline.FragmentShader as IOSFragmentShader;

        if (_currentVertexShader == null)
            throw new InvalidOperationException("Pipeline requires a IOSVertexShader.");
        if (_currentFragmentShader == null)
            throw new InvalidOperationException("Pipeline requires a IOSFragmentShader.");

        // Create vertex descriptor for CubeVertex: Float3 Position (12 bytes) + Color4 Color (16 bytes)
        // This tells the hardware vertex fetch unit how to read tightly-packed C# data.
        var vertexDesc = NativeMetal.CreateVertexDescriptor(
            [
                new() { Format = 30, Offset = 0, BufferIndex = 0 },   // MTLVertexFormatFloat3 at offset 0
                new() { Format = 31, Offset = 12, BufferIndex = 0 },  // MTLVertexFormatFloat4 at offset 12
            ],
            stride: 28);

        // Create render pipeline state with vertex descriptor, vertex and fragment functions
        // MTLPixelFormatDepth32Float = 252
        var pipelineDesc = NativeMetal.CreateRenderPipelineDescriptor(
            _currentVertexShader.Function, _currentFragmentShader.Function, vertexDesc, depthPixelFormat: 252);
        NativeMetal.Release(vertexDesc);

        // Release old pipeline state if any
        if (_pipelineState != 0)
        {
            NativeMetal.Release(_pipelineState);
            _pipelineState = 0;
        }

        _pipelineState = NativeMetal.Device_NewRenderPipelineState(
            _device, pipelineDesc, out nint error);

        NativeMetal.Release(pipelineDesc);

        if (_pipelineState == 0 || error != 0)
        {
            var errorDesc = NativeMetal.GetErrorDescription(error);
            throw new InvalidOperationException($"Failed to create Metal render pipeline state:\n{errorDesc}");
        }

        // Set pipeline state on encoder
        NativeMetal.Encoder_SetRenderPipelineState(_encoder, _pipelineState);

        // Create and set depth stencil state
        var dsDesc = NativeMetal.CreateDepthStencilDescriptor(pipeline.DepthTestEnabled, pipeline.DepthWriteEnabled);
        if (_depthStencilState != 0) NativeMetal.Release(_depthStencilState);
        _depthStencilState = NativeMetal.Device_NewDepthStencilState(_device, dsDesc);
        NativeMetal.Release(dsDesc);
        _depthTest = pipeline.DepthTestEnabled; _depthWrite = pipeline.DepthWriteEnabled;
        NativeMetal.Encoder_SetDepthStencilState(_encoder, _depthStencilState);
        ApplyRasterState(pipeline);
    }

    private void ApplyRasterState(GpuPipelineDesc pipeline)
    {
        // Set cull mode: GpuCullMode maps to MTLCullMode (None=0, Front=1, Back=2)
        ulong mtlCullMode = pipeline.CullMode switch
        {
            GpuCullMode.None => 0UL,   // MTLCullModeNone
            GpuCullMode.Front => 1UL,  // MTLCullModeFront
            GpuCullMode.Back => 2UL,   // MTLCullModeBack
            _ => 0UL,
        };
        NativeMetal.Encoder_SetCullMode(_encoder, mtlCullMode);

        // Set front-facing winding to counter-clockwise (matching OpenGL/D3D convention)
        NativeMetal.Encoder_SetFrontFacingWinding(_encoder, 1); // MTLWindingCounterClockwise = 1
    }

    /// <inheritdoc/>
    public void SetVertexBuffer(void* data, GpuVertexBufferDesc desc)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _vertexCount = desc.VertexCount;

        // Release old vertex buffer
        if (_vertexBuffer != 0)
        {
            NativeMetal.Release(_vertexBuffer);
            _vertexBuffer = 0;
        }

        // Create vertex buffer - MTLResourceStorageModeShared = 0
        nuint byteLength = (nuint)(desc.Stride * desc.VertexCount);
        _vertexBuffer = NativeMetal.Device_NewBufferWithBytes(_device, (nint)data, byteLength, 0);

        if (_vertexBuffer == 0)
            throw new InvalidOperationException("Failed to create Metal vertex buffer.");

        // Bind vertex buffer to slot 0
        NativeMetal.Encoder_SetVertexBuffer(_encoder, _vertexBuffer, 0, 0);
    }

    /// <inheritdoc/>
    public void SetConstantBuffer(void* data, int sizeInBytes)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Release old constant buffer
        if (_constantBuffer != 0)
        {
            NativeMetal.Release(_constantBuffer);
            _constantBuffer = 0;
        }

        if (data != null && sizeInBytes > 0)
        {
            // Create constant buffer
            _constantBuffer = NativeMetal.Device_NewBufferWithBytes(
                _device, (nint)data, (nuint)sizeInBytes, 0);

            if (_constantBuffer == 0)
                throw new InvalidOperationException("Failed to create Metal constant buffer.");

            // Bind to vertex shader buffer slot 1 (slot 0 is vertex data)
            NativeMetal.Encoder_SetVertexBuffer(_encoder, _constantBuffer, 0, 1);
        }
    }

    /// <inheritdoc/>
    public void Draw(int vertexCount)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        // MTLPrimitiveTypeTriangle = 3
        NativeMetal.Encoder_DrawPrimitives(_encoder, 3, 0, (ulong)vertexCount);
    }

    /// <inheritdoc/>
    public void Commit()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_encoder != 0) throw new InvalidOperationException("End the render pass before submitting it.");
        if (_commandBuffer == 0) throw new InvalidOperationException("No render commands to submit.");
        var drawable = _currentRenderTarget?.Drawable ?? 0;
        if (drawable != 0) NativeMetal.CommandBuffer_PresentDrawable(_commandBuffer, drawable);
        NativeMetal.CommandBuffer_Commit(_commandBuffer);
        if (drawable == 0) NativeMetal.CommandBuffer_WaitUntilCompleted(_commandBuffer);
        else _currentRenderTarget!.MarkPresented();

        // Submitted command buffers retain their resources until GPU completion.
        NativeMetal.Release(_commandBuffer);
        _commandBuffer = 0;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_encoder != 0) NativeMetal.Release(_encoder);
        if (_vertexBuffer != 0) NativeMetal.Release(_vertexBuffer);
        if (_constantBuffer != 0) NativeMetal.Release(_constantBuffer);
        if (_pipelineState != 0) NativeMetal.Release(_pipelineState);
        if (_depthStencilState != 0) NativeMetal.Release(_depthStencilState);
        if (_commandBuffer != 0) NativeMetal.Release(_commandBuffer);
    }
}
