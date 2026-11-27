namespace Xui.Compiler.Runtime.Memory;

/// <summary>Explicit unmanaged allocations in the module's growable WASM32 memory.</summary>
public static unsafe class NativeMemory
{
    public static void* Alloc(nuint byteCount) => Allocate(byteCount, 1, false);
    public static void* Alloc(nuint elementCount, nuint elementSize) => Allocate(elementCount, elementSize, false);
    public static void* AllocZeroed(nuint byteCount) => Allocate(byteCount, 1, true);
    public static void* AllocZeroed(nuint elementCount, nuint elementSize) => Allocate(elementCount, elementSize, true);
    public static void Free(void* pointer) => Release((nuint)pointer);

    private static void* Allocate(nuint count, nuint size, bool zero)
    {
        nuint pointer = Reserve(count, size, zero);
        if (pointer == 0) throw new OutOfMemoryException();
        return (void*)pointer;
    }

    [RuntimeImport("xui-memory", "allocate")]
    private static nuint Reserve(nuint count, nuint size, bool zero) => throw new NotSupportedException();
    [RuntimeImport("xui-memory", "free")]
    private static void Release(nuint pointer) => throw new NotSupportedException();
}
