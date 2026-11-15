using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Transform canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TransformCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Value operand.</summary>
    public AffineTransform Value;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public TransformCommand() { this = default; Op = CanvasOp.Transform; }
}
