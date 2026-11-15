using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the ArcTo canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ArcToCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Control operand.</summary>
    public Point Control;

    /// <summary>The End operand.</summary>
    public Point End;

    /// <summary>The Radius operand.</summary>
    public nfloat Radius;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public ArcToCommand() { this = default; Op = CanvasOp.ArcTo; }
}
