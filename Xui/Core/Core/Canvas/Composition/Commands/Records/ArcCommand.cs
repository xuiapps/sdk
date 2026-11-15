using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Arc canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ArcCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Center operand.</summary>
    public Point Center;

    /// <summary>The Radius operand.</summary>
    public nfloat Radius;

    /// <summary>The StartAngle operand.</summary>
    public nfloat StartAngle;

    /// <summary>The EndAngle operand.</summary>
    public nfloat EndAngle;

    /// <summary>The Winding operand.</summary>
    public Winding Winding;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public ArcCommand() { this = default; Op = CanvasOp.Arc; }
}
