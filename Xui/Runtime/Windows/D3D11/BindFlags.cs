using System;

namespace Xui.Runtime.Windows;

public static partial class D3D11
{
    /// <summary>
    /// Identifies how a D3D11 resource is bound to the pipeline.
    /// Mirrors <c>D3D11_BIND_FLAG</c>.
    /// </summary>
    [Flags]
    public enum BindFlags : uint
    {
        VertexBuffer    = 0x1,
        IndexBuffer     = 0x2,
        ConstantBuffer  = 0x4,
        ShaderResource  = 0x8,
        StreamOutput    = 0x10,
        RenderTarget    = 0x20,
        DepthStencil    = 0x40,
        UnorderedAccess = 0x80,
        Decoder         = 0x200,
        VideoEncoder    = 0x400,
    }
}
