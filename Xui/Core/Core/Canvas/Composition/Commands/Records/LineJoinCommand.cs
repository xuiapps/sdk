using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the LineJoin canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LineJoinCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Value operand.</summary>
    public LineJoin Value;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public LineJoinCommand() { this = default; Op = CanvasOp.LineJoin; }
}
