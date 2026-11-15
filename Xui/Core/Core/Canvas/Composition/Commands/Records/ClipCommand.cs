using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Clip canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ClipCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public ClipCommand() { this = default; Op = CanvasOp.Clip; }
}
