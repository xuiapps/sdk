using System;
using System.Runtime.InteropServices;
using static Xui.Runtime.Windows.COM;

namespace Xui.Runtime.Windows;

/// <summary>
/// Code from &lt;d3d11.h&gt; in the d3d11.dll lib.
/// </summary>
public static partial class D3D11
{
    public unsafe class Device : Unknown
    {
        public static new readonly Guid IID = new Guid("db6f6ddb-ac77-4e88-8253-819df9bbf140");

        public Device(void* ptr) : base(ptr)
        {
        }

        /// <summary>
        /// Creates a 2D texture resource.
        /// Wraps <c>ID3D11Device::CreateTexture2D</c> (vtable [5]).
        /// </summary>
        public Texture2D CreateTexture2D(in Texture2DDesc desc, in SubresourceData initialData)
        {
            void* texture;
            fixed (Texture2DDesc* descPtr = &desc)
            fixed (SubresourceData* dataPtr = &initialData)
            {
                Marshal.ThrowExceptionForHR(
                    ((delegate* unmanaged[MemberFunction]<void*, Texture2DDesc*, SubresourceData*, void**, int>)this[5])
                    (this, descPtr, dataPtr, &texture));
            }
            return new Texture2D(texture);
        }
    }
}
