using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Cubic canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CubicCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Control1 operand.</summary>
    public Point Control1;

    /// <summary>The Control2 operand.</summary>
    public Point Control2;

    /// <summary>The End operand.</summary>
    public Point End;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public CubicCommand() { this = default; Op = CanvasOp.Cubic; }
}
