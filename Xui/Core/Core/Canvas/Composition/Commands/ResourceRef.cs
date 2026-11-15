using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands;

/// <summary>An ID for a managed object retained by a canvas command buffer.</summary>
/// <remarks>IDs belong to the buffer that recorded them. CopyFrom preserves their mapping;
/// clearing or recording into that buffer invalidates previously borrowed IDs.
/// The buffer retains references but does not dispose the resources.</remarks>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ResourceRef
{
    /// <summary>Zero-based index in the owning buffer's shared resource list.</summary>
    public int Id;

    /// <summary>Creates a reference to an entry in the owning buffer.</summary>
    public ResourceRef(int id) => Id = id;
}
