using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the ClosePath canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ClosePathCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public ClosePathCommand() { this = default; Op = CanvasOp.ClosePath; }
}
