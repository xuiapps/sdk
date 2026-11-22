namespace Xui.Compiler.XuiNet.Layout;

/// <summary>Target storage rules for the scalar and unmanaged struct layouts currently supported.</summary>
/// <remarks>These profiles describe data layout, not a complete calling convention or object model.</remarks>
public sealed class TargetLayout
{
    private TargetLayout(string name, int pointerSize)
    {
        Name = name;
        PointerSize = pointerSize;
    }

    /// <summary>WASM32 storage: four-byte pointers and NFloat, eight-byte default packing.</summary>
    public static TargetLayout Wasm32 { get; } = new("WASM32", 4);
    /// <summary>64-bit scalar storage: eight-byte pointers and NFloat, eight-byte default packing.</summary>
    public static TargetLayout X64 { get; } = new("X64", 8);
    /// <summary>The target name used in diagnostics and snapshots.</summary>
    public string Name { get; }
    /// <summary>The storage size and alignment of native integers.</summary>
    public int PointerSize { get; }
    /// <summary>The storage size and alignment of System.Runtime.InteropServices.NFloat.</summary>
    public int NativeFloatSize => PointerSize;
    /// <summary>The alignment ceiling used when metadata specifies Pack = 0.</summary>
    public int DefaultPackingSize => 8;
}
