using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Rect canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RectCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Bounds operand.</summary>
    public Rect Bounds;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public RectCommand() { this = default; Op = CanvasOp.Rect; }
}
