using System.Runtime.InteropServices;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the LineCap canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LineCapCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Value operand.</summary>
    public LineCap Value;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public LineCapCommand() { this = default; Op = CanvasOp.LineCap; }
}
