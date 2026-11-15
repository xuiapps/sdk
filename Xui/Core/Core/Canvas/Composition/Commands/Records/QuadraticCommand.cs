using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Quadratic canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct QuadraticCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Control operand.</summary>
    public Point Control;

    /// <summary>The End operand.</summary>
    public Point End;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public QuadraticCommand() { this = default; Op = CanvasOp.Quadratic; }
}
