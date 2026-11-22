using System.Runtime.InteropServices;

namespace Xui.Runtime.Windows;

public static partial class D2D1
{
    /// <summary>ID2D1CommandList: resolution-independent Direct2D drawing instructions.</summary>
    public unsafe class CommandList(void* ptr) : Image(ptr)
    {
        public void Close() => Marshal.ThrowExceptionForHR(
            ((delegate* unmanaged[MemberFunction]<void*, int>)this[5])(this));
    }
}
