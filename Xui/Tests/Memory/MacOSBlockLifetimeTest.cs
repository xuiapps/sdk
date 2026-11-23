using System.Runtime.InteropServices;
using Xui.Runtime.MacOS;
using CoreGraphicsFact = Xui.Tests.Component.IOS.IOSDrawingContextTest.CoreGraphicsFactAttribute;

namespace Xui.Tests.Memory;

public sealed partial class MacOSBlockLifetimeTest
{
    [CoreGraphicsFact]
    public unsafe void NativeBlockCallbacksRemainAliveAcrossCollections()
    {
        bool invoked = false;
        var callback = GCHandle.Alloc((Action)(() => invoked = true));
        var literal = new Block.BlockLiteral
        {
            Isa = Block.NSConcreteStackBlock,
            Flags = (int)Block.BlockFlags.HasCopyDispose,
            FunctionPtr = Block.InvokeFPtr,
            BlockDescriptor = Block.StackBlockDescriptor,
            Handler = GCHandle.ToIntPtr(callback),
        };
        var owned = Copy((nint)(&literal));
        if (owned == 0) { callback.Free(); Assert.Fail("Could not copy the native block."); }
        try
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var invoke = (delegate* unmanaged<nint, void>)((Block.BlockLiteral*)owned)->FunctionPtr;
            invoke(owned);
            Assert.True(invoked);
        }
        finally { Release(owned); } // Native dispose also releases the GCHandle.
    }

    [LibraryImport(Block.SystemLib, EntryPoint = "_Block_copy")]
    private static partial nint Copy(nint block);

    [LibraryImport(Block.SystemLib, EntryPoint = "_Block_release")]
    private static partial void Release(nint block);
}
