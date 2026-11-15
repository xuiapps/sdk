using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the RoundRect canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RoundRectCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Bounds operand.</summary>
    public Rect Bounds;

    /// <summary>The Corners operand.</summary>
    public CornerRadius Corners;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public RoundRectCommand() { this = default; Op = CanvasOp.RoundRect; }
}
