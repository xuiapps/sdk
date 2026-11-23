using System;
using System.Runtime.InteropServices;

namespace Xui.Runtime.MacOS;

public partial class Block
{
    public const string SystemLib = "/usr/lib/libSystem.dylib";

    public static readonly nint Lib;

    public static readonly nint NSConcreteStackBlock;
    public static readonly nint NSConcreteGlobalBlock;
    public static readonly nint GlobalBlockDescriptor;
    public static readonly nint StackBlockDescriptor;

    // Native function pointers do not root their managed delegates. These callbacks
    // are referenced by process-lifetime block descriptors and must live just as long.
    private static readonly CopyDelegate CopyCallback = Copy;
    private static readonly InvokeDelegate InvokeCallback = Invoke;
    private static readonly DisposeDelegate DisposeCallback = Dispose;

    public static readonly nint CopyFPtr;

    public static readonly nint InvokeFPtr;

    public static readonly nint DisposeFPtr;

    static unsafe Block()
    {
        Lib = NativeLibrary.Load(SystemLib);

        NSConcreteStackBlock = NativeLibrary.GetExport(Block.Lib, "_NSConcreteStackBlock");
        NSConcreteGlobalBlock = NativeLibrary.GetExport(Block.Lib, "_NSConcreteGlobalBlock");

        CopyFPtr = Marshal.GetFunctionPointerForDelegate(CopyCallback);
        InvokeFPtr = Marshal.GetFunctionPointerForDelegate(InvokeCallback);
        DisposeFPtr = Marshal.GetFunctionPointerForDelegate(DisposeCallback);

        GlobalBlockDescriptor = (nint)NativeMemory.AlignedAlloc((nuint)Marshal.SizeOf<BlockDescriptor>(), 8);
        Marshal.StructureToPtr(new BlockDescriptor() {
            Reserved = 0,
            Size = (uint)Marshal.SizeOf<BlockLiteral>(),
            CopyFPtr = 0,
            DisposeFPtr = 0,
            Signature = 0,
        }, GlobalBlockDescriptor, false);

        StackBlockDescriptor = (nint)NativeMemory.AlignedAlloc((nuint)Marshal.SizeOf<BlockDescriptor>(), 8);
        Marshal.StructureToPtr(new BlockDescriptor() {
            Reserved = 0,
            Size = (uint)Marshal.SizeOf<BlockLiteral>(),
            CopyFPtr = 0, // Marshal.GetFunctionPointerForDelegate(CopyCallback),
            DisposeFPtr = DisposeFPtr,
            Signature = 0,
        }, StackBlockDescriptor, false);
    }

    public delegate void CopyDelegate(ref BlockLiteral dst, ref BlockLiteral src);

    private static void Copy(ref BlockLiteral dst, ref BlockLiteral src)
    {
    }

    public delegate void DisposeDelegate(ref BlockLiteral blockRef);

    private static void Dispose(ref BlockLiteral blockRef) =>
        GCHandle.FromIntPtr(blockRef.Handler).Free();

    public delegate void InvokeDelegate(ref BlockLiteral blockRef);

    private static void Invoke(ref BlockLiteral blockRef) =>
        ((Action)GCHandle.FromIntPtr(blockRef.Handler).Target!)();
}