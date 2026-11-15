using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the Ellipse canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EllipseCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The Center operand.</summary>
    public Point Center;

    /// <summary>The RadiusX operand.</summary>
    public nfloat RadiusX;

    /// <summary>The RadiusY operand.</summary>
    public nfloat RadiusY;

    /// <summary>The Rotation operand.</summary>
    public nfloat Rotation;

    /// <summary>The StartAngle operand.</summary>
    public nfloat StartAngle;

    /// <summary>The EndAngle operand.</summary>
    public nfloat EndAngle;

    /// <summary>The Winding operand.</summary>
    public Winding Winding;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public EllipseCommand() { this = default; Op = CanvasOp.Ellipse; }
}
