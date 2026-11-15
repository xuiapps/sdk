using System.Runtime.InteropServices;
using Xui.Core.Math2D;

namespace Xui.Core.Canvas.Composition.Commands.Records;

/// <summary>Packed operands for the FillRadial canvas operation.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FillRadialCommand
{
    /// <summary>The command discriminator, at byte offset zero.</summary>
    public CanvasOp Op;

    /// <summary>The StartCenter operand.</summary>
    public Point StartCenter;

    /// <summary>The StartRadius operand.</summary>
    public nfloat StartRadius;

    /// <summary>The EndCenter operand.</summary>
    public Point EndCenter;

    /// <summary>The EndRadius operand.</summary>
    public nfloat EndRadius;

    /// <summary>Number of GradientStop elements stored immediately after this record.</summary>
    public int Length;

    /// <summary>Initializes the discriminator and zeroes the operands.</summary>
    public FillRadialCommand() { this = default; Op = CanvasOp.FillRadial; }
}
