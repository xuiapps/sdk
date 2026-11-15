using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>The common prefix of every packed canvas command.</summary>
/// <remarks>Read from a command boundary with MemoryMarshal.Read. Records contain no managed
/// references and use process-local nfloat widths; this is not a portable file format.</remarks>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Command
{
    /// <summary>Identifies the concrete header; variable-length headers also specify their payload length.</summary>
    public CanvasOp Op;
}
