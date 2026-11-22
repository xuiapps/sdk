using System;
using Xui.GPU.Shaders.Types;
using System.Runtime.InteropServices;
using Xui.Runtime.Windows;
using static Xui.Runtime.Windows.D3D11;
using static Xui.Runtime.Windows.COM;

namespace Xui.GPU.Hardware.D3D11;

/// <summary>
/// D3D11 GPU command list for recording and executing rendering commands.
/// </summary>
internal sealed unsafe class DirectXCommandList : IGpuCommandList
{
    private readonly Device _device;
    private readonly DeviceContext _context;

    // Currently bound state
    private DirectXRenderTarget? _currentRenderTarget;
    private DirectXVertexShader? _currentVertexShader;
    private DirectXFragmentShader? _currentFragmentShader;
    private void* _currentVertexBuffer;
    private uint _currentStride;
    private void* _currentConstantBuffer;
    private bool _disposed;

    internal DirectXCommandList(Device device, DeviceContext context)
    {
        _device = device;
        _context = context;
    }

    /// <inheritdoc/>
    public void BeginRenderPass(IGpuRenderTarget renderTarget, Color4 clearColor)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _currentRenderTarget = (DirectXRenderTarget)renderTarget;

        var rtv = _currentRenderTarget.NativeRtv;
        var dsv = _currentRenderTarget.NativeDsv;

        // Set render targets
        _context.OMSetRenderTargets(1, &rtv, dsv);

        // Clear color
        var color = stackalloc float[4] { clearColor.R, clearColor.G, clearColor.B, clearColor.A };
        _context.ClearRenderTargetView(rtv, color);

        // Clear depth (0x1 = DEPTH, 0x2 = STENCIL)
        _context.ClearDepthStencilView(dsv, 0x1 | 0x2, 1.0f, 0);

        // Set viewport
        var viewport = new Viewport
        {
            TopLeftX = 0,
            TopLeftY = 0,
            Width = renderTarget.Width,
            Height = renderTarget.Height,
            MinDepth = 0,
            MaxDepth = 1
        };
        _context.RSSetViewports(1, &viewport);
    }

    /// <inheritdoc/>
    public void EndRenderPass()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _currentRenderTarget = null;
    }

    /// <inheritdoc/>
    public void SetPipeline(GpuPipelineDesc pipeline)
    {
        if (pipeline.PremultipliedAlpha || pipeline.VertexComponents is not null)
            throw new NotSupportedException("Custom vertex layouts and premultiplied blending require the GPU resource capability.");
        ObjectDisposedException.ThrowIf(_disposed, this);

        _currentVertexShader = pipeline.VertexShader as DirectXVertexShader;
        _currentFragmentShader = pipeline.FragmentShader as DirectXFragmentShader;

        // Set vertex shader
        void* vsNative = _currentVertexShader != null ? _currentVertexShader.NativeShader : null;
        _context.VSSetShader(vsNative, null, 0);

        // Set pixel shader
        void* psNative = _currentFragmentShader != null ? _currentFragmentShader.NativeShader : null;
        _context.PSSetShader(psNative, null, 0);

        // Set depth stencil state
        var dsDesc = new DepthStencilDesc
        {
            DepthEnable = pipeline.DepthTestEnabled ? 1 : 0,
            DepthWriteMask = pipeline.DepthWriteEnabled ? 1u : 0u,
            DepthFunc = 4u,   // D3D11_COMPARISON_LESS
            StencilEnable = 0,
            StencilReadMask = 0xFF,
            StencilWriteMask = 0xFF,
            FrontFace = new DepthStencilOpDesc
            {
                StencilFailOp = 1u,     // D3D11_STENCIL_OP_KEEP
                StencilDepthFailOp = 1u,
                StencilPassOp = 1u,
                StencilFunc = 8u,       // D3D11_COMPARISON_ALWAYS
            },
            BackFace = new DepthStencilOpDesc
            {
                StencilFailOp = 1u,
                StencilDepthFailOp = 1u,
                StencilPassOp = 1u,
                StencilFunc = 8u,
            },
        };
        void* depthStencilState = _device.CreateDepthStencilState(dsDesc);
        _context.OMSetDepthStencilState(depthStencilState, 0);
        Unknown.Release(depthStencilState);

        // Set rasterizer state with cull mode from pipeline descriptor
        uint d3dCullMode = pipeline.CullMode switch
        {
            GpuCullMode.None => 1u,   // D3D11_CULL_NONE
            GpuCullMode.Front => 2u,  // D3D11_CULL_FRONT
            GpuCullMode.Back => 3u,   // D3D11_CULL_BACK
            _ => 1u,
        };
        var rsDesc = new RasterizerDesc
        {
            FillMode = 3u,   // D3D11_FILL_SOLID
            CullMode = d3dCullMode,
            FrontCounterClockwise = 1, // CCW winding = front-facing (matching Metal/OpenGL convention)
            DepthBias = 0,
            DepthBiasClamp = 0f,
            SlopeScaledDepthBias = 0f,
            DepthClipEnable = 1,
            ScissorEnable = 0,
            MultisampleEnable = 0,
            AntialiasedLineEnable = 0,
        };
        void* rasterizerState = _device.CreateRasterizerState(rsDesc);
        _context.RSSetState(rasterizerState);
        Unknown.Release(rasterizerState);

        // Set primitive topology (triangle list)
        const uint D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST = 4;
        _context.IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
    }

    /// <inheritdoc/>
    public void SetVertexBuffer(void* data, GpuVertexBufferDesc desc)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _currentStride = (uint)desc.Stride;

        // Release previous vertex buffer
        if (_currentVertexBuffer != null)
        {
            Unknown.Release(_currentVertexBuffer);
            _currentVertexBuffer = null;
        }

        // Create a new vertex buffer
        var bufDesc = new BufferDesc
        {
            ByteWidth = (uint)(desc.Stride * desc.VertexCount),
            Usage = Xui.Runtime.Windows.D3D11.Usage.Immutable,
            BindFlags = (uint)BindFlags.VertexBuffer,
            CPUAccessFlags = 0,
            MiscFlags = 0,
            StructureByteStride = 0
        };

        var initData = new SubresourceData
        {
            pSysMem = data,
            SysMemPitch = 0,
            SysMemSlicePitch = 0
        };

        void* buffer = _device.CreateBuffer(bufDesc, &initData);
        _currentVertexBuffer = buffer;

        // Bind vertex buffer
        var stride = _currentStride;
        uint offset = 0;
        _context.IASetVertexBuffers(0, 1, &buffer, &stride, &offset);

        // Create input layout from vertex shader bytecode
        if (_currentVertexShader != null)
        {
            CreateAndSetInputLayout(_currentVertexShader.Bytecode);
        }
    }

    private void CreateAndSetInputLayout(byte[] vertexShaderBytecode)
    {
        // Input layout for CubeVertex: Float3 Position, Color4 (Float4) Color
        // The HLSL semantic names must match what the HLSL generator produces (TEXCOORD0, TEXCOORD1)
        var posSemanticBytes = System.Text.Encoding.ASCII.GetBytes("TEXCOORD\0");
        var colorSemanticBytes = System.Text.Encoding.ASCII.GetBytes("TEXCOORD\0");

        fixed (byte* posName = posSemanticBytes)
        fixed (byte* colorName = colorSemanticBytes)
        {
            var elements = stackalloc InputElementDesc[2];

            // Position: TEXCOORD0, float3 (R32G32B32_FLOAT = 6 in DXGI)
            elements[0] = new InputElementDesc
            {
                SemanticName = posName,
                SemanticIndex = 0,
                Format = DXGI.Format.R32G32B32_FLOAT,
                InputSlot = 0,
                AlignedByteOffset = 0,
                InputSlotClass = 0,  // PER_VERTEX_DATA
                InstanceDataStepRate = 0
            };

            // Color: TEXCOORD1, float4 (R32G32B32A32_FLOAT = 2 in DXGI)
            elements[1] = new InputElementDesc
            {
                SemanticName = colorName,
                SemanticIndex = 1,
                Format = DXGI.Format.R32G32B32A32_FLOAT,
                InputSlot = 0,
                AlignedByteOffset = 12,  // After float3 position
                InputSlotClass = 0,
                InstanceDataStepRate = 0
            };

            void* inputLayout;
            fixed (byte* pBytecode = vertexShaderBytecode)
            {
                inputLayout = _device.CreateInputLayout(elements, 2, pBytecode, (nuint)vertexShaderBytecode.Length);
            }

            _context.IASetInputLayout(inputLayout);
            Unknown.Release(inputLayout);
        }
    }

    /// <inheritdoc/>
    public void SetConstantBuffer(void* data, int sizeInBytes)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Release previous constant buffer
        if (_currentConstantBuffer != null)
        {
            Unknown.Release(_currentConstantBuffer);
            _currentConstantBuffer = null;
        }

        // Constant buffer sizes must be multiples of 16 bytes (Direct3D 11 requirement:
        // D3D11_REQ_CONSTANT_BUFFER_ELEMENT_COUNT per D3D11 spec, CBs are indexed in 16-byte chunks).
        int alignedSize = (sizeInBytes + 15) & ~15;

        var bufDesc = new BufferDesc
        {
            ByteWidth = (uint)alignedSize,
            Usage = Xui.Runtime.Windows.D3D11.Usage.Default,
            BindFlags = (uint)BindFlags.ConstantBuffer,
            CPUAccessFlags = 0,
            MiscFlags = 0,
            StructureByteStride = 0
        };

        void* buffer;

        if (data != null)
        {
            // If we need to pad (align to 16 bytes), allocate a padded copy
            if (alignedSize > sizeInBytes)
            {
                var padded = stackalloc byte[alignedSize];
                Buffer.MemoryCopy(data, padded, alignedSize, sizeInBytes);

                var initData = new SubresourceData
                {
                    pSysMem = padded,
                    SysMemPitch = 0,
                    SysMemSlicePitch = 0
                };
                buffer = _device.CreateBuffer(bufDesc, &initData);
            }
            else
            {
                var initData = new SubresourceData
                {
                    pSysMem = data,
                    SysMemPitch = 0,
                    SysMemSlicePitch = 0
                };
                buffer = _device.CreateBuffer(bufDesc, &initData);
            }
        }
        else
        {
            buffer = _device.CreateBuffer(bufDesc, null);
        }

        _currentConstantBuffer = buffer;

        // Bind to vertex and pixel shader constant buffer slot 0
        _context.VSSetConstantBuffers(0, 1, &buffer);
        _context.PSSetConstantBuffers(0, 1, &buffer);
    }

    /// <inheritdoc/>
    public void Draw(int vertexCount)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _context.DrawPrimitive((uint)vertexCount, 0);
    }

    /// <inheritdoc/>
    public void Commit()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _context.Flush();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_currentVertexBuffer != null)
        {
            Unknown.Release(_currentVertexBuffer);
            _currentVertexBuffer = null;
        }

        if (_currentConstantBuffer != null)
        {
            Unknown.Release(_currentConstantBuffer);
            _currentConstantBuffer = null;
        }
    }
}
